using DevLib.ServiceLocator;
using RYU.Script.Service.GoldService;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _moenyText;
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
        if (_moenyText == null) return;

        _moenyText.SetText($"Money: {_gold.Gold}");
    }
}
