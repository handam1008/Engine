using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RYU.Editor
{
    // 뽑기 씬(TT)에 Bliss 바탕화면, MainScene 작업표시줄 복사본, 압축 풀기 마법사 창을 만든다.
    // 다시 실행하면 이전에 만든 것을 지우고 새로 만든다.
    public static class GachaDesktopBuilder
    {
        private const string GachaScenePath = "Assets/PJW/0.0Scene/TT.unity";
        private const string MainScenePath = "Assets/RYU/Scene/MainScene.unity";
        private const string BlissPath = "Assets/RYU/Sprite/images (1).jpg";
        private const string TitleBarPath = "Assets/RYU/Sprite/WindowUI/XpTitleBarSquare.png";
        private const string CloseButtonPath = "Assets/RYU/Sprite/WindowUI/XpCaptionCloseSquare.png";
        private const string ZipFolderPath = "Assets/RYU/Sprite/Charcter/Folder/Folder_01.png";
        private const string FontPath = "Assets/RYU/Font/GULIM SDF.asset";

        private const string BackgroundName = "Desktop Background";
        private const string TaskbarName = "Panel - Taskbar";
        private const string WizardName = "Phase_Wizard";
        private const string ModuleName = "GachaUnzipWizardModule";

        private static readonly Color DialogGray = new(0.831f, 0.816f, 0.784f);
        private static readonly Color XpBlue = new(0f, 0.329f, 0.89f);
        private static readonly Color BannerBlue = new(0.243f, 0.427f, 0.808f);

        private static TMP_FontAsset _font;

        [MenuItem("Tools/Gacha/바탕화면 · 압축 마법사 UI 만들기")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[GachaDesktopBuilder] 플레이 모드에서는 실행할 수 없습니다.");
                return;
            }

            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            bool openedGacha = OpenAdditive(GachaScenePath, out Scene gacha);
            bool openedMain = OpenAdditive(MainScenePath, out Scene main);

            GameObject canvas = gacha.GetRootGameObjects().First(g => g.name == "Canvas");
            GameObject owner = gacha.GetRootGameObjects().First(g => g.name == "GachaOwner");
            GameObject mainCanvas = main.GetRootGameObjects().First(g => g.name == "Canvas");

            RemoveChild(canvas.transform, BackgroundName);
            RemoveChild(canvas.transform, TaskbarName);
            RemoveChild(canvas.transform, WizardName);
            RemoveChild(owner.transform, ModuleName);

            BuildBackground(canvas.transform);
            var wizard = BuildWizard(canvas.transform, out GameObject window, out TextMeshProUGUI body, out Button confirm);
            BuildTaskbar(canvas.transform, mainCanvas.transform);

            // Phase_1(압축 해제 창) 바로 뒤에 마법사를 둔다
            Transform phase1 = canvas.transform.Find("Phase_1");
            if (phase1 != null) wizard.transform.SetSiblingIndex(phase1.GetSiblingIndex() + 1);

            var module = new GameObject(ModuleName).AddComponent<GachaUnzipWizardModule>();
            module.transform.SetParent(owner.transform, false);
            var so = new SerializedObject(module);
            so.FindProperty("_wizardWindow").objectReferenceValue = window;
            so.FindProperty("_bodyText").objectReferenceValue = body;
            so.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddVoidPersistentListener(confirm.onClick, owner.GetComponent<GachaOwner>().OnWizardConfirm);
            WireFolderButtons(canvas.transform.Find("Phase_0"), module);

            EditorSceneManager.MarkSceneDirty(gacha);
            EditorSceneManager.SaveScene(gacha);
            if (openedMain) EditorSceneManager.CloseScene(main, true);
            if (openedGacha) EditorSceneManager.CloseScene(gacha, true);
            Debug.Log("[GachaDesktopBuilder] TT 씬에 바탕화면, 작업표시줄, 압축 풀기 마법사를 만들었습니다.");
        }

        private static bool OpenAdditive(string path, out Scene scene)
        {
            scene = SceneManager.GetSceneByPath(path);
            if (scene.isLoaded) return false;
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            return true;
        }

        private static void RemoveChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        private static void BuildBackground(Transform canvas)
        {
            var bg = CreateImage("Desktop Background", canvas, Color.white);
            bg.sprite = LoadSprite(BlissPath);
            bg.raycastTarget = false;
            Stretch(bg.rectTransform, 0, 0, 0, 0);
            bg.transform.SetAsFirstSibling();
        }

        private static void BuildTaskbar(Transform canvas, Transform mainCanvas)
        {
            Transform source = mainCanvas.Find("Panel - Taskbar ");
            var copy = Object.Instantiate(source.gameObject, canvas, false);
            copy.name = TaskbarName;
            copy.transform.SetAsLastSibling();

            // MainScene 창을 여는 버튼 연결은 다른 씬을 가리키므로 끊는다
            foreach (var button in copy.GetComponentsInChildren<Button>(true))
            {
                for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                {
                    Object target = button.onClick.GetPersistentTarget(i);
                    bool sceneObject = target != null && !EditorUtility.IsPersistent(target);
                    bool insideCopy = target is Component c && c.transform.IsChildOf(copy.transform)
                                      || target is GameObject g && g.transform.IsChildOf(copy.transform);
                    if (target == null || sceneObject && !insideCopy)
                        UnityEventTools.RemovePersistentListener(button.onClick, i);
                }
            }
        }

        private static GameObject BuildWizard(Transform canvas, out GameObject window, out TextMeshProUGUI body, out Button confirm)
        {
            var root = new GameObject(WizardName, typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            Stretch((RectTransform)root.transform, 0, 0, 0, 0);

            // 창 테두리(XP 파란 테두리)
            var frame = CreateImage("Wizard Window", root.transform, XpBlue);
            Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(1000, 660));
            window = frame.gameObject;

            var titleBar = CreateImage("Title Bar", frame.transform, Color.white);
            titleBar.sprite = LoadSprite(TitleBarPath);
            TopBar(titleBar.rectTransform, 40);
            var title = CreateText("Window Title", titleBar.transform, "압축 풀기 마법사", 24, Color.white, FontStyles.Bold);
            Stretch(title.rectTransform, 14, 0, -60, 0);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            var close = CreateImage("Close Button", titleBar.transform, Color.white);
            close.sprite = LoadSprite(CloseButtonPath);
            close.raycastTarget = false;
            Place(close.rectTransform, new Vector2(1, 0.5f), new Vector2(-6, 0), new Vector2(30, 30), new Vector2(1, 0.5f));

            var windowBody = CreateImage("Window Body", frame.transform, DialogGray);
            Stretch(windowBody.rectTransform, 4, 4, -4, -40);

            // 왼쪽 배너 + 압축 폴더 아이콘
            var banner = CreateImage("Banner", windowBody.transform, BannerBlue);
            banner.rectTransform.anchorMin = new Vector2(0, 0);
            banner.rectTransform.anchorMax = new Vector2(0, 1);
            banner.rectTransform.pivot = new Vector2(0, 1);
            banner.rectTransform.offsetMin = new Vector2(0, 90);
            banner.rectTransform.offsetMax = new Vector2(300, 0);
            var icon = CreateImage("Zip Icon", banner.transform, Color.white);
            icon.sprite = LoadSprite(ZipFolderPath);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            Place(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(220, 220), new Vector2(0.5f, 1));

            // 오른쪽 흰 본문
            var content = CreateImage("Content", windowBody.transform, Color.white);
            content.rectTransform.anchorMin = Vector2.zero;
            content.rectTransform.anchorMax = Vector2.one;
            content.rectTransform.offsetMin = new Vector2(300, 90);
            content.rectTransform.offsetMax = Vector2.zero;
            var heading = CreateText("Heading", content.transform, "압축 풀기 마법사 시작", 36, Color.black, FontStyles.Bold);
            TopBar(heading.rectTransform, 50, 40, -40, -40);
            body = CreateText("Body", content.transform, "", 26, Color.black, FontStyles.Normal);
            Stretch(body.rectTransform, 40, 30, -40, -120);
            body.alignment = TextAlignmentOptions.TopLeft;

            // 아래 버튼 줄
            var lineDark = CreateImage("Separator Dark", windowBody.transform, new Color(0.5f, 0.5f, 0.5f));
            BottomLine(lineDark.rectTransform, 90);
            var lineLight = CreateImage("Separator Light", windowBody.transform, Color.white);
            BottomLine(lineLight.rectTransform, 88);

            var back = CreateButton("Back Button", windowBody.transform, "< 뒤로(B)", new Vector2(-230, 20), new Color(0.6f, 0.6f, 0.6f));
            back.interactable = false;
            confirm = CreateButton("Confirm Button", windowBody.transform, "확인", new Vector2(-30, 20), Color.black);
            confirm.GetComponent<Outline>().effectColor = new Color(0f, 0.235f, 0.455f);

            window.SetActive(false);
            return root;
        }

        private static void WireFolderButtons(Transform phase0, GachaUnzipWizardModule module)
        {
            if (phase0 == null) return;
            foreach (var button in phase0.GetComponentsInChildren<Button>(true))
            {
                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label == null) continue;
                // 다시 실행할 때 이전 연결(지워진 모듈을 가리킴)을 먼저 정리
                for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                    if (button.onClick.GetPersistentMethodName(i) == nameof(GachaUnzipWizardModule.SelectFile))
                        UnityEventTools.RemovePersistentListener(button.onClick, i);
                UnityEventTools.AddObjectPersistentListener<TMP_Text>(button.onClick, module.SelectFile, label);
            }
        }

        private static Button CreateButton(string name, Transform parent, string text, Vector2 pos, Color textColor)
        {
            var face = CreateImage(name, parent, new Color(0.96f, 0.96f, 0.94f));
            Place(face.rectTransform, new Vector2(1, 0), pos, new Vector2(170, 50), new Vector2(1, 0));
            var outline = face.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 0.25f, 0.25f);
            outline.effectDistance = new Vector2(2, -2);
            var label = CreateText("Label", face.transform, text, 24, textColor, FontStyles.Normal);
            Stretch(label.rectTransform, 0, 0, 0, 0);
            label.alignment = TextAlignmentOptions.Center;
            return face.gameObject.AddComponent<Button>();
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, Color color, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (_font != null) tmp.font = _font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Sprite LoadSprite(string path) =>
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();

        private static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(right, top);
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static void TopBar(RectTransform rt, float height, float left = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(left, top - height);
            rt.offsetMax = new Vector2(right, top);
        }

        private static void BottomLine(RectTransform rt, float y)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(0, y);
            rt.offsetMax = new Vector2(0, y + 2);
        }
    }
}
