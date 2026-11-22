using RYU.Script.Charcter;
using UnityEngine;

public class HpSupport : ISupportAction
{
    public void ApplyCorrection(CharacterDataSO charcter, int amount)
    {
        //charcter.maxHp += amount;        
    }

    public string SupportDiscription(int amount)
    {
        return $"체력 {amount}만큼 증가";
    }

}
