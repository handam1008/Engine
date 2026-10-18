using System;
using DynamicPanels;
using UnityEngine;

public class UIFullScreenPanel : MonoBehaviour
{
    private Panel _panel;
    private bool _isFull;

    private Vector2 _beforePosition;
    private Vector2 _beforeSize;

    public void FullScreen()
    {
        // 실제 창은 실행 중 생성되므로 버튼을 누를 때 찾는다.
        if (_panel == null)
            _panel = GetComponentInParent<Panel>();

        RectTransform window = _panel.RectTransform;

        if (!_isFull)
        {
            // 최대화하기 전 상태 저장
            _beforePosition = window.anchoredPosition;
            _beforeSize = window.sizeDelta;

            // 작업 영역 전체를 채운다.
            window.sizeDelta = _panel.Canvas.RectTransform.rect.size;
            window.anchoredPosition = Vector2.zero;
        }
        else
        {
            // 이전 상태 복원
            window.sizeDelta = _beforeSize;
            window.anchoredPosition = _beforePosition;
        }

        _isFull = !_isFull;
    }
         
}

