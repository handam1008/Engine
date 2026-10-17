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
    public class CharcterDataSO : ScriptableObject
    {
        [Header("기본")]
        public string charcterName;
        public Sprite sprite;
        public RuntimeAnimatorController animatorController;
        public float moveSpeed;
        public int maxHp;
        public float backcineGage;
        public LayerMask whatIsDetect;
        public CharcterRank charcterRank;
        //public UnitType unitType;
        
        
        [Header("저장공간")]
        public int spawnStorage;
        public int attackStorage; 

        [Header("Attack Setting")]
        public float attackRange;
        public float attackCoolTime;
        public int attackDmg;

    }
}