using System;

namespace RYU.Script.Service.GoldService
{
    public interface IGoldService
    {
        public int Gold { get; }
        public event Action OnChanged;
        public void AddGold(int gold);
        public bool RemoveGold(int gold);
    }
}
