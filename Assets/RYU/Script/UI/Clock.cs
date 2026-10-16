using System;
using TMPro;
using UnityEngine;

namespace RYU.Script.UI
{
    public class Clock : MonoBehaviour
    {
        private TextMeshProUGUI _clockText;


        private void Awake()
        {
            _clockText = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            DateTime koreaTime = DateTime.Now;
            _clockText.SetText(koreaTime.ToString("tt hh:mm"));
        }
    }
}