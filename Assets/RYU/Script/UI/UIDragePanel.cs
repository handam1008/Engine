using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RYU.Script.UI
{
    public class UIDragePanel : MonoBehaviour, IDragHandler
    {
        RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position += new Vector3(eventData.delta.x, eventData.delta.y);
        }
    }
}