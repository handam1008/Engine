using UnityEngine;

namespace RYU.Script.PoolManager
{
    public interface IPoolable
    {
        public string ItemName { get;  }
        public GameObject GetGameObject();
        public void ResetItem();
    }
}