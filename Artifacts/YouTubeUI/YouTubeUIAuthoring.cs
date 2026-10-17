// Temporary editor-only authoring tool. No runtime playback logic is generated.
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class YouTubeUIAuthoring
{
    const string Output = "Artifacts/YouTubeUI";
    const string Request = Output + "/apply.request";
    const string AssetsPath = "Assets/RYU/Sprite/YouTubeUI";
    static TMP_FontAsset font;
    static Sprite titleSprite, buttonSprite;
    static readonly Color Ink = Hex("222222"), Link = Hex("0033CC"), Cream = Hex("ECE9D8");

    static YouTubeUIAuthoring() { EditorApplication.delayCall += Run; }

    static void Run()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Run; return; }
        try
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/RYU/Scene/MainScene.unity")
                throw new InvalidOperationException("Open MainScene before applying YouTube UI.");
            var canvas = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>(true))
                .FirstOrDefault(c => c.isRootCanvas && c.name == "Canvas");
            if (!canvas) throw new InvalidOperationException("Main Canvas not found.");
            var existing = canvas.transform.Find("Panel - YouTube");
            if (existing)
            {
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/RYU/Font/GULIM SDF.asset");
                var refresh = existing.Find("Browser Chrome/Refresh/Label").GetComponent<TextMeshProUGUI>();
                refresh.text = "새로고침"; refresh.fontSize = 14;
                refresh.rectTransform.sizeDelta = new Vector2(82, 33);
                existing.Find("Browser Chrome/Refresh").GetComponent<RectTransform>().sizeDelta = new Vector2(84, 33);
                existing.Find("Browser Chrome/Home Icon").GetComponent<RectTransform>().anchoredPosition = new Vector2(304, -41);
                existing.Find("Browser Chrome/Toolbar Labels").GetComponent<RectTransform>().anchoredPosition = new Vector2(355, -41);
                foreach (var slider in existing.GetComponentsInChildren<UnityEngine.UI.Slider>(true))
                { slider.handleRect.sizeDelta = new Vector2(11, 0); slider.handleRect.anchoredPosition = Vector2.zero; }
                var existingScroll = existing.GetComponentInChildren<ScrollRect>(true);
                Canvas.ForceUpdateCanvases(); LayoutRebuilder.ForceRebuildLayoutImmediate(existingScroll.content);
                existingScroll.content.anchoredPosition = Vector2.zero; existingScroll.verticalNormalizedPosition = 1; existingScroll.StopMovement();
                EditorSceneManager.MarkSceneDirty(scene);
                PrefabUtility.SaveAsPrefabAsset(existing.gameObject, "Assets/RYU/Prefab/Panel - YouTube.prefab");
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                Capture(existing.gameObject);
                File.AppendAllText(Output + "/result.txt", "\nFinal verification: unsupported glyph replaced; world-space preview captured.");
                File.Delete(Request);
                return;
            }
            Directory.CreateDirectory(Output);
            File.Copy(scene.path, Output + "/MainScene.before.unity.txt", true);
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/RYU/Font/GULIM SDF.asset");
            if (!font) throw new InvalidOperationException("Project font missing.");
            titleSprite = Gradient("TitleBar", Hex("398CFF"), Hex("0755D6"), Hex("00359E"));
            buttonSprite = Gradient("Button", Color.white, Hex("F4F3EC"), Hex("D7D3C4"));
            var panel = Box("Panel - YouTube", canvas.transform, 0, 0, 1280, 860, Hex("0054E3"));
            panel.anchorMin = panel.anchorMax = new Vector2(.5f, .5f);
            panel.pivot = new Vector2(.5f, .5f);
            panel.anchoredPosition = new Vector2(80, 30);
            panel.GetComponent<Image>().raycastTarget = true;
            Undo.RegisterCreatedObjectUndo(panel.gameObject, "Create XP YouTube UI");
            Build(panel);
            Canvas.ForceUpdateCanvases();
            const string prefab = "Assets/RYU/Prefab/Panel - YouTube.prefab";
            Directory.CreateDirectory("Assets/RYU/Prefab");
            PrefabUtility.SaveAsPrefabAsset(panel.gameObject, prefab);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Capture(panel.gameObject);
            var inputs = panel.GetComponentsInChildren<TMP_InputField>(true);
            var scroll = panel.GetComponentInChildren<ScrollRect>(true);
            var events = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true)).Count();
            if (inputs.Length != 2 || !scroll || !scroll.content || !scroll.viewport || !scroll.verticalScrollbar || events != 1 || !canvas.GetComponent<GraphicRaycaster>())
                throw new InvalidOperationException("UI structure verification failed.");
            File.WriteAllText(Output + "/result.txt", "SUCCESS\nScene: " + scene.path + "\nPanel: Canvas/Panel - YouTube\nPrefab: " + prefab
                + "\nInputs: 2\nScrollRect: content, viewport, scrollbar assigned\nEventSystem: " + events
                + "\nButtons: " + panel.GetComponentsInChildren<Button>(true).Length + "\nRuntime scripts added: 0\nPreview: " + Output + "/preview.png");
            File.Delete(Request);
            Debug.Log("YouTube UI authored and verified. " + Output + "/result.txt");
        }
        catch (Exception e)
        {
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "/error.txt", e.ToString());
            File.Delete(Request);
            Debug.LogException(e);
        }
    }

    static void Build(RectTransform p)
    {
        var title = Box("Title Bar", p, 4, 4, 1272, 37, Color.white, titleSprite);
        Icon("Browser Icon", title, "Assets/RYU/Sprite/XPStartMenu/Internet.png", 7, 6, 24, 24);
        Text("Window Title", title, "YouTube - Microsoft Internet Explorer", 40, 2, 1060, 32, 21, Color.white, true);
        Btn("Minimize", title, "_", 1165, 4, 29, 28, 23, Hex("337CE5"), Color.white);
        Btn("Maximize", title, "□", 1198, 4, 29, 28, 20, Hex("337CE5"), Color.white);
        var close = Btn("Close", title, "×", 1231, 4, 29, 28, 25, Hex("D8492F"), Color.white);
        UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(close.onClick, p.gameObject.SetActive, false);
        var browser = Box("Browser Chrome", p, 4, 41, 1272, 123, Cream);
        Text("Menu Bar", browser, "파일(F)   편집(E)   보기(V)   즐겨찾기(A)   도구(T)   도움말(H)", 12, 3, 1160, 25, 17, Ink);
        Box("Menu Divider", browser, 0, 31, 1272, 1, Hex("FFFFFF"));
        var back = Btn("Back", browser, "<", 12, 38, 33, 33, 24, Hex("4DA641"), Color.white);
        Text("Back Label", browser, "뒤로", 53, 39, 51, 30, 17, Ink);
        Btn("Forward", browser, ">", 110, 38, 33, 33, 24, Hex("B5C7A9"), Color.white);
        Btn("Stop", browser, "×", 161, 38, 33, 33, 23, Hex("D45B4C"), Color.white);
        Btn("Refresh", browser, "↻", 204, 38, 33, 33, 23, Hex("4DA641"), Color.white);
        Icon("Home Icon", browser, "Assets/RYU/Sprite/XPTaskbar/folder.png", 251, 41, 28, 28);
        Text("Toolbar Labels", browser, "검색     즐겨찾기     미디어", 303, 41, 750, 28, 18, Ink);
        Text("Address Label", browser, "주소(D)", 12, 87, 68, 27, 17, Ink);
        Input("Address Input", browser, "http://www.youtube.com/", 87, 85, 1081, 28, 17);
        Btn("Go", browser, "> 이동", 1178, 84, 84, 30, 17, Hex("F5F4EC"), Ink);

        var page = Box("Web Page", p, 5, 164, 1270, 666, Color.white);
        Text("Logo You", page, "You", 27, 16, 72, 47, 37, Ink, true);
        Box("Logo Tube Badge", page, 99, 19, 96, 44, Hex("CC151D"));
        Text("Logo Tube", page, "Tube", 103, 19, 91, 42, 34, Color.white, true);
        Text("Tagline", page, "Broadcast Yourself™", 33, 68, 220, 21, 14, Hex("777777"));
        Input("Search Input", page, "", 315, 31, 460, 32, 18);
        Btn("Search", page, "검색", 785, 31, 80, 32, 17, Cream, Ink);
        Text("Account Links", page, "회원가입  |  내 계정  |  로그인", 900, 28, 347, 34, 16, Link);
        Text("Navigation", page, "동영상     카테고리     채널     커뮤니티", 319, 75, 900, 27, 18, Link, true);
        Box("Header Rule", page, 24, 110, 1220, 2, Hex("BBBBBB"));
        Text("Video Title", page, "Windows XP - 추억의 컴퓨터", 27, 122, 775, 33, 24, Ink, true);
        var player = Box("Video Player", page, 28, 164, 778, 354, Hex("151515"));
        var surface = new GameObject("Video Surface", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        surface.transform.SetParent(player, false);
        Place(surface.GetComponent<RectTransform>(), 0, 0, 778, 354);
        surface.GetComponent<RawImage>().color = Hex("10151B");
        surface.GetComponent<RawImage>().raycastTarget = false;
        Icon("Preview Monitor", player, "Assets/RYU/Sprite/XPTaskbar/monitor.png", 349, 93, 80, 80);
        Text("Preview Title", player, "Windows XP Memories", 70, 189, 638, 47, 30, Hex("D5DBE2"), true, TextAlignmentOptions.Center);
        Text("Preview Subtitle", player, "Broadcast Yourself", 70, 239, 638, 25, 17, Hex("8D9BAA"), false, TextAlignmentOptions.Center);
        var controls = Box("Player Controls", page, 28, 518, 778, 40, Hex("D7D7D7"));
        Btn("Play", controls, ">", 6, 5, 34, 30, 21, Cream, Ink);
        Btn("Stop", controls, "■", 44, 5, 34, 30, 15, Cream, Ink);
        Slider("Playback Progress", controls, 91, 10, 374, 20, Hex("B20000"), 0);
        Text("Playback Time", controls, "0:00 / 0:00", 480, 6, 116, 28, 14, Ink);
        Text("Volume Label", controls, "음량", 608, 5, 40, 30, 14, Ink);
        Slider("Volume Slider", controls, 653, 10, 79, 20, Hex("777777"), .7f);
        Btn("Fullscreen", controls, "□", 741, 5, 30, 30, 18, Cream, Ink);
        Text("Stars", page, "★ ★ ★ ★ ★", 29, 571, 225, 27, 24, Hex("D52317"));
        Text("View Count", page, "조회수 12,345", 590, 575, 214, 25, 17, Hex("666666"), false, TextAlignmentOptions.Right);
        Text("Video Description", page, "추억의 영상을 함께 감상해 보세요.", 30, 611, 775, 27, 17, Ink);
        Sidebar(page);
        var status = Box("Status Bar", p, 4, 830, 1272, 26, Cream);
        Text("Status", status, "완료", 10, 0, 950, 25, 15, Ink);
        Text("Connection", status, "인터넷", 1130, 0, 130, 25, 15, Hex("555555"));
    }

    static void Sidebar(RectTransform page)
    {
        Box("Related Header", page, 836, 125, 407, 34, Hex("EEEEEE"));
        Text("Related Title", page, "관련 동영상", 848, 127, 380, 30, 19, Ink, true);
        var root = Box("Related Videos Scroll View", page, 835, 164, 409, 475, Color.white);
        root.GetComponent<Image>().raycastTarget = true;
        var viewport = Box("Viewport", root, 0, 0, 387, 475, Color.white);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        var content = Box("Content", viewport, 0, 0, 387, 700, Color.clear);
        content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.sizeDelta = new Vector2(0, 700);
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10; layout.padding = new RectOffset(2, 5, 2, 5);
        layout.childControlWidth = true; layout.childControlHeight = true;
        layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        string[] titles = { "Windows XP 시작 소리", "3D 파이프 화면 보호기", "추억의 핀볼 게임", "옛날 컴퓨터 광고", "플래시 애니메이션 모음", "Windows 바탕화면 이야기" };
        string[] icons = { "monitor", "network", "folder", "monitor", "folder", "network" };
        for (int i = 0; i < titles.Length; i++)
        {
            var row = Box("Video Item " + (i + 1).ToString("00"), content, 0, 0, 380, 122, Color.white);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 122;
            var b = row.gameObject.AddComponent<Button>(); b.targetGraphic = row.GetComponent<Image>();
            row.GetComponent<Image>().raycastTarget = true;
            var thumb = Box("Thumbnail", row, 0, 3, 144, 95, Hex(i % 2 == 0 ? "BDD7EF" : "D2E2CD"));
            Icon("Preview Icon", thumb, "Assets/RYU/Sprite/XPTaskbar/" + icons[i] + ".png", 45, 13, 56, 56);
            Box("Duration Background", thumb, 93, 75, 49, 18, Hex("333333"));
            Text("Duration", thumb, "0:00", 97, 75, 43, 18, 12, Color.white);
            Text("Title", row, titles[i], 154, 1, 220, 45, 18, Link, true);
            Text("Author", row, "XP Archive", 154, 51, 220, 20, 14, Hex("666666"));
            Text("Rating", row, "★★★★★", 154, 75, 150, 20, 17, Hex("C52717"));
            Text("Views", row, "추천 영상", 154, 99, 220, 19, 13, Hex("777777"));
            Box("Divider", row, 0, 120, 378, 1, Hex("DDDDDD"));
        }
        var track = Box("Scrollbar", root, 391, 0, 18, 475, Hex("EEEDE5"));
        var sliding = Box("Sliding Area", track, 1, 1, 16, 473, Color.clear);
        var handle = Box("Handle", sliding, 0, 0, 16, 120, Hex("B6C8E5"));
        handle.anchorMin = Vector2.zero; handle.anchorMax = Vector2.one; handle.offsetMin = handle.offsetMax = Vector2.zero;
        track.GetComponent<Image>().raycastTarget = true; handle.GetComponent<Image>().raycastTarget = true;
        var bar = track.gameObject.AddComponent<Scrollbar>();
        bar.handleRect = handle; bar.targetGraphic = handle.GetComponent<Image>(); bar.direction = Scrollbar.Direction.BottomToTop;
        var scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30;
        scroll.verticalScrollbar = bar; scroll.verticalNormalizedPosition = 1;
    }

    static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
    static void Place(RectTransform r, float x, float y, float w, float h)
    { r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); }
    static RectTransform Box(string name, Transform parent, float x, float y, float w, float h, Color color, Sprite sprite = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5; go.transform.SetParent(parent, false); var r = go.GetComponent<RectTransform>(); Place(r, x, y, w, h);
        var im = go.GetComponent<Image>(); im.color = color; im.sprite = sprite; im.raycastTarget = false; return r;
    }
    static TextMeshProUGUI Text(string name, Transform parent, string text, float x, float y, float w, float h, float size, Color color, bool bold = false, TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5; go.transform.SetParent(parent, false); Place(go.GetComponent<RectTransform>(), x, y, w, h);
        var t = go.GetComponent<TextMeshProUGUI>(); t.font = font; t.text = text; t.fontSize = size; t.color = color;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal; t.alignment = align;
        t.verticalAlignment = VerticalAlignmentOptions.Middle;
        t.textWrappingMode = TextWrappingModes.Normal; t.overflowMode = TextOverflowModes.Ellipsis; t.raycastTarget = false; return t;
    }
    static Button Btn(string name, Transform parent, string label, float x, float y, float w, float h, int size, Color bg, Color fg)
    {
        var r = Box(name, parent, x, y, w, h, bg, buttonSprite); var im = r.GetComponent<Image>(); im.raycastTarget = true;
        var outline = r.gameObject.AddComponent<Outline>(); outline.effectColor = Hex("777777"); outline.effectDistance = new Vector2(1, -1);
        var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = im;
        var colors = b.colors; colors.highlightedColor = Hex("E2EDFF"); colors.pressedColor = Hex("B9CAE6"); b.colors = colors;
        Text("Label", r, label, 1, 0, w - 2, h, size, fg, false, TextAlignmentOptions.Center); return b;
    }
    static void Icon(string name, Transform parent, string path, float x, float y, float w, float h)
    {
        var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (!sprite) throw new InvalidOperationException("Missing sprite: " + path);
        var r = Box(name, parent, x, y, w, h, Color.white, sprite); r.GetComponent<Image>().preserveAspect = true;
    }
    static void Input(string name, Transform parent, string value, float x, float y, float w, float h, int size)
    {
        var r = Box(name, parent, x, y, w, h, Color.white); r.GetComponent<Image>().raycastTarget = true;
        var outline = r.gameObject.AddComponent<Outline>(); outline.effectColor = Hex("7F9DB9"); outline.effectDistance = new Vector2(1, -1);
        var area = Box("Text Area", r, 6, 1, w - 12, h - 2, Color.clear); area.gameObject.AddComponent<RectMask2D>();
        var label = Text("Text", area, value, 0, 0, w - 12, h - 2, size, Ink);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        var input = r.gameObject.AddComponent<TMP_InputField>(); input.textViewport = area; input.textComponent = label;
        input.targetGraphic = r.GetComponent<Image>(); input.text = value; input.lineType = TMP_InputField.LineType.SingleLine;
    }
    static void Slider(string name, Transform parent, float x, float y, float w, float h, Color fillColor, float value)
    {
        var r = Box(name, parent, x, y, w, h, Color.clear);
        var track = Box("Track", r, 0, 7, w, 6, Hex("777777")); track.GetComponent<Image>().raycastTarget = true;
        var fillArea = Box("Fill Area", r, 0, 7, w, 6, Color.clear);
        var fill = Box("Fill", fillArea, 0, 0, w, 6, fillColor);
        fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.offsetMin = fill.offsetMax = Vector2.zero;
        var handleArea = Box("Handle Area", r, 5, 0, w - 10, h, Color.clear);
        var handle = Box("Handle", handleArea, 0, 1, 11, h - 2, Cream, buttonSprite);
        handle.pivot = new Vector2(.5f, .5f); handle.anchorMin = handle.anchorMax = new Vector2(0, .5f); handle.anchoredPosition = Vector2.zero;
        var slider = r.gameObject.AddComponent<UnityEngine.UI.Slider>(); slider.fillRect = fill; slider.handleRect = handle;
        slider.targetGraphic = handle.GetComponent<Image>(); handle.GetComponent<Image>().raycastTarget = true;
        slider.value = value;
    }
    static Sprite Gradient(string name, Color top, Color middle, Color bottom)
    {
        Directory.CreateDirectory(AssetsPath); string path = AssetsPath + "/" + name + ".png";
        var texture = new Texture2D(8, 64, TextureFormat.RGBA32, false);
        for (int y = 0; y < 64; y++) for (int x = 0; x < 8; x++)
        { float t = y / 63f; texture.SetPixel(x, y, t > .65f ? Color.Lerp(middle, top, (t - .65f) / .35f) : Color.Lerp(bottom, middle, t / .65f)); }
        texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static void Capture(GameObject panel)
    {
        var canvasGo = new GameObject("Preview Canvas", typeof(RectTransform), typeof(Canvas));
        canvasGo.hideFlags = HideFlags.HideAndDontSave;
        var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
        canvasGo.GetComponent<RectTransform>().sizeDelta = new Vector2(1440, 980);
        var cameraGo = new GameObject("Preview Camera", typeof(Camera)); cameraGo.hideFlags = HideFlags.HideAndDontSave;
        var camera = cameraGo.GetComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Hex("3C6BA3");
        camera.transform.position = new Vector3(0, 0, -1000); camera.orthographic = true; camera.orthographicSize = 490;
        camera.nearClipPlane = .3f; camera.farClipPlane = 2000; camera.cullingMask = 1 << 31;
        canvas.worldCamera = camera;
        var clone = UnityEngine.Object.Instantiate(panel, canvasGo.transform); clone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        clone.SetActive(true);
        foreach (var child in canvasGo.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
        var rt = new RenderTexture(1440, 980, 24); camera.targetTexture = rt;
        Canvas.ForceUpdateCanvases();
        var scroll = clone.GetComponentInChildren<ScrollRect>(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content); scroll.content.anchoredPosition = Vector2.zero;
        scroll.verticalNormalizedPosition = 1; scroll.StopMovement(); Canvas.ForceUpdateCanvases();
        camera.Render(); var previous = RenderTexture.active; RenderTexture.active = rt;
        var image = new Texture2D(1440, 980, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, 1440, 980), 0, 0); image.Apply();
        File.WriteAllBytes(Output + "/preview.png", image.EncodeToPNG()); RenderTexture.active = previous; camera.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(image); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(canvasGo); UnityEngine.Object.DestroyImmediate(cameraGo);
    }
}
