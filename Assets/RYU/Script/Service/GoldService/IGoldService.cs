namespace RYU.Script.Service.GoldService
{
    public interface IGoldService
    {
        public int Gold { get;  set; }
        public void AddGold(int gold);
        public bool RemoveGold(int gold);
    }
}
