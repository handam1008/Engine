using System;
using DevLib.ServiceLocator;
using RYU.Script.BackGround;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RYU.Script.XP
{
    public class XpDesktopIconSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Image _image;
        private Material _material;
        private TextMeshProUGUI _text;
        private readonly int _selectionAmount = Shader.PropertyToID("_SelectionAmount");
        private readonly int _color = Shader.PropertyToID("_SelectionColor");

        private void Awake()
        {
            _image = GetComponent<Image>();
            _material =  _image.material;
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            _material.SetFloat(_selectionAmount, 0f);
            _text.color =  Color.white;
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
                ServiceLocator.Get<IPanelService>().charcterPanelSetActivePanel(true);
            }
           
        }
    }
}
