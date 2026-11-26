using System;
using System.Collections.Generic;
using KSM._00.Scripts;
using UnityEngine;

namespace RYU.Script.Stage
{
    [Serializable]
    public class StageData
    {
        public EnemySO enemy;
        public float nextWaitTime;
        public int count;
        public float countInterval = 0.5f;
        public float percent;
        public bool once;
        public EnemySO reviveEnemy;
        public bool isSpawn;
    }
    
    [CreateAssetMenu(fileName = "StageSO", menuName = "RYU/StageSO", order = 0)]
    public class StageSO : ScriptableObject
    {
        public List<StageData> stageDates;
        public int getGold;

     
    }
}