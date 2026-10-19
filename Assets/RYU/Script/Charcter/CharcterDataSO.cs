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
    public class CharcterDataSO : UnitSO
    {
        [Header("기본")]
        public CharcterRank charcterRank;
        
        [Header("저장공간")]
        public int spawnStorage;
        public int attackStorage; 
        

    }
}