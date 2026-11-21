using UnityEngine;

namespace RYU.Script.Service.GoldService
{
    public class GoldService : MonoBehaviour, IGoldService
    {
        public int Gold { get; set; }

        public void AddGold(int gold)
        {
            Gold += gold;
        }

        public bool RemoveGold(int gold)
        {
            if (Gold >= gold)
            {
                Gold -= gold;
                return true;
            }
            return false;
        }
    }
}
