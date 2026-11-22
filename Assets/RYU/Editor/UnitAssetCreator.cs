using System;
using System.IO;
using System.Linq;
using System.Text;
using KSM._00.Scripts;
using RYU.Script.Charcter;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RYU.Editor
{
    public enum UnitCreatorKind { Character, Enemy }

    [Serializable]
    public sealed class UnitCreationRequest
    {
        public UnitCreatorKind kind;
        public string displayName;
        public string parentFolder;
        public bool flipSprite;
        public int maxHp = 100;
        public int damage = 10;
        public float moveSpeed = 2f;
        public float attackRange = 1f;
        public float attackCooldown = 1f;
        public int spawnStorage = 10;
        public float spawnCooldown = 5f;
        public int vaccine = 10;
        [NonSerialized] public Sprite sprite;

        public string Identifier => UnitAssetCreator.NormalizeIdentifier(displayName);
        public string Folder => parentFolder.TrimEnd('/') + "/" + Identifier;
        public string Namespace => kind == UnitCreatorKind.Character ? "RYU.Generated.Characters" : "RYU.Generated.Enemies";
        public string UnitClass => Identifier + (kind == UnitCreatorKind.Character ? "Character" : "Enemy");
        public string AttackClass => Identifier + "AttackSO";
    }

    [InitializeOnLoad]
    public static class UnitAssetCreator
    {
        private const string MessageKey = "RYU.UnitCreator.Message";
        private const string ResultKey = "RYU.UnitCreator.Result";
        private static readonly string[] States = { "Wait", "Chase", "Attack", "Hit", "Dead", "Corpse" };
        private static string PendingPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/RYU.UnitCreator.pending.json"));
        public static bool IsPending => File.Exists(PendingPath);
        public static string LastMessage => SessionState.GetString(MessageKey, "");
        public static string LastCreatedFolder => SessionState.GetString(ResultKey, "");

        static UnitAssetCreator()
        {
            EditorApplication.delayCall += CompletePendingCreation;
        }

        public static string NormalizeIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            string id = new string(name.Trim().Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
            return char.IsDigit(id[0]) ? "Unit_" + id : id;
        }

        public static string Validate(UnitCreationRequest request)
        {
            if (request == null) return "생성 정보를 입력해 주세요.";
            if (EditorApplication.isPlayingOrWillChangePlaymode) return "플레이 모드를 종료한 뒤 생성해 주세요.";
            if (EditorApplication.isCompiling) return "스크립트 컴파일이 끝난 뒤 생성해 주세요.";
            if (IsPending) return "이전 유닛의 컴파일·연결을 기다리는 중입니다.";
            if (string.IsNullOrWhiteSpace(request.displayName)) return "이름을 입력해 주세요.";
            if (request.displayName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || request.displayName.Contains(".."))
                return "이름에는 경로 기호나 '..'를 사용할 수 없습니다.";
            if (!request.Identifier.Any(char.IsLetterOrDigit)) return "이름에 문자나 숫자를 넣어 주세요.";
            if (!IsAssetFolderPath(request.parentFolder) || !AssetDatabase.IsValidFolder(request.parentFolder))
                return "Assets 안의 기존 폴더를 저장 위치로 선택해 주세요.";
            if (!IsRuntimeScriptFolder(request.Folder))
                return "유닛 코드를 참조할 수 없는 폴더입니다. 기본 저장 폴더나 일반 Assets 폴더를 선택해 주세요.";
            if (request.sprite == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(request.sprite, out string _, out long _))
                return "프로젝트에 저장된 Sprite를 선택해 주세요.";
            if (Directory.Exists(request.Folder) || File.Exists(request.Folder) || AssetDatabase.IsValidFolder(request.Folder))
                return "같은 이름의 폴더가 이미 있습니다. 다른 이름이나 저장 위치를 선택해 주세요.";
            if (FindType(request.Namespace + "." + request.UnitClass) != null || FindType(request.Namespace + "." + request.AttackClass) != null)
                return "같은 종류에 해당 이름의 스크립트가 이미 있습니다.";
            if (request.maxHp <= 0 || request.damage < 0 || !ValidNumber(request.moveSpeed, false)
                || !ValidNumber(request.attackRange, true) || !ValidNumber(request.attackCooldown, false)
                || request.spawnStorage < 0 || !ValidNumber(request.spawnCooldown, false) || request.vaccine < 0)
                return "체력·사거리는 양수, 나머지 수치는 0 이상이어야 합니다.";
            if (LayerMask.NameToLayer("Player") < 0 || LayerMask.NameToLayer("Enemy") < 0)
                return "프로젝트에 Player와 Enemy 레이어가 필요합니다.";
            return null;
        }

        public static void BeginCreation(UnitCreationRequest request)
        {
            string error = Validate(request);
            if (error != null) throw new InvalidOperationException(error);
            request.displayName = request.displayName.Trim();
            AssetDatabase.CreateFolder(request.parentFolder, request.Identifier);
            AssetDatabase.CreateFolder(request.Folder, "Scripts");
            AssetDatabase.CreateFolder(request.Folder, "Animations");
            File.WriteAllText(PendingPath, JsonUtility.ToJson(request), new UTF8Encoding(false));
            SessionState.SetString(MessageKey, "스크립트 컴파일 후 SO·애니메이터·프리팹을 자동 연결합니다.");
            try
            {
                var controller = CreateController(request);
                UnitSO data = request.kind == UnitCreatorKind.Character
                    ? ScriptableObject.CreateInstance<CharacterDataSO>()
                    : ScriptableObject.CreateInstance<EnemySO>();
                data.Name = request.displayName;
                data.sprite = request.sprite;
                data.animatorController = controller;
                data.unitType = request.kind == UnitCreatorKind.Character ? UnitType.Charcter : UnitType.Enemy;
                data.maxHp = request.maxHp;
                data.attackDmg = request.damage;
                data.moveSpeed = request.moveSpeed;
                data.attackRange = request.attackRange;
                data.attackCoolTime = request.attackCooldown;
                data.whatIsDetect = LayerMask.GetMask(request.kind == UnitCreatorKind.Character ? "Enemy" : "Player");
                if (data is CharacterDataSO character)
                {
                    character.spawnStorage = request.spawnStorage;
                    character.spawnCoolTime = request.spawnCooldown;
                    character.number = AssetDatabase.FindAssets("t:CharacterDataSO")
                        .Select(g => AssetDatabase.LoadAssetAtPath<CharacterDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                        .Where(c => c != null).Select(c => c.number).DefaultIfEmpty(-1).Max() + 1;
                }
                if (data is EnemySO enemy) enemy.vaccinegauge = request.vaccine;
                AssetDatabase.CreateAsset(data, request.Folder + "/" + request.Identifier + ".asset");
                File.WriteAllText(request.Folder + "/Scripts/" + request.UnitClass + ".cs", UnitScript(request), new UTF8Encoding(false));
                File.WriteAllText(request.Folder + "/Scripts/" + request.AttackClass + ".cs", AttackScript(request), new UTF8Encoding(false));
                AssetDatabase.Refresh();
                EditorApplication.delayCall += CompletePendingCreation;
            }
            catch (Exception exception)
            {
                SessionState.SetString(MessageKey, "생성을 완료하지 못했습니다: " + exception.Message);
                throw;
            }
        }

        public static void CompletePendingCreation()
        {
            if (!IsPending || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CompletePendingCreation;
                return;
            }
            try
            {
                var request = JsonUtility.FromJson<UnitCreationRequest>(File.ReadAllText(PendingPath));
                if (!IsAssetFolderPath(request.parentFolder) || !AssetDatabase.IsValidFolder(request.Folder))
                    throw new InvalidOperationException("생성 중이던 폴더를 찾을 수 없습니다.");
                Type unitType = FindType(request.Namespace + "." + request.UnitClass);
                Type attackType = FindType(request.Namespace + "." + request.AttackClass);
                if (unitType == null || attackType == null)
                {
                    SessionState.SetString(MessageKey, "스크립트 컴파일 대기 중입니다. 콘솔에 오류가 있으면 해결한 뒤 다시 연결해 주세요.");
                    return;
                }
                var data = AssetDatabase.LoadAssetAtPath<UnitSO>(request.Folder + "/" + request.Identifier + ".asset");
                if (data == null) throw new InvalidOperationException("생성한 유닛 SO를 찾을 수 없습니다.");
                string attackPath = request.Folder + "/" + request.Identifier + "Attack.asset";
                var attack = AssetDatabase.LoadAssetAtPath<AbstractAttackSO>(attackPath);
                if (attack == null)
                {
                    attack = (AbstractAttackSO)ScriptableObject.CreateInstance(attackType);
                    AssetDatabase.CreateAsset(attack, attackPath);
                }
                string prefabPath = request.Folder + "/" + request.Identifier + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                    throw new InvalidOperationException("프리팹이 이미 있습니다. 기존 파일을 덮어쓰지 않았습니다.");
                data.prefab = CreatePrefab(request, data, attack, unitType, prefabPath);
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssetIfDirty(data);
                File.Delete(PendingPath);
                SessionState.SetString(ResultKey, request.Folder);
                SessionState.SetString(MessageKey, request.displayName + " 생성 완료 — SO·스크립트·애니메이터·프리팹 연결됨");
                EditorGUIUtility.PingObject(data.prefab);
            }
            catch (Exception exception)
            {
                SessionState.SetString(MessageKey, "자동 연결을 완료하지 못했습니다: " + exception.Message);
                Debug.LogException(exception);
            }
        }

        public static void StopPendingCreation()
        {
            if (IsPending) File.Delete(PendingPath);
            SessionState.SetString(MessageKey, "자동 연결을 중단했습니다. 이미 생성된 파일은 보존했습니다.");
        }

        private static AnimatorController CreateController(UnitCreationRequest request)
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath(request.Folder + "/" + request.Identifier + ".controller");
            var machine = controller.layers[0].stateMachine;
            for (int i = 0; i < States.Length; i++)
            {
                string stateName = States[i];
                bool loop = stateName == "Wait" || stateName == "Chase" || stateName == "Corpse";
                float duration = stateName == "Hit" ? .25f : stateName == "Dead" ? .4f : .5f;
                var clip = new AnimationClip { name = request.Identifier + "_" + stateName, frameRate = 12f };
                AnimationUtility.SetObjectReferenceCurve(clip,
                    EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"),
                    new[] { new ObjectReferenceKeyframe { time = 0f, value = request.sprite }, new ObjectReferenceKeyframe { time = duration, value = request.sprite } });
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                if (!loop)
                {
                    var end = new AnimationEvent { time = duration - 1f / 60f, functionName = "AnimEnd" };
                    AnimationUtility.SetAnimationEvents(clip, stateName == "Attack"
                        ? new[] { new AnimationEvent { time = .15f, functionName = "Attack" }, end }
                        : new[] { end });
                }
                AssetDatabase.CreateAsset(clip, request.Folder + "/Animations/" + clip.name + ".anim");
                controller.AddParameter(stateName, AnimatorControllerParameterType.Bool);
                var state = machine.AddState(stateName, new Vector3(260f, i * 70f));
                state.motion = clip;
                state.writeDefaultValues = false;
                var transition = machine.AddAnyStateTransition(state);
                transition.hasExitTime = false;
                transition.duration = 0f;
                transition.canTransitionToSelf = false;
                transition.AddCondition(AnimatorConditionMode.If, 0f, stateName);
                if (stateName == "Wait") machine.defaultState = state;
            }
            AssetDatabase.SaveAssetIfDirty(controller);
            return controller;
        }

        private static GameObject CreatePrefab(UnitCreationRequest request, UnitSO data, AbstractAttackSO attack, Type unitType, string path)
        {
            var root = new GameObject(request.displayName);
            root.SetActive(false);
            try
            {
                root.layer = LayerMask.NameToLayer(request.kind == UnitCreatorKind.Character ? "Player" : "Enemy");
                root.tag = request.kind == UnitCreatorKind.Character ? "Character" : "Enemy";
                var rigidbody = root.AddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 1f;
                rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                var collider = root.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(data.sprite.bounds.size.x, data.sprite.bounds.size.y);
                collider.offset = new Vector2(data.sprite.bounds.center.x, data.sprite.bounds.center.y);
                root.AddComponent<MoveSystem>();
                root.AddComponent<HealthSystem>();
                var unit = root.AddComponent(unitType);
                var serialized = new SerializedObject(unit);
                serialized.FindProperty("<UnitSo>k__BackingField").objectReferenceValue = data;
                serialized.FindProperty("<Attack>k__BackingField").objectReferenceValue = attack;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                visual.layer = root.layer;
                var renderer = visual.AddComponent<SpriteRenderer>();
                renderer.sprite = data.sprite;
                renderer.flipX = request.flipSprite;
                visual.AddComponent<UnitAnimation>();
                var animator = visual.AddComponent<Animator>();
                animator.runtimeAnimatorController = data.animatorController;
                animator.applyRootMotion = false;
                root.SetActive(true);
                return PrefabUtility.SaveAsPrefabAsset(root, path)
                    ?? throw new InvalidOperationException("프리팹을 저장하지 못했습니다.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static bool ValidNumber(float value, bool positive)
            => !float.IsNaN(value) && !float.IsInfinity(value) && (positive ? value > 0f : value >= 0f);

        private static bool IsAssetFolderPath(string path)
            => !string.IsNullOrEmpty(path) && (path == "Assets" || path.StartsWith("Assets/", StringComparison.Ordinal))
                && !path.Contains('\\') && !path.Split('/').Any(s => s == ".." || s == "." || s.Length == 0);

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName)).FirstOrDefault(t => t != null);

        private static bool IsRuntimeScriptFolder(string folder)
        {
            string[] parts = folder.Split('/');
            if (parts.Any(p => string.Equals(p, "Editor", StringComparison.OrdinalIgnoreCase))) return false;
            if (parts.Length > 1 && new[] { "Plugins", "Standard Assets", "Pro Standard Assets" }
                .Any(p => string.Equals(p, parts[1], StringComparison.OrdinalIgnoreCase))) return false;
            for (string current = folder; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            {
                if (Directory.Exists(current) && (Directory.GetFiles(current, "*.asmdef").Length > 0
                    || Directory.GetFiles(current, "*.asmref").Length > 0)) return false;
                if (current == "Assets") break;
            }
            return true;
        }

        private static string UnitScript(UnitCreationRequest r)
            => "namespace " + r.Namespace + "\n{\n    public class " + r.UnitClass + " : AbstractUnit\n    {\n    }\n}\n";

        private static string AttackScript(UnitCreationRequest r)
            => "using UnityEngine;\n\nnamespace " + r.Namespace + "\n{\n    public class " + r.AttackClass + " : AbstractAttackSO\n    {\n"
             + "        public override void Attacking(AbstractUnit unit)\n        {\n            Collider2D target = unit.NearTarget();\n"
             + "            if (target != null && target.TryGetComponent(out HealthSystem health))\n                health.Damage(unit.UnitSo.attackDmg);\n        }\n    }\n}\n";
    }
}
