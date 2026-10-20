using System;
using DynamicPanels;
using TMPro;
using UnityEngine;

public class UIFullScreenPanel : MonoBehaviour
{
    private Panel _panel;
    private bool _isFull;

    private Vector2 _beforePosition;
    private Vector2 _beforeSize;

    public void FullScreen()
    {
       if(_panel == null)
           _panel=  GetComponentInParent<Panel>();
       
       RectTransform window = _panel.RectTransform;

       if (!_isFull)
       {
           _beforePosition = window.anchoredPosition;
           _beforeSize = window.sizeDelta;

           window.sizeDelta = _panel.Canvas.RectTransform.rect.size;
           window.anchoredPosition = Vector2.zero;
           _isFull = true;
       }
       else if (_isFull)
       {
           window.sizeDelta = _beforeSize;
           window.anchoredPosition = _beforePosition;
           _isFull = false;
       }
    }
         
}

