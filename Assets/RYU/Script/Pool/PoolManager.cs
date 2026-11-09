using System;
using System.Collections.Generic;
using UnityEngine;

namespace RYU.Script.PoolManager
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance;
        [SerializeField] private PoolItemListSO poolList;
        private readonly Dictionary<string, Pool> _pools = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            foreach (var item in poolList.list)
            {
                CreatePool(item.prefab, item.count);
            }
        }

        private void CreatePool(GameObject itemPrefab, int itemCount)
        {
            IPoolable poolable = itemPrefab.GetComponent<IPoolable>();
            _pools.Add(poolable.ItemName, new Pool(poolable,transform,itemCount));
        }

        public IPoolable Pop(string itemName)
        {
            if (_pools.ContainsKey(itemName))
            {
                IPoolable item = _pools[itemName].Pop();
                item.ResetItem();
                return item;
            }

            return null;
        }

        public void Push(IPoolable item)
        {
            if (_pools.ContainsKey(item.ItemName))
            {
                _pools[item.ItemName].Push(item);
                return;
            }
            Debug.LogError($"Pool {item.ItemName} doesn't exist");
        }
    }
}