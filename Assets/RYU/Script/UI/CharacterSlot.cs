using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class CharacterSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image image;


        public void SetCharcterSlot(string text, Sprite sprite)
        {
            this.text.text = text;
            image.sprite = sprite;
        }
    }
}