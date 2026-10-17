using System;
using DevLib.ServiceLocator;
using RYU.Script.Serivce;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class DesktopIconSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Image _image;
        private Material _material;
        private TextMeshProUGUI _text;
        private readonly int _selectionAmount = Shader.PropertyToID("_SelectionAmount");
        private readonly int _color = Shader.PropertyToID("_SelectionColor");

        public UnityEvent onDoubleClick;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _material = new Material(_image.material);
            _image.material = _material;
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            _material.SetFloat(_selectionAmount, 0f);
            _text.color =  Color.white;
        }

        private void OnDestroy()
        {
            if(_material != null)
                Destroy(_material);
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            _material.SetFloat(_selectionAmount, 0.5f);
            _text.color = _material.GetColor(_color);
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _material.SetFloat(_selectionAmount, 0f);
            _text.color =  Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount >= 2)
            {
                onDoubleClick?.Invoke();
            }
           
        }
    }
}
