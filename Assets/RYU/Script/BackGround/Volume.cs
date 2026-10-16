using DevLib.ServiceLocator;
using RYU.Script.XP;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.BackGround
{
    public class Volume : MonoBehaviour
    {
        [SerializeField] private Button volumeButton;
        [SerializeField] private Button volumeCloseButton;

        private void Start()
        {
            volumeButton.onClick.AddListener(() => ServiceLocator.Get<IPanelService>().volumePanelSetActivePanel());
            volumeCloseButton.onClick.AddListener(() => ServiceLocator.Get<IPanelService>().volumePanelSetActivePanel());
        }
        
    }
    
}