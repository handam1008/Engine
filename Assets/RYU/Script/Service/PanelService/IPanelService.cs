using UnityEngine;

namespace RYU.Script.Service.PanelService
{
    public interface IPanelService
    {
        public GameObject VolumePanel { get; set; }
        public GameObject CharcterPanel { get; set; }
        public GameObject WindowPanel { get; set; }
        public GameObject MyComputerPanel { get; set; }
        
        void SetVolumePanelActive();
        void SetCharacterPanelActive(bool active);
        void SetWindowPanelActive();
        void SetMyComputerPanelActive(bool active);
    }
}