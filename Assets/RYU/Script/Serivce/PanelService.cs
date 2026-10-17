using DevLib.ServiceLocator;
using UnityEngine;

namespace RYU.Script.Serivce
{
    public class PanelService : MonoBehaviour, IPanelService
    {
        [field:SerializeField] public GameObject YoutubePanel { get; set; }
        [field:SerializeField] public GameObject VolumePanel { get; set; }
        [field:SerializeField] public GameObject CharcterPanel { get; set; }
        [field:SerializeField] public GameObject WindowPanel { get; set; }

        private void Awake()
        {
            ServiceLocator.Register<IPanelService>(this);
            VolumePanel.SetActive(false);
            CharcterPanel.SetActive(false);
            WindowPanel.SetActive(false);
            YoutubePanel.SetActive(false);
        }

     

        public void SetVolumePanelActive()
        {
            VolumePanel.SetActive(!VolumePanel.activeSelf);
        }

        public void SetCharacterPanelActive(bool active)
        {
            CharcterPanel.SetActive(active);
        }

        public void SetWindowPanelActive()
        {
            WindowPanel.SetActive(!WindowPanel.activeSelf);
        }

        public void SetYoutubePanelActive(bool active)
        {
            YoutubePanel.SetActive(active);
        }
    }

    
}