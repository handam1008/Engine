using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RYU.Editor
{
    public sealed class UnitCreatorWindow : EditorWindow
    {
        [SerializeField] private UnitCreatorKind kind;
        [SerializeField] private string unitName = "";
        [SerializeField] private Sprite sprite;
        [SerializeField] private DefaultAsset destination;
        [SerializeField] private bool flipSprite;
        [SerializeField] private int maxHp = 100, damage = 10, spawnStorage = 10, vaccine = 10;
        [SerializeField] private float moveSpeed = 2f, attackRange = 1f, attackCooldown = 1f, spawnCooldown = 5f;
        private HelpBox status;
        private Label summary;
        private Button generate;
        private Button retry;
        private Button stop;
        private Image preview;
        private VisualElement characterFields;
        private VisualElement enemyFields;
        private ObjectField folderField;

        [MenuItem("RYU/유닛 생성기")]
        public static void Open()
        {
            var window = GetWindow<UnitCreatorWindow>("유닛 생성기");
            window.minSize = new Vector2(420f, 520f);
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.paddingLeft = scroll.style.paddingRight = 14;
            scroll.style.paddingTop = scroll.style.paddingBottom = 12;
            rootVisualElement.Add(scroll);
            var title = new Label("캐릭터 · 적 생성");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            scroll.Add(title);
            scroll.Add(new HelpBox("이름과 스프라이트를 고르면 SO·스크립트·애니메이터·프리팹을 생성합니다.", HelpBoxMessageType.Info));

            var kindField = new PopupField<string>("종류", new List<string> { "아군 캐릭터", "적" }, (int)kind) { name = "unit-kind" };
            kindField.RegisterValueChangedCallback(e =>
            {
                string previous = DefaultFolder();
                kind = (UnitCreatorKind)kindField.index;
                if (destination == null || AssetDatabase.GetAssetPath(destination) == previous)
                    destination = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultFolder());
                folderField?.SetValueWithoutNotify(destination);
                Refresh();
            });
            scroll.Add(kindField);
            var nameField = new TextField("이름") { name = "unit-name", value = unitName };
            nameField.RegisterValueChangedCallback(e => { unitName = e.newValue; Refresh(); });
            scroll.Add(nameField);
            var spriteField = new ObjectField("스프라이트") { name = "unit-sprite", objectType = typeof(Sprite), allowSceneObjects = false, value = sprite };
            spriteField.RegisterValueChangedCallback(e => { sprite = e.newValue as Sprite; Refresh(); });
            scroll.Add(spriteField);
            preview = new Image { scaleMode = ScaleMode.ScaleToFit };
            preview.style.height = 100;
            preview.style.marginTop = preview.style.marginBottom = 6;
            scroll.Add(preview);

            if (destination == null) destination = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultFolder());
            folderField = new ObjectField("저장 폴더") { name = "unit-folder", objectType = typeof(DefaultAsset), allowSceneObjects = false, value = destination };
            folderField.RegisterValueChangedCallback(e => { destination = e.newValue as DefaultAsset; Refresh(); });
            scroll.Add(folderField);
            scroll.Add(new Button(ChooseFolder) { text = "저장 폴더 찾아보기" });

            var advanced = new Foldout { text = "기본 능력치 · 방향", value = false };
            var flip = new Toggle("스프라이트 좌우 반전") { value = flipSprite };
            flip.RegisterValueChangedCallback(e => { flipSprite = e.newValue; Refresh(); });
            advanced.Add(flip);
            AddInt(advanced, "체력", maxHp, v => maxHp = v);
            AddInt(advanced, "공격력", damage, v => damage = v);
            AddFloat(advanced, "이동속도", moveSpeed, v => moveSpeed = v);
            AddFloat(advanced, "사거리", attackRange, v => attackRange = v);
            AddFloat(advanced, "공격 간격(초)", attackCooldown, v => attackCooldown = v);
            characterFields = new VisualElement();
            AddInt(characterFields, "생산 저장공간", spawnStorage, v => spawnStorage = v);
            AddFloat(characterFields, "재생산 대기(초)", spawnCooldown, v => spawnCooldown = v);
            advanced.Add(characterFields);
            enemyFields = new VisualElement();
            AddInt(enemyFields, "처치 백신 보상", vaccine, v => vaccine = v);
            advanced.Add(enemyFields);
            scroll.Add(advanced);
            summary = new Label();
            summary.style.whiteSpace = WhiteSpace.Normal;
            summary.style.marginTop = summary.style.marginBottom = 10;
            scroll.Add(summary);
            status = new HelpBox("", HelpBoxMessageType.Info);
            scroll.Add(status);
            generate = new Button(Generate) { name = "generate-unit", text = "생성" };
            generate.style.height = 34;
            generate.style.marginTop = 8;
            scroll.Add(generate);
            retry = new Button(UnitAssetCreator.CompletePendingCreation) { text = "컴파일 후 다시 연결" };
            stop = new Button(UnitAssetCreator.StopPendingCreation) { text = "자동 연결 중단 (생성된 파일 보존)" };
            scroll.Add(retry);
            scroll.Add(stop);
            scroll.Add(new Button(() =>
            {
                var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(UnitAssetCreator.LastCreatedFolder);
                if (folder != null) { Selection.activeObject = folder; EditorGUIUtility.PingObject(folder); }
            }) { text = "마지막 생성 폴더 보기" });
            scroll.Add(new HelpBox("기본 클립은 선택한 한 장의 이미지를 사용합니다. 걷기·공격 그림은 생성된 클립에서 교체할 수 있습니다. 기본 공격은 가장 가까운 대상에게 단일 1타입니다.", HelpBoxMessageType.Info));
            rootVisualElement.schedule.Execute(Refresh).Every(500);
            Refresh();
        }

        private string DefaultFolder() => kind == UnitCreatorKind.Character ? "Assets/RYU/GameModule/Charcter" : "Assets/KSM/02.Prefab/Enemy";

        private UnitCreationRequest Request() => new UnitCreationRequest
        {
            kind = kind, displayName = unitName, parentFolder = AssetDatabase.GetAssetPath(destination), sprite = sprite,
            flipSprite = flipSprite, maxHp = maxHp, damage = damage, moveSpeed = moveSpeed,
            attackRange = attackRange, attackCooldown = attackCooldown, spawnStorage = spawnStorage,
            spawnCooldown = spawnCooldown, vaccine = vaccine
        };

        private void Refresh()
        {
            if (generate == null) return;
            var request = Request();
            string error = UnitAssetCreator.Validate(request);
            generate.SetEnabled(error == null);
            bool pending = UnitAssetCreator.IsPending;
            generate.text = pending ? "컴파일 · 연결 중…" : "생성";
            retry.style.display = stop.style.display = pending ? DisplayStyle.Flex : DisplayStyle.None;
            retry.SetEnabled(!EditorApplication.isCompiling && !EditorApplication.isUpdating);
            status.text = pending ? UnitAssetCreator.LastMessage : error ?? UnitAssetCreator.LastMessage;
            status.messageType = pending || error == null ? HelpBoxMessageType.Info : HelpBoxMessageType.Warning;
            preview.sprite = sprite;
            preview.style.scale = new Scale(new Vector3(flipSprite ? -1f : 1f, 1f, 1f));
            characterFields.style.display = kind == UnitCreatorKind.Character ? DisplayStyle.Flex : DisplayStyle.None;
            enemyFields.style.display = kind == UnitCreatorKind.Enemy ? DisplayStyle.Flex : DisplayStyle.None;
            summary.text = string.IsNullOrWhiteSpace(unitName) ? "저장 폴더 안에 유닛 이름의 하위 폴더가 생성됩니다."
                : request.Folder + "\n유닛 SO · 공격 SO · 스크립트 2개 · Animator · 기본 클립 6개 · 프리팹";
        }

        private void Generate()
        {
            try { UnitAssetCreator.BeginCreation(Request()); }
            catch (System.Exception exception) { Debug.LogException(exception); }
            Refresh();
        }

        private void ChooseFolder()
        {
            string path = EditorUtility.OpenFolderPanel("유닛 저장 폴더", Application.dataPath, "");
            if (string.IsNullOrEmpty(path)) return;
            string relative = FileUtil.GetProjectRelativePath(path);
            if (!AssetDatabase.IsValidFolder(relative) || !(relative == "Assets" || relative.StartsWith("Assets/")))
            {
                status.text = "Assets 안의 폴더를 선택해 주세요.";
                return;
            }
            destination = AssetDatabase.LoadAssetAtPath<DefaultAsset>(relative);
            folderField.value = destination;
            Refresh();
        }

        private void AddInt(VisualElement parent, string label, int value, System.Action<int> assign)
        {
            var field = new IntegerField(label) { value = value };
            field.RegisterValueChangedCallback(e => { assign(e.newValue); Refresh(); });
            parent.Add(field);
        }

        private void AddFloat(VisualElement parent, string label, float value, System.Action<float> assign)
        {
            var field = new FloatField(label) { value = value };
            field.RegisterValueChangedCallback(e => { assign(e.newValue); Refresh(); });
            parent.Add(field);
        }
    }
}
