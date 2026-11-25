using System.Collections.Generic;
using KSM._00.Scripts;
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
    public class CharacterDataSO : UnitSO
    {
        [Header("기본")]
        public CharcterRank charcterRank;
        public int number = 0;
        [Header("강한상대")]
       [field:SerializeField] public List<TypeDamage> WhatIsStrong { get; private set; }
        
        [Header("레벨")]
        public int level = 1; // 맥스는 20
        
        
        [Header("저장공간")]
        public int spawnStorage;
        public int attackStorage;

        [field:SerializeField] public float Weight { get; private set; }
        public float spawnCoolTime;

        
    }
}