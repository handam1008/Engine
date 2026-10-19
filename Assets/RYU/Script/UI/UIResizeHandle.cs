using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RYU.Script.UI
{
    public class UIResizeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform targetPanel;
        [SerializeField] private Vector2 minimumSize = new Vector2(500, 350);
        [SerializeField] private Vector2 maximumSize = new Vector2(1920, 1080);
        
        private RectTransform _parentRect;
        
        private Vector2 _startMousePosition;
        private Vector2 _startOffsetMin;
        private Vector2 _startOffsetMax;
        private Vector2 _startSize;

        private void Awake()
        {
            _parentRect =  targetPanel.parent as RectTransform;
        }

        //OnBeginDrag는 드래그 시작할 때 위치 담는거
        public void OnBeginDrag(PointerEventData eventData)
        {
            //화면의 마우스 좌표를 UI에서 사용할 좌표로 바꾸는 함수임
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out _startMousePosition);
            
            _startOffsetMin = targetPanel.offsetMin;
            _startOffsetMax = targetPanel.offsetMax;
            _startSize = targetPanel.rect.size;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out Vector2 currentMousePosition);
            
            //얼마나 움직였는지 계산
            Vector2 movement = currentMousePosition - _startMousePosition;
            
            movement.x = Mathf.Clamp(movement.x, minimumSize.x - _startSize.x, maximumSize.x - _startSize.x);
            movement.y = Mathf.Clamp(movement.y, _startSize.y - maximumSize.y,  _startSize.y - minimumSize.y);
            
            targetPanel.offsetMax = new Vector2(_startOffsetMax.x + movement.x, _startOffsetMax.y);
            targetPanel.offsetMin = new Vector2(_startOffsetMin.x, _startOffsetMin.y + movement.y);
        }
    }   
}