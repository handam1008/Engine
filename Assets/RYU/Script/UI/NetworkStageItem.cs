using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class NetworkStageItem : MonoBehaviour
    {
        [SerializeField] private string computerName;
        [SerializeField] private GameObject stage;
        [SerializeField] private bool unlocked;
        [SerializeField] private NetworkStageWindow window;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI statusText;

        public string ComputerName => computerName;
        public GameObject Stage => stage;
        public bool Unlocked => unlocked;
        public bool Cleared { get; private set; }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => window.Select(this));
            Refresh();
        }

        public void Unlock()
        {
            unlocked = true;
            Refresh();
        }

        public void SetCleared()
        {
            Cleared = true;
            Refresh();
        }

        private void Refresh()
        {
            nameText.SetText(computerName);
            icon.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.35f);

            if (Cleared)
            {
                statusText.SetText("정리 완료");
                statusText.color = new Color(0.13f, 0.55f, 0.13f);
            }
            else if (unlocked)
            {
                statusText.SetText("감염됨");
                statusText.color = new Color(0.8f, 0.1f, 0.1f);
            }
            else
            {
                statusText.SetText("액세스 불가");
                statusText.color = new Color(0.5f, 0.5f, 0.5f);
            }
        }
    }
}
