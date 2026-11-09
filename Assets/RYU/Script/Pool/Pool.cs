using System.Collections.Generic;
using UnityEngine;

namespace RYU.Script.PoolManager
{
    public class Pool
    {
        private Stack<IPoolable> _pool;
        private Transform _parent;
        private IPoolable _poolable;
        private GameObject _prefab;

        public Pool(IPoolable poolable, Transform parent, int count)
        {
            _pool = new Stack<IPoolable>();
            _poolable = poolable;
            _parent = parent;
            _prefab = _poolable.GetGameObject();

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Object.Instantiate(_prefab, _parent);
                obj.SetActive(false);
                obj.name = _poolable.ItemName;
                IPoolable item = obj.GetComponent<IPoolable>();
                _pool.Push(item);
            }
        }

        public IPoolable Pop()
        {
            IPoolable item = null;
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
                item.GetGameObject().SetActive(true);
            }
            else
            {
                GameObject obj = Object.Instantiate(_prefab, _parent);
                obj.name = _poolable.ItemName;
                item = obj.GetComponent<IPoolable>();
            }

            return item;
        }

        public void Push(IPoolable item)
        {
            item.GetGameObject().SetActive(false);
            _pool.Push(item);
        }
    }
}