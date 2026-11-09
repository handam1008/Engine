using System.Collections.Generic;
using UnityEngine;


    [CreateAssetMenu(fileName = "SO", menuName = "Enemy", order = 0)]
    public class  UnitSO : ScriptableObject
    {
        [Header("Default Enemy Setting")]
        public string Name;
        public Sprite sprite;
        public GameObject prefab;   
        public RuntimeAnimatorController animatorController;
        public float moveSpeed;
        public int maxHp;
        public LayerMask whatIsDetect;
        public UnitType unitType;
        public List<float> knockbackList = new();
        
        [Header("Attack Setting")]
        
        public float attackRange;
        public float attackCoolTime;
        public int attackDmg;
       
        


    }