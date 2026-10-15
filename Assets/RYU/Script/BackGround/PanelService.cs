using DevLib.ServiceLocator;
using RYU.Script.XP;
using UnityEngine;

namespace RYU.Script.BackGround
{
    public class PanelService : MonoBehaviour, IPanelService
    {
        [SerializeField] private GameObject volumePanel;
        [SerializeField] private GameObject charcterPanel;
        [SerializeField] private GameObject windowPanel;

        private void Awake()
        {
            ServiceLocator.Register<IPanelService>(this);
        }

        private void Start()
        {
            volumePanel.SetActive(false);
            charcterPanel.SetActive(false);
            windowPanel.SetActive(false);
        }


   

        public void volumePanelSetActivePanel()
        {
            volumePanel.SetActive(!volumePanel.activeSelf);
        }

        public void charcterPanelSetActivePanel(bool active)
        {
            charcterPanel.SetActive(active);
        }

        public void windowPanelSetActivePanel()
        {
            windowPanel.SetActive(!windowPanel.activeSelf);
        }
    }

    
}