using System;
using DevLib.ServiceLocator;
using RYU.Script.BackGround;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.XP
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private Button exitButton;

        private void Start()
        {
            exitButton.onClick.AddListener(() => ServiceLocator.Get<IPanelService>().charcterPanelSetActivePanel(false));
        }
    }
}
