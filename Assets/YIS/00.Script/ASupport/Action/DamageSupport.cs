using RYU.Script.Charcter;

internal class DamageSupport : ISupportAction
{
    public void ApplyCorrection(CharcterDataSo charcter, int amount)
    {
      //  charcter.attackDmg += amount;
    }

    public string SupportDiscription(int amount)
    {
        return $"공격력 {amount}만큼 증가";
    }
}

