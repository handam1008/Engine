 using UnityEngine;

namespace RYU.Script.Pool
{
    [CreateAssetMenu(fileName = "PoolItemSO", menuName = "SO/RYU/PoolItemSO", order = 0)]
    public class PoolItemSO : ScriptableObject
    {
        public string poolName;
        public GameObject prefab;
        public int count;

        private void OnValidate()
        {
            if (prefab == null) return;
            IPoolable item = prefab.GetComponent<IPoolable>();
            Debug.Log(item);
            if (item == null)
            {   
                prefab = null;
            }
            else
            {
                poolName = item.ItemName;
            }
        }
    }
}