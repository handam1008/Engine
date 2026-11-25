using DevLib.ServiceLocator;
using RYU.Script.Service.GoldService;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RYU.Script.Gacha
{
    public class GachaShopWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _balanceText;
        [SerializeField] private int _drawOncePrice = 3000;
        [SerializeField] private string _gachaSceneName = "GachaScene";

        private IGoldService _gold;

        private void Start()
        {
            _gold = ServiceLocator.Get<IGoldService>();
            if (_gold == null) return;

            _gold.OnChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_gold != null) _gold.OnChanged -= Refresh;
        }

        private void Refresh()
        {
            _balanceText.SetText("보유 골드 " + _gold.Gold);
        }

        public void DrawOnce()
        {
            if (_gold == null || !_gold.RemoveGold(_drawOncePrice)) return;

            SceneManager.LoadScene(_gachaSceneName);
        }
    }
}
