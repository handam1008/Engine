using UnityEngine;

namespace RYU.Script.UI
{
    public class TaskbarWindow : MonoBehaviour
    {
        [SerializeField] private GameObject taskButton;
        [SerializeField] private Transform taskbar;

        private bool _minimized;

        private void OnEnable()
        {
            _minimized = false;
            if (taskButton != null)
                taskButton.SetActive(true);
        }

        private void OnDisable()
        {
            if (!_minimized && taskButton != null)
                taskButton.SetActive(false);
        }

        public void Minimize()
        {
            _minimized = true;
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (taskbar == null || transform.parent != taskbar.parent)
                return;

            int taskbarIndex = taskbar.GetSiblingIndex();
            int targetIndex = transform.GetSiblingIndex() < taskbarIndex ? taskbarIndex - 1 : taskbarIndex;
            transform.SetSiblingIndex(targetIndex);
        }
    }
}
