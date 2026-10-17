using DevLib.ServiceLocator;
using RYU.Script.Serivce;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private Button exitButton;

        private void Start()
        {
            exitButton.onClick.AddListener(() => ServiceLocator.Get<IPanelService>().SetCharacterPanelActive(false));
        }
    }
}
