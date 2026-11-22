using RYU.Script.Charcter;

public interface ISupportAction 
{
    void ApplyCorrection(CharacterDataSO charcter, int amount);
    string SupportDiscription(int amount);
}
