using DevLib.ServiceLocator;
using RYU.Script.Charcter;
using RYU.Script.Service.SpawnService;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class CharacterSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI storageText;
        [SerializeField] private Image image;
        private CharacterDataSO _data;
        private Button _createBtn;
        private bool _canSpawn;
        private Character _folder;


        private void Awake()
        {
            _createBtn = GetComponent<Button>();
        }

        private void Start()
        {
            if (_canSpawn && _createBtn != null)
                _createBtn.onClick.AddListener(() => ServiceLocator.Get<ISpawnService>().TrySpawn(_data));
        }


        public void SetCharcterSlot(CharacterDataSO data, bool canSpawn)
        {
            _data = data;
            _canSpawn = canSpawn;
            _folder = canSpawn ? null : GetComponentInParent<Character>(true);
            nameText.text = data.Name;

            if (storageText != null)
            {
                storageText.gameObject.SetActive(canSpawn);
                if (canSpawn)
                    storageText.SetText("생산 저장공간 " + data.spawnStorage);
            }

            if (image.TryGetComponent<Animator>(out var animator) || TryGetComponent(out animator))
                animator.runtimeAnimatorController = data.animatorController;

            image.sprite = data.sprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_folder != null)
                _folder.BeginItemHover(this, _data, eventData);
        }

        public void OnPointerExit(PointerEventData eventData) => EndHover();
        public void OnPointerDown(PointerEventData eventData) => EndHover();
        private void OnDisable() => EndHover();

        private void EndHover()
        {
            if (_folder != null)
                _folder.EndItemHover(this);
        }
    }
}
