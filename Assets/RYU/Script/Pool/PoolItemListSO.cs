using System.Collections.Generic;
using UnityEngine;

namespace RYU.Script.PoolManager
{
    [CreateAssetMenu(fileName = "PoolItemListSO", menuName = "SO/RYU/PoolItemListSO", order = 0)]
    public class PoolItemListSO : ScriptableObject
    {
        public List<PoolItemSO> list;
    }
}