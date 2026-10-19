using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RYU.Script.UI
{
    public class UIDragePanel : MonoBehaviour, IDragHandler
    {
        RectTransform _rectTransform;
        private Canvas _canvas;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }
    }
}