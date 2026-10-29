using UnityEngine;

namespace RYU.Script.Charcter
{
    public enum CharcterRank
    {
        NORMAL,
        RARE,
        HIDDEN
    }
    
    [CreateAssetMenu(fileName = "CharcterDataSo", menuName = "RYU/CharcterDataSo", order = 0)]
    public class CharcterDataSo : UnitSO
    {
        [Header("기본")]
        public CharcterRank charcterRank;
        
        [Header("레벨")]
        public int level = 1; // 맥스는 20
        
        
        [Header("저장공간")]
        public int spawnStorage;
        public int attackStorage;

        [field:SerializeField] public float weight { get; private set; }

        
    }
}