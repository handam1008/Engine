using System.Collections;
using DevLib.ServiceLocator;
using RYU.Script.Charcter;
using RYU.Script.Service.PanelService;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private Button exitButton;
        [SerializeField] private TextMeshProUGUI itemTooltipText;

        private RectTransform _itemTooltip;
        private CharacterSlot _hoveredSlot;
        private Coroutine _hoverDelay;

        private void Awake()
        {
            if (itemTooltipText != null)
                _itemTooltip = (RectTransform)itemTooltipText.transform.parent;
        }

        private void Start()
        {
            exitButton.onClick.AddListener(() => ServiceLocator.Get<IPanelService>().SetCharacterPanelActive(false));
        }

        private void OnDisable() => HideTooltip();

        public void BeginItemHover(CharacterSlot slot, CharacterDataSO data, PointerEventData pointer)
        {
            if (!isActiveAndEnabled || _itemTooltip == null)
                return;

            HideTooltip();
            _hoveredSlot = slot;
            _hoverDelay = StartCoroutine(ShowTooltipAfterDelay(data, pointer));
        }

        public void EndItemHover(CharacterSlot slot)
        {
            if (_hoveredSlot == slot)
                HideTooltip();
        }

        private IEnumerator ShowTooltipAfterDelay(CharacterDataSO data, PointerEventData pointer)
        {
            yield return new WaitForSecondsRealtime(0.55f);
            _hoverDelay = null;

            var window = (RectTransform)_itemTooltip.parent;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    window, pointer.position, pointer.enterEventCamera, out var position))
                yield break;

            itemTooltipText.text = $"{data.Name}\n종류: 캐릭터\n크기: {data.spawnStorage} KB";
            Vector2 size = itemTooltipText.GetPreferredValues();
            _itemTooltip.sizeDelta = new Vector2(Mathf.Ceil(size.x) + 12f, Mathf.Ceil(size.y) + 8f);

            Rect bounds = window.rect;
            if (position.x + 12f + _itemTooltip.rect.width > bounds.xMax)
                position.x -= 24f + _itemTooltip.rect.width;
            if (position.y - 18f - _itemTooltip.rect.height < bounds.yMin)
                position.y += 30f + _itemTooltip.rect.height;

            _itemTooltip.localPosition = position + new Vector2(12f, -18f);
            _itemTooltip.gameObject.SetActive(true);
        }

        private void HideTooltip()
        {
            if (_hoverDelay != null)
                StopCoroutine(_hoverDelay);
            _hoverDelay = null;
            _hoveredSlot = null;

            if (_itemTooltip != null)
                _itemTooltip.gameObject.SetActive(false);
        }
    }
}
