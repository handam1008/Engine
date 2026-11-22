using System.Collections;
using TMPro;
using UnityEngine;

namespace RYU.Script.UI
{
    public class NetworkStageWindow : MonoBehaviour
    {
        [SerializeField] private TaskbarWindow window;
        [SerializeField] private NetworkStageItem[] items;
        [SerializeField] private TextMeshProUGUI clearCountText;

        [Header("Dialogs")]
        [SerializeField] private GameObject errorDialog;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private GameObject connectDialog;
        [SerializeField] private TextMeshProUGUI connectText;
        [SerializeField] private RectTransform connectFill;
        [SerializeField] private float connectTime = 1f;

        private NetworkStageItem _current;

        private void OnEnable()
        {
            errorDialog.SetActive(false);
            connectDialog.SetActive(false);
            RefreshClearCount();
        }

        public void Select(NetworkStageItem item)
        {
            if (connectDialog.activeSelf) return;

            if (_current != null)
                ShowError($"이미 \\\\{_current.ComputerName}에 연결되어 있습니다.");
            else if (!item.Unlocked)
                ShowError($"\\\\{item.ComputerName}에 액세스할 수 없습니다.\n이전 컴퓨터를 먼저 정리하세요.");
            else if (item.Stage == null)
                ShowError($"\\\\{item.ComputerName}을(를) 찾을 수 없습니다.\n아직 준비 중인 컴퓨터입니다.");
            else
                StartCoroutine(Connect(item));
        }

        private IEnumerator Connect(NetworkStageItem item)
        {
            connectText.SetText($"\\\\{item.ComputerName}에 연결하는 중...");
            connectDialog.SetActive(true);
            for (float t = 0f; t < connectTime; t += Time.deltaTime)
            {
                connectFill.anchorMax = new Vector2(t / connectTime, 1f);
                yield return null;
            }

            _current = item;
            item.Stage.SetActive(true);
            gameObject.SetActive(false);
        }

        public void OnStageCleared()
        {
            if (_current != null)
            {
                _current.SetCleared();
                int next = System.Array.IndexOf(items, _current) + 1;
                if (next < items.Length)
                    items[next].Unlock();
                _current = null;
            }

            window.Show();
        }

        public void ShowError(string message)
        {
            errorText.SetText(message);
            errorDialog.SetActive(true);
        }

        public void CloseError()
        {
            errorDialog.SetActive(false);
        }

        private void RefreshClearCount()
        {
            int cleared = 0;
            foreach (var item in items)
                if (item.Cleared) cleared++;
            clearCountText.SetText($"정리 완료: {cleared} / {items.Length}");
        }
    }
}
