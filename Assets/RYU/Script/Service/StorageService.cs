using System;
using DevLib.ServiceLocator;
using RYU.Script.Serivce;
using UnityEngine;

namespace RYU.Script.Service
{
    public class StorageService : MonoBehaviour, IStorageService
    {
        public int CurrentStorage { get; private set; }
        public int MaxStorage { get; private set; }
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