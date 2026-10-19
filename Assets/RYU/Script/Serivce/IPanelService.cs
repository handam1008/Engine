using UnityEngine;

namespace RYU.Script.Serivce
{
    public interface IPanelService
    {
        public GameObject YoutubePanel { get; set; }
        public GameObject VolumePanel { get; set; }
        public GameObject CharcterPanel { get; set; }
        public GameObject WindowPanel { get; set; }
        public GameObject MyComputerPanel { get; set; }
        
        void SetVolumePanelActive();
        void SetCharacterPanelActive(bool active);
        void SetWindowPanelActive();
        void SetYoutubePanelActive(bool active);
        void SetMyComputerPanelActive(bool active);
    }
}