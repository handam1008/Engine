using RYU.Script.Charcter;

public interface ISupportAction 
{
    void ApplyCorrection(CharcterDataSo charcter, int amount);
    string SupportDiscription(int amount);
}
