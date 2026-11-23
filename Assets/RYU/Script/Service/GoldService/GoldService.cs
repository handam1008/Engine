using System;
using DevLib.ServiceLocator;
using UnityEngine;

namespace RYU.Script.Service.GoldService
{
    public class GoldService : MonoBehaviour, IGoldService
    {
        public int Gold { get; private set; }
        public event Action OnChanged;

        private void Awake()
        {
            ServiceLocator.Register<IGoldService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IGoldService>();
        }

        public void AddGold(int gold)
        {
            Gold += gold;
            OnChanged?.Invoke();
        }

        public bool RemoveGold(int gold)
        {
            if (Gold >= gold)
            {
                Gold -= gold;
                OnChanged?.Invoke();
                return true;
            }
            return false;
        }
    }
}
