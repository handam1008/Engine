using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _moenyText;
    [field:SerializeField] public float Money { get; private set;}

    private void Update()
    {
        if (_moenyText == null) return;

        _moenyText.SetText($"Money: {Mathf.Ceil(Money)}");
    }

    public void PlusMoney(float amount)
    {
        Money += amount;
    }

    public void MinusMoney(float amount)
    {
        Money -= amount;

        if (amount > Money) Money = 0;

    }
}
