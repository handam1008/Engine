using DevLib.ServiceLocator;
using RYU.Script.Service.PanelService;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class Volume : MonoBehaviour
    {
        [SerializeField] private Button volumeCloseButton;

        private void Start()
        {
          volumeCloseButton.onClick.AddListener(() => ServiceLocator.Get<IPanelService>().SetVolumePanelActive());
        }
        
    }
    
}