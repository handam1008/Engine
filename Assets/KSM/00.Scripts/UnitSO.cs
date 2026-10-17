using UnityEngine;


    [CreateAssetMenu(fileName = "SO", menuName = "Enemy", order = 0)]
    public class UnitSO : ScriptableObject
    {
        [Header("Default Enemy Setting")]
        public string enemyName;
        public Sprite sprite;
        public RuntimeAnimatorController animatorController;
        public float detectRange;
        public float moveSpeed;
        public int maxHp;
        public float backcineGage;
        public LayerMask whatIsDetect;
        public UnitType unitType;
        
        [Header("Attack Setting")]
        
        public float attackRange;
        public float attackCoolTime;
        public int attackDmg;
       
        


    }