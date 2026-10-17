using System;
using UnityEngine;

public class UIFullScreenPanel : MonoBehaviour
{
   private RectTransform _rectTransform;
   private bool _isFull;
   private Vector2 _beforeoffestMix;
   private Vector2 _beforeoffestMax;

   private void Awake()
   {
      _rectTransform = GetComponent<RectTransform>();
   }

   public void FullScreen()
   {
      if (!_isFull)
      {
         _beforeoffestMix = _rectTransform.offsetMin;
         _beforeoffestMax = _rectTransform.offsetMax;
         _rectTransform.offsetMin = new Vector2(0, 60);
         _rectTransform.offsetMax = new Vector2(0, 0);
         _isFull = true;
      }
      else
      {
         _rectTransform.offsetMin = _beforeoffestMix;
         _rectTransform.offsetMax = _beforeoffestMax;
         _isFull = false;
      }
         
   }
}
