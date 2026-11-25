using DevLib.ServiceLocator;
using RYU.Script.Service.GoldService;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RYU.Script.Gacha
{
    //MainScene의 뽑기 창. 1회 뽑기를 누르면 골드를 차감하고 뽑기 씬(TT)으로 넘어간다
    public class GachaShopWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _balanceText;
        [SerializeField] private string _balanceFormat = "보유 골드   {0:N0}";
        [SerializeField] private int _drawOncePrice = 3000;
        [SerializeField] private string _gachaSceneName = "TT";

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
            _balanceText.SetText(string.Format(_balanceFormat, _gold.Gold));
        }

        //1회 뽑기 버튼 onClick
        public void DrawOnce()
        {
            if (_gold == null || !_gold.RemoveGold(_drawOncePrice)) return;

            SceneManager.LoadScene(_gachaSceneName);
        }
    }
}
