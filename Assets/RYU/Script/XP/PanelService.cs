using System;
using DevLib.ServiceLocator;
using UnityEngine;

namespace RYU.Script.XP
{
    public class PanelService : MonoBehaviour, IPanelService
    {
        [SerializeField] private GameObject volumePanel;
        [SerializeField] private GameObject charcterPanel;

        private void Awake()
        {
            ServiceLocator.Register<IPanelService>(this);
        }

        private void Start()
        {
            volumePanel.SetActive(false);
            charcterPanel.SetActive(false);
        }


        public void volumePanelSetActivePanel(bool active)
        {
           volumePanel.SetActive(active);
        }

        public void charcterPanelSetActivePanel(bool active)
        {
            charcterPanel.SetActive(active);
        }
    }

    
}