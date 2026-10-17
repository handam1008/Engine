using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ResizeHandleAuthoring
{
    const string Folder = "Artifacts/YouTubeUI/ResizeHandle";
    const string Request = Folder + "/apply.request";
    const string PrefabPath = "Assets/RYU/Prefab/Panel - YouTube.prefab";
    static ResizeHandleAuthoring() { EditorApplication.delayCall += Run; }

    static void Run()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Run; return; }
        try
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/RYU/Scene/MainScene.unity") throw new Exception("Open MainScene first.");
            var canvas = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>(true)).First(c => c.name == "Canvas");
            var panel = canvas.transform.Find("Panel - YouTube") as RectTransform;
            if (!panel) throw new Exception("Panel - YouTube not found.");
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/RYU/Script/UI/UIResizeHandle.cs");
            var type = script.GetClass();
            if (type == null) throw new Exception("UIResizeHandle script has not compiled yet.");
            Directory.CreateDirectory(Folder);
            File.Copy(scene.path, Folder + "/MainScene.before.unity.txt", true);
            File.Copy(PrefabPath, Folder + "/Panel.before.prefab.txt", true);
            Apply(panel, type);
            Verify(panel, type);
            TestDrag(panel, type, canvas);
            var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Apply(prefab.GetComponent<RectTransform>(), type);
                Verify(prefab.GetComponent<RectTransform>(), type);
                PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Folder + "/result.txt", "SUCCESS\nCanvas/Panel - YouTube/Resize Handle\nHit area: 24 x 24; bottom-right anchor and pivot\nTarget Panel: connected\nMinimum size: 500 x 350\nScene and prefab verified\nDrag +120/-80: width +120, height +80\nExtreme inward drag: clamped to 500 x 350\nTop-left corner remains fixed\nRuntime source edits: 0");
            File.Delete(Request);
            Debug.Log("Resize Handle UI applied and drag simulation passed.");
        }
        catch (Exception e)
        {
            Directory.CreateDirectory(Folder); File.WriteAllText(Folder + "/error.txt", e.ToString());
            File.Delete(Request); Debug.LogException(e);
        }
    }

    static void Apply(RectTransform panel, Type type)
    {
        var handle = panel.Find("Resize Handle") as RectTransform;
        if (!handle)
        {
            var go = new GameObject("Resize Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = panel.gameObject.layer; go.transform.SetParent(panel, false);
            handle = go.GetComponent<RectTransform>();
        }
        handle.anchorMin = handle.anchorMax = new Vector2(1, 0);
        handle.pivot = new Vector2(1, 0);
        handle.anchoredPosition = new Vector2(-4, 4);
        handle.sizeDelta = new Vector2(24, 24);
        handle.localScale = Vector3.one; handle.SetAsLastSibling();
        var image = handle.GetComponent<Image>(); image.color = new Color(1, 1, 1, 0); image.raycastTarget = true;
        var component = handle.GetComponent(type) ?? handle.gameObject.AddComponent(type);
        var serialized = new SerializedObject(component);
        serialized.FindProperty("targetPanel").objectReferenceValue = panel;
        serialized.FindProperty("minimumSize").vector2Value = new Vector2(500, 350);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        // Three small stair-stepped diagonal grip lines. Only the parent receives input.
        for (int i = 0; i < 3; i++)
        {
            string name = "Grip " + (i + 1);
            var line = handle.Find(name) as RectTransform;
            if (!line)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.layer = panel.gameObject.layer; go.transform.SetParent(handle, false); line = go.GetComponent<RectTransform>();
            }
            line.anchorMin = line.anchorMax = new Vector2(1, 0);
            line.pivot = new Vector2(1, .5f); line.anchoredPosition = new Vector2(-2, 3 + i * 5);
            line.sizeDelta = new Vector2(6 + i * 6, 2); line.localEulerAngles = new Vector3(0, 0, 45);
            var graphic = line.GetComponent<Image>(); graphic.color = new Color(.42f, .42f, .42f, 1); graphic.raycastTarget = false;
        }
        EditorUtility.SetDirty(component);
    }

    static void Verify(RectTransform panel, Type type)
    {
        var handle = panel.Find("Resize Handle") as RectTransform;
        var serialized = new SerializedObject(handle.GetComponent(type));
        if (serialized.FindProperty("targetPanel").objectReferenceValue != panel || !handle.GetComponent<Image>().raycastTarget
            || handle.sizeDelta != new Vector2(24, 24) || handle.anchorMin != new Vector2(1, 0)
            || handle.GetSiblingIndex() != panel.childCount - 1)
            throw new Exception("Resize Handle configuration failed verification.");
    }

    static void TestDrag(RectTransform panel, Type type, Canvas canvas)
    {
        var clone = UnityEngine.Object.Instantiate(panel.gameObject, panel.parent);
        clone.name = "Resize verification clone"; clone.hideFlags = HideFlags.HideAndDontSave; clone.SetActive(false);
        try
        {
            var rect = clone.GetComponent<RectTransform>();
            var component = clone.transform.Find("Resize Handle").GetComponent(type);
            type.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);
            Vector2 size = rect.rect.size; var corners = new Vector3[4]; rect.GetWorldCorners(corners); Vector3 topLeft = corners[1];
            var eventSystem = canvas.GetComponentInParent<Transform>().root.gameObject.scene.GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<EventSystem>(true)).First();
            var data = new PointerEventData(eventSystem); data.position = new Vector2(700, 450);
            type.GetMethod("OnBeginDrag").Invoke(component, new object[] { data });
            data.position += new Vector2(120, -80) * canvas.scaleFactor;
            type.GetMethod("OnDrag").Invoke(component, new object[] { data });
            if ((rect.rect.size - size - new Vector2(120, 80)).sqrMagnitude > .1f) throw new Exception("Outward drag size mismatch.");
            rect.GetWorldCorners(corners);
            if ((corners[1] - topLeft).sqrMagnitude > .1f) throw new Exception("Top-left corner moved.");
            data.position = new Vector2(700, 450) + new Vector2(-10000, 10000) * canvas.scaleFactor;
            type.GetMethod("OnDrag").Invoke(component, new object[] { data });
            if ((rect.rect.size - new Vector2(500, 350)).sqrMagnitude > .1f) throw new Exception("Minimum size clamp failed.");
        }
        finally { UnityEngine.Object.DestroyImmediate(clone); }
    }
}
