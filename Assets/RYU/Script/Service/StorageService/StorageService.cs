using System;
using DevLib.ServiceLocator;
using UnityEngine;

namespace RYU.Script.Service.StorageService
{
    public class StorageService : MonoBehaviour, IStorageService
    {
        [SerializeField] private int maxStorage = 1440;

        public int CurrentStorage { get; private set; }
        public int MaxStorage => maxStorage;
        public event Action OnChanged;

        private void Awake()
        {
            ServiceLocator.Register<IStorageService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnRegister<IStorageService>();
        }

        public bool AddStorage(int kb)
        {
            CurrentStorage += kb;                         
            OnChanged?.Invoke();                    
            return true;
        }

        public void MinusStorage(int kb)
        {
            CurrentStorage = Mathf.Max(0, CurrentStorage - kb);  
            OnChanged?.Invoke();
        }
    }
}