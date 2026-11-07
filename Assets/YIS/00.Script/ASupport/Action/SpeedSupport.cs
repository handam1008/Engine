using RYU.Script.Charcter;

internal class SpeedSupport : ISupportAction
{
    public void ApplyCorrection(CharcterDataSo charcter, int amount)
    {
       // charcter.moveSpeed += amount;
    }

    public string SupportDiscription(int amount)
    {
        return $"속력 {amount}만큼 증가";
    }
}
