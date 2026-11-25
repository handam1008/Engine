using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Papyrus.Ide.Editor
{
    /// <summary>
    /// PAPyrus' Inspector / Hierarchy window, served from the live Unity Editor.
    /// Requests come through <see cref="PapyrusBridge"/> as "inspector.*" commands with an id and are
    /// answered with {"type":"reply","id":..,"ok":..}. Edits go through SerializedObject / Undo, so
    /// they behave exactly like edits made in Unity's own Inspector (undo, dirty scenes, prefabs).
    ///   Unity -> IDE pushes: inspector.changed (anything edited), inspector.selection (Unity selection)
    /// </summary>
    [InitializeOnLoad]
    internal static class PapyrusInspector
    {
        const int k_MaxNodes = 5000;      // hierarchy objects sent per request
        const int k_MaxProps = 600;       // serialized properties per component
        const int k_MaxArray = 100;       // array elements expanded per array
        const int k_MaxDepth = 6;         // nested struct / array depth
        const int k_MaxString = 4000;     // characters of a string value
        const double k_ChangeDelay = 0.25; // seconds of quiet before "inspector.changed"

        static double s_ChangedAt = -1;
        static bool s_SelectionDirty;

        static PapyrusInspector()
        {
            if (Application.isBatchMode)
                return;
            EditorApplication.hierarchyChanged += MarkChanged;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
            Undo.undoRedoPerformed += MarkChanged;
            EditorSceneManager.sceneOpened += (s, m) => MarkChanged();
            EditorSceneManager.sceneClosed += s => MarkChanged();
            Selection.selectionChanged += () => s_SelectionDirty = true;
            EditorApplication.update += Update;
        }

        static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            MarkChanged();
        }

        static void MarkChanged()
        {
            s_ChangedAt = EditorApplication.timeSinceStartup;
        }

        static void Update()
        {
            if (PapyrusBridge.ConnectedClients == 0)
                return;
            if (s_ChangedAt >= 0 && EditorApplication.timeSinceStartup - s_ChangedAt > k_ChangeDelay)
            {
                s_ChangedAt = -1;
                PapyrusBridge.Send("{\"type\":\"inspector.changed\"}");
            }
            if (s_SelectionDirty)
            {
                s_SelectionDirty = false;
                var o = Selection.activeObject;
                PapyrusBridge.Send("{\"type\":\"inspector.selection\",\"target\":" + PapyrusIds.Of(o) +
                                   ",\"kind\":" + Json.Str(KindOf(o)) +
                                   ",\"assetPath\":" + Json.Str(o ? AssetDatabase.GetAssetPath(o) : "") + "}");
            }
        }

        // ------------------------------------------------------------------------------ dispatch

        /// <summary>Handles an "inspector.*" command. Returns false for anything else.</summary>
        internal static bool Handle(PapyrusBridge.Command cmd)
        {
            if (cmd.type == null || !cmd.type.StartsWith("inspector.", StringComparison.Ordinal))
                return false;
            string body;
            try
            {
                body = Dispatch(cmd);
            }
            catch (NotReadyException e)
            {
                // the IDE retries these once Unity has compiled / imported the script
                body = Fail(e.Message) + ",\"code\":\"notReady\"";
            }
            catch (Exception e)
            {
                body = Fail(e.Message);
            }
            // id 0 = fire-and-forget (mirror input events), no reply
            if (cmd.id != 0)
                PapyrusBridge.Send("{\"type\":\"reply\",\"id\":" + cmd.id + "," + body + "}");
            return true;
        }

        static string Ok(string data)
        {
            return "\"ok\":true,\"playMode\":" + Json.Bool(EditorApplication.isPlaying) + ",\"data\":" + (data ?? "null");
        }

        static string Fail(string message)
        {
            return "\"ok\":false,\"error\":" + Json.Str(message);
        }

        static string Dispatch(PapyrusBridge.Command cmd)
        {
            switch (cmd.type)
            {
                case "inspector.hierarchy": return Ok(Hierarchy());
                case "inspector.object": return ObjectReply(Find(cmd.target));
                case "inspector.asset": return AssetReply(cmd.path);
                case "inspector.scripts": return Ok(Scripts());
                case "inspector.addComponent": return AddComponent(cmd);
                case "inspector.removeComponent": return RemoveComponent(cmd);
                case "inspector.setProperty": return SetProperty(cmd);
                case "inspector.setActive": return SetActive(cmd);
                case "inspector.rename": return Rename(cmd);
                case "inspector.delete": return DeleteObject(cmd);
                case "inspector.undo":
                    Undo.PerformUndo();
                    return Ok(null);
                case "inspector.createObject": return CreateObject(cmd);
                case "inspector.createAsset": return CreateAsset(cmd);
                case "inspector.select": return SelectInUnity(cmd);
                case "inspector.mirrorOpen": return MirrorOpen(cmd);
                case "inspector.mirrorResize":
                    PapyrusInspectorMirror.Resize(cmd.width, cmd.height);
                    return Ok(null);
                case "inspector.mirrorPlace":
                    PapyrusInspectorMirror.PlaceOverIde(cmd);
                    return Ok(null);
                case "inspector.mirrorInput":
                {
                    var err = PapyrusInspectorMirror.Input(cmd);
                    return err == null ? Ok(null) : Fail(err);
                }
                case "inspector.mirrorClose":
                    PapyrusInspectorMirror.Close();
                    return Ok(null);
                default: return Fail("알 수 없는 명령: " + cmd.type);
            }
        }

        static Object Find(int id) => PapyrusIds.Find(id); // instance ids before Unity 6.5, see PapyrusIds

        static string KindOf(Object o)
        {
            if (!o) return "";
            if (o is GameObject) return "GameObject";
            if (o is ScriptableObject) return "ScriptableObject";
            return o.GetType().Name;
        }

        static string BusyReason()
        {
            if (EditorApplication.isCompiling) return "Unity가 컴파일 중이에요. 끝난 뒤 다시 시도하세요.";
            if (EditorApplication.isUpdating) return "Unity가 에셋을 가져오는 중이에요. 끝난 뒤 다시 시도하세요.";
            return null;
        }

        static void MarkDirty(Object o)
        {
            if (!o) return;
            EditorUtility.SetDirty(o);
            GameObject go = o as GameObject;
            if (!go && o is Component) go = ((Component)o).gameObject;
            if (go && go.scene.IsValid() && !EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(go.scene);
        }

        // ------------------------------------------------------------------------------ hierarchy

        static string Hierarchy()
        {
            var sb = new StringBuilder("{\"scenes\":[");
            var count = 0;
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (i > 0) sb.Append(',');
                sb.Append("{\"name\":").Append(Json.Str(string.IsNullOrEmpty(scene.name) ? "제목 없음" : scene.name))
                  .Append(",\"path\":").Append(Json.Str(scene.path))
                  .Append(",\"loaded\":").Append(Json.Bool(scene.isLoaded))
                  .Append(",\"dirty\":").Append(Json.Bool(scene.isDirty))
                  .Append(",\"active\":").Append(Json.Bool(scene == SceneManager.GetActiveScene()))
                  .Append(",\"roots\":[");
                if (scene.isLoaded)
                {
                    var roots = scene.GetRootGameObjects();
                    for (var r = 0; r < roots.Length; r++)
                    {
                        if (r > 0) sb.Append(',');
                        AppendNode(sb, roots[r].transform, ref count);
                    }
                }
                sb.Append("]}");
            }
            sb.Append("],\"truncated\":").Append(Json.Bool(count >= k_MaxNodes)).Append('}');
            return sb.ToString();
        }

        static void AppendNode(StringBuilder sb, Transform t, ref int count)
        {
            count++;
            var go = t.gameObject;
            sb.Append("{\"id\":").Append(PapyrusIds.Of(go))
              .Append(",\"name\":").Append(Json.Str(go.name))
              .Append(",\"active\":").Append(Json.Bool(go.activeSelf))
              .Append(",\"prefab\":").Append(Json.Bool(PrefabUtility.IsPartOfPrefabInstance(go) && PrefabUtility.IsOutermostPrefabInstanceRoot(go)))
              .Append(",\"children\":[");
            var n = t.childCount;
            var written = 0;
            for (var i = 0; i < n; i++)
            {
                if (count >= k_MaxNodes) break;
                if (written++ > 0) sb.Append(',');
                AppendNode(sb, t.GetChild(i), ref count);
            }
            sb.Append("]");
            if (written < n) sb.Append(",\"more\":").Append(n - written);
            sb.Append('}');
        }

        // ------------------------------------------------------------------------------ inspector

        static string ObjectReply(Object o)
        {
            if (!o) return Fail("오브젝트를 찾지 못했어요. 씬이 바뀌었거나 삭제됐을 수 있어요.");
            return Ok(ObjectJson(o));
        }

        static string AssetReply(string path)
        {
            if (string.IsNullOrEmpty(path)) return Fail("에셋 경로가 비어 있어요.");
            var o = AssetDatabase.LoadMainAssetAtPath(path);
            if (!o) return Fail("에셋을 찾지 못했어요: " + path + " (Unity가 아직 가져오지 않았을 수 있어요)");
            return Ok(ObjectJson(o));
        }

        static string ObjectJson(Object o)
        {
            var sb = new StringBuilder("{");
            sb.Append("\"id\":").Append(PapyrusIds.Of(o))
              .Append(",\"name\":").Append(Json.Str(o.name))
              .Append(",\"kind\":").Append(Json.Str(KindOf(o)))
              .Append(",\"assetPath\":").Append(Json.Str(AssetDatabase.GetAssetPath(o)));
            var go = o as GameObject;
            if (go)
            {
                sb.Append(",\"active\":").Append(Json.Bool(go.activeSelf))
                  .Append(",\"tag\":").Append(Json.Str(go.tag))
                  .Append(",\"layer\":").Append(Json.Str(LayerMask.LayerToName(go.layer)))
                  .Append(",\"isStatic\":").Append(Json.Bool(go.isStatic))
                  .Append(",\"scene\":").Append(Json.Str(go.scene.IsValid() ? go.scene.name : ""))
                  .Append(",\"components\":[");
                var comps = go.GetComponents<Component>();
                for (var i = 0; i < comps.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    if (comps[i] == null)
                        sb.Append("{\"id\":0,\"type\":\"(스크립트 없음)\",\"missing\":true,\"props\":[]}");
                    else
                        AppendComponent(sb, comps[i]);
                }
                sb.Append(']');
            }
            else
            {
                sb.Append(",\"components\":[");
                AppendComponent(sb, o);
                sb.Append(']');
            }
            sb.Append('}');
            return sb.ToString();
        }

        static void AppendComponent(StringBuilder sb, Object c)
        {
            MonoScript script = null;
            if (c is MonoBehaviour) script = MonoScript.FromMonoBehaviour((MonoBehaviour)c);
            else if (c is ScriptableObject) script = MonoScript.FromScriptableObject((ScriptableObject)c);
            var behaviour = c as Behaviour;
            sb.Append("{\"id\":").Append(PapyrusIds.Of(c))
              .Append(",\"type\":").Append(Json.Str(ObjectNames.NicifyVariableName(c.GetType().Name)))
              .Append(",\"typeName\":").Append(Json.Str(c.GetType().FullName))
              .Append(",\"script\":").Append(Json.Str(script ? AssetDatabase.GetAssetPath(script) : null))
              .Append(",\"enabled\":").Append(behaviour ? Json.Bool(behaviour.enabled) : "null")
              .Append(",\"canRemove\":").Append(Json.Bool(c is Component && !(c is Transform)))
              .Append(",\"props\":[");
            AppendProps(sb, c);
            sb.Append("]}");
        }

        static void AppendProps(StringBuilder sb, Object c)
        {
            var so = new SerializedObject(c);
            var it = so.GetIterator();
            var enter = true;
            var count = 0;
            var first = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.propertyPath == "m_Script") continue;
                if (it.propertyPath.EndsWith(".Array.size", StringComparison.Ordinal)) continue;
                if (count++ >= k_MaxProps) break;

                if (!first) sb.Append(',');
                first = false;
                AppendProp(sb, it);

                // descend into structs and arrays (not strings, which are arrays of char)
                var generic = it.propertyType == SerializedPropertyType.Generic;
                if (generic && it.hasVisibleChildren && it.depth < k_MaxDepth && (!it.isArray || it.arraySize <= k_MaxArray))
                    enter = true;
            }
        }

        static void AppendProp(StringBuilder sb, SerializedProperty p)
        {
            sb.Append("{\"path\":").Append(Json.Str(p.propertyPath))
              .Append(",\"label\":").Append(Json.Str(p.displayName))
              .Append(",\"depth\":").Append(p.depth)
              .Append(",\"tooltip\":").Append(Json.Str(string.IsNullOrEmpty(p.tooltip) ? null : p.tooltip));
            var editable = p.editable;
            string kind;
            string value;
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer:
                    kind = "int"; value = p.longValue.ToString(CultureInfo.InvariantCulture); break;
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    kind = "int"; value = p.intValue.ToString(CultureInfo.InvariantCulture); break;
                case SerializedPropertyType.Boolean:
                    kind = "bool"; value = Json.Bool(p.boolValue); break;
                case SerializedPropertyType.Float:
                    kind = "float"; value = Num(p.doubleValue); break;
                case SerializedPropertyType.String:
                    kind = "string";
                    var s = p.stringValue ?? "";
                    value = Json.Str(s.Length > k_MaxString ? s.Substring(0, k_MaxString) : s);
                    break;
                case SerializedPropertyType.Enum:
                    kind = "enum";
                    value = "{\"index\":" + p.enumValueIndex + ",\"names\":" + StrArray(p.enumDisplayNames) + "}";
                    break;
                case SerializedPropertyType.Color:
                    kind = "color"; var col = p.colorValue;
                    value = NumArray(col.r, col.g, col.b, col.a); break;
                case SerializedPropertyType.Vector2:
                    kind = "vector"; value = NumArray(p.vector2Value.x, p.vector2Value.y); break;
                case SerializedPropertyType.Vector3:
                    kind = "vector"; value = NumArray(p.vector3Value.x, p.vector3Value.y, p.vector3Value.z); break;
                case SerializedPropertyType.Vector4:
                    kind = "vector"; var v4 = p.vector4Value; value = NumArray(v4.x, v4.y, v4.z, v4.w); break;
                case SerializedPropertyType.Vector2Int:
                    kind = "vectorInt"; value = NumArray(p.vector2IntValue.x, p.vector2IntValue.y); break;
                case SerializedPropertyType.Vector3Int:
                    kind = "vectorInt"; var v3i = p.vector3IntValue; value = NumArray(v3i.x, v3i.y, v3i.z); break;
                case SerializedPropertyType.Quaternion:
                    // shown like Unity's Inspector: as euler angles
                    kind = "euler"; var e = p.quaternionValue.eulerAngles; value = NumArray(e.x, e.y, e.z); break;
                case SerializedPropertyType.Rect:
                    kind = "vector"; var r = p.rectValue; value = NumArray(r.x, r.y, r.width, r.height); break;
                case SerializedPropertyType.ObjectReference:
                    kind = "object";
                    var o = p.objectReferenceValue;
                    value = o
                        ? "{\"id\":" + PapyrusIds.Of(o) + ",\"name\":" + Json.Str(o.name) + ",\"type\":" + Json.Str(o.GetType().Name) +
                          ",\"assetPath\":" + Json.Str(AssetDatabase.GetAssetPath(o)) + "}"
                        : "null";
                    sb.Append(",\"refType\":").Append(Json.Str(RefTypeName(p)));
                    break;
                case SerializedPropertyType.Generic:
                    if (p.isArray)
                    {
                        kind = "array"; value = p.arraySize.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        kind = "struct"; value = "null";
                    }
                    break;
                default:
                    kind = "other"; value = Json.Str(p.propertyType.ToString()); editable = false; break;
            }
            sb.Append(",\"type\":").Append(Json.Str(kind))
              .Append(",\"editable\":").Append(Json.Bool(editable))
              .Append(",\"value\":").Append(value)
              .Append('}');
        }

        static string RefTypeName(SerializedProperty p)
        {
            // "PPtr<$Rigidbody2D>" -> "Rigidbody2D"
            var t = p.type ?? "";
            var a = t.IndexOf('<');
            var b = t.LastIndexOf('>');
            return a >= 0 && b > a ? t.Substring(a + 1, b - a - 1).TrimStart('$') : t;
        }

        static string Num(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d)) return "null";
            return d.ToString("R", CultureInfo.InvariantCulture);
        }

        static string Num(float f)
        {
            if (float.IsNaN(f) || float.IsInfinity(f)) return "null";
            return f.ToString("R", CultureInfo.InvariantCulture); // 0.1f -> "0.1", not 0.100000001490116
        }

        static string NumArray(params float[] v)
        {
            var sb = new StringBuilder("[");
            for (var i = 0; i < v.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(Num(v[i]));
            }
            return sb.Append(']').ToString();
        }

        static string NumArray(params int[] v)
        {
            var sb = new StringBuilder("[");
            for (var i = 0; i < v.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(v[i].ToString(CultureInfo.InvariantCulture));
            }
            return sb.Append(']').ToString();
        }

        static string StrArray(string[] v)
        {
            var sb = new StringBuilder("[");
            for (var i = 0; i < v.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(Json.Str(v[i]));
            }
            return sb.Append(']').ToString();
        }

        // ------------------------------------------------------------------------------ scripts

        static string Scripts()
        {
            var sb = new StringBuilder("[");
            var first = true;
            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var t = ms ? ms.GetClass() : null;
                var kind = ScriptKind(t);
                if (kind == null) continue;
                if (!first) sb.Append(',');
                first = false;
                sb.Append("{\"name\":").Append(Json.Str(t.Name))
                  .Append(",\"fullName\":").Append(Json.Str(t.FullName))
                  .Append(",\"path\":").Append(Json.Str(path))
                  .Append(",\"kind\":").Append(Json.Str(kind)).Append('}');
            }
            return sb.Append(']').ToString();
        }

        /// <summary>"mono" (can be added to a GameObject), "so" (can become an asset) or null.</summary>
        static string ScriptKind(Type t)
        {
            if (t == null || t.IsAbstract || t.IsGenericTypeDefinition) return null;
            if (typeof(MonoBehaviour).IsAssignableFrom(t)) return "mono";
            if (typeof(ScriptableObject).IsAssignableFrom(t) && !typeof(UnityEditor.Editor).IsAssignableFrom(t) &&
                !typeof(EditorWindow).IsAssignableFrom(t))
                return "so";
            return null;
        }

        sealed class NotReadyException : Exception
        {
            public NotReadyException(string message) : base(message) { }
        }

        /// <summary>
        /// The compiled class of a script asset. Throws <see cref="NotReadyException"/> while Unity is
        /// compiling / importing or hasn't imported the file yet; returns null with an error otherwise.
        /// </summary>
        static Type ScriptType(string path, out string error)
        {
            error = null;
            var busy = BusyReason();
            if (busy != null) throw new NotReadyException(busy);
            var ms = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (!ms)
                throw new NotReadyException("Unity가 아직 '" + path + "'을(를) 가져오지 않았어요.");
            var t = ms.GetClass();
            if (t == null)
                error = "'" + ms.name + "' 클래스를 찾지 못했어요. 컴파일 에러가 있거나 파일 이름과 클래스 이름이 달라요.";
            return t;
        }

        // ------------------------------------------------------------------------------ edits

        static string AddComponent(PapyrusBridge.Command cmd)
        {
            var go = Find(cmd.target) as GameObject;
            if (!go) return Fail("붙일 오브젝트를 찾지 못했어요.");
            string error;
            var t = ScriptType(cmd.script, out error);
            if (t == null) return Fail(error);
            if (ScriptKind(t) != "mono") return Fail("'" + t.Name + "'은(는) MonoBehaviour가 아니라서 오브젝트에 붙일 수 없어요.");
            var c = Undo.AddComponent(go, t);
            if (!c) return Fail("'" + t.Name + "'을(를) 붙이지 못했어요. 이미 있거나(DisallowMultipleComponent) 필요한 조건이 맞지 않아요.");
            MarkDirty(go);
            return Ok("{\"component\":" + PapyrusIds.Of(c) + "}");
        }

        static string RemoveComponent(PapyrusBridge.Command cmd)
        {
            var c = Find(cmd.target) as Component;
            if (!c) return Fail("컴포넌트를 찾지 못했어요.");
            if (c is Transform) return Fail("Transform은 지울 수 없어요.");
            var go = c.gameObject;
            Undo.DestroyObjectImmediate(c);
            MarkDirty(go);
            return Ok(null);
        }

        static string SetActive(PapyrusBridge.Command cmd)
        {
            var go = Find(cmd.target) as GameObject;
            if (!go) return Fail("오브젝트를 찾지 못했어요.");
            Undo.RecordObject(go, "Set Active");
            go.SetActive(cmd.value == "true");
            MarkDirty(go);
            return Ok(null);
        }

        /// <summary>Delete a scene object the way Unity's hierarchy does (undoable).</summary>
        static string DeleteObject(PapyrusBridge.Command cmd)
        {
            var go = Find(cmd.target) as GameObject;
            if (!go) return Fail("오브젝트를 찾지 못했어요.");
            if (EditorUtility.IsPersistent(go)) return Fail("에셋 안의 오브젝트는 여기서 지울 수 없어요.");
            if (PrefabUtility.IsPartOfPrefabInstance(go) && !PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                return Fail("프리팹 인스턴스 안의 오브젝트는 지울 수 없어요. 프리팹을 열거나 Unpack 한 뒤에 지워 주세요.");
            MarkDirty(go);
            Undo.DestroyObjectImmediate(go);
            return Ok(null);
        }

        static string Rename(PapyrusBridge.Command cmd)
        {
            var o = Find(cmd.target);
            if (!o) return Fail("오브젝트를 찾지 못했어요.");
            if (string.IsNullOrEmpty(cmd.name)) return Fail("이름이 비어 있어요.");
            var path = AssetDatabase.GetAssetPath(o);
            if (!string.IsNullOrEmpty(path) && AssetDatabase.IsMainAsset(o))
            {
                var err = AssetDatabase.RenameAsset(path, cmd.name);
                if (!string.IsNullOrEmpty(err)) return Fail(err);
            }
            else
            {
                Undo.RecordObject(o, "Rename");
                o.name = cmd.name;
                MarkDirty(o);
            }
            return Ok(null);
        }

        static string SetProperty(PapyrusBridge.Command cmd)
        {
            var o = Find(cmd.target);
            if (!o) return Fail("대상을 찾지 못했어요.");
            var so = new SerializedObject(o);
            var p = so.FindProperty(cmd.path);
            if (p == null) return Fail("속성을 찾지 못했어요: " + cmd.path);
            if (!p.editable) return Fail("이 값은 바꿀 수 없어요.");
            var v = cmd.value ?? "";
            var inv = CultureInfo.InvariantCulture;
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer:
                    p.longValue = (long)Math.Round(double.Parse(v, inv)); break;
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    p.intValue = (int)Math.Round(double.Parse(v, inv)); break;
                case SerializedPropertyType.Boolean:
                    p.boolValue = v == "true"; break;
                case SerializedPropertyType.Float:
                    p.doubleValue = double.Parse(v, inv); break;
                case SerializedPropertyType.String:
                    p.stringValue = v; break;
                case SerializedPropertyType.Enum:
                    p.enumValueIndex = int.Parse(v, inv); break;
                case SerializedPropertyType.Color:
                {
                    var f = Floats(v, 4);
                    p.colorValue = new Color(f[0], f[1], f[2], f[3]);
                    break;
                }
                case SerializedPropertyType.Vector2:
                {
                    var f = Floats(v, 2);
                    p.vector2Value = new Vector2(f[0], f[1]);
                    break;
                }
                case SerializedPropertyType.Vector3:
                {
                    var f = Floats(v, 3);
                    p.vector3Value = new Vector3(f[0], f[1], f[2]);
                    break;
                }
                case SerializedPropertyType.Vector4:
                {
                    var f = Floats(v, 4);
                    p.vector4Value = new Vector4(f[0], f[1], f[2], f[3]);
                    break;
                }
                case SerializedPropertyType.Vector2Int:
                {
                    var f = Floats(v, 2);
                    p.vector2IntValue = new Vector2Int(Mathf.RoundToInt(f[0]), Mathf.RoundToInt(f[1]));
                    break;
                }
                case SerializedPropertyType.Vector3Int:
                {
                    var f = Floats(v, 3);
                    p.vector3IntValue = new Vector3Int(Mathf.RoundToInt(f[0]), Mathf.RoundToInt(f[1]), Mathf.RoundToInt(f[2]));
                    break;
                }
                case SerializedPropertyType.Quaternion:
                {
                    var f = Floats(v, 3);
                    p.quaternionValue = Quaternion.Euler(f[0], f[1], f[2]);
                    break;
                }
                case SerializedPropertyType.Rect:
                {
                    var f = Floats(v, 4);
                    p.rectValue = new Rect(f[0], f[1], f[2], f[3]);
                    break;
                }
                case SerializedPropertyType.ObjectReference:
                {
                    // value = instance id, or an asset path, or "" for None
                    Object target = null;
                    int refId;
                    if (int.TryParse(v, NumberStyles.Integer, inv, out refId)) target = Find(refId);
                    else if (v.Length > 0) target = AssetDatabase.LoadMainAssetAtPath(v);
                    if (v.Length > 0 && !target) return Fail("연결할 대상을 찾지 못했어요: " + v);
                    p.objectReferenceValue = target;
                    if (target && p.objectReferenceValue != target)
                        return Fail("'" + target.name + "'(" + target.GetType().Name + ")은(는) 이 필드에 넣을 수 없어요.");
                    break;
                }
                default:
                    return Fail("이 형식은 아직 PAPyrus에서 바꿀 수 없어요: " + p.propertyType);
            }
            so.ApplyModifiedProperties(); // records Undo like Unity's Inspector
            MarkDirty(o);
            return Ok(null);
        }

        static float[] Floats(string v, int n)
        {
            var parts = v.Split(',');
            var f = new float[n];
            for (var i = 0; i < n; i++)
                f[i] = i < parts.Length ? float.Parse(parts[i].Trim(), CultureInfo.InvariantCulture) : 0f;
            return f;
        }

        static string CreateObject(PapyrusBridge.Command cmd)
        {
            var parent = Find(cmd.parent) as GameObject;
            Type t = null;
            if (!string.IsNullOrEmpty(cmd.script))
            {
                string error;
                t = ScriptType(cmd.script, out error);
                if (t == null) return Fail(error);
                if (ScriptKind(t) != "mono") return Fail("'" + t.Name + "'은(는) MonoBehaviour가 아니라서 오브젝트에 붙일 수 없어요.");
            }
            var name = string.IsNullOrEmpty(cmd.name) ? (t != null ? t.Name : "GameObject") : cmd.name;
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            if (parent)
            {
                Undo.SetTransformParent(go.transform, parent.transform, "Parent " + name);
                go.layer = parent.layer;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            if (t != null) Undo.AddComponent(go, t);
            MarkDirty(go);
            Selection.activeGameObject = go;
            return Ok("{\"id\":" + PapyrusIds.Of(go) + "}");
        }

        static string CreateAsset(PapyrusBridge.Command cmd)
        {
            string error;
            var t = ScriptType(cmd.script, out error);
            if (t == null) return Fail(error);
            if (ScriptKind(t) != "so") return Fail("'" + t.Name + "'은(는) ScriptableObject가 아니라서 에셋으로 만들 수 없어요.");
            var dir = string.IsNullOrEmpty(cmd.path) ? System.IO.Path.GetDirectoryName(cmd.script) : cmd.path;
            dir = (dir ?? "Assets").Replace('\\', '/');
            var path = AssetDatabase.GenerateUniqueAssetPath(dir + "/" + (string.IsNullOrEmpty(cmd.name) ? t.Name : cmd.name) + ".asset");
            var asset = ScriptableObject.CreateInstance(t);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            return Ok("{\"id\":" + PapyrusIds.Of(asset) + ",\"path\":" + Json.Str(path) + "}");
        }

        static string MirrorOpen(PapyrusBridge.Command cmd)
        {
            var o = Find(cmd.target);
            if (!o && !string.IsNullOrEmpty(cmd.path)) o = AssetDatabase.LoadMainAssetAtPath(cmd.path);
            if (!o)
            {
                var busy = BusyReason();
                if (busy != null) throw new NotReadyException(busy);
                return Fail("오브젝트를 찾지 못했어요.");
            }
            var err = PapyrusInspectorMirror.Open(o, cmd.width, cmd.height);
            return err == null ? Ok(null) : Fail(err);
        }

        static string SelectInUnity(PapyrusBridge.Command cmd)
        {
            var o = Find(cmd.target);
            if (!o && !string.IsNullOrEmpty(cmd.path)) o = AssetDatabase.LoadMainAssetAtPath(cmd.path);
            if (!o) return Fail("오브젝트를 찾지 못했어요.");
            Selection.activeObject = o;
            EditorGUIUtility.PingObject(o);
            return Ok(null);
        }
    }
}
