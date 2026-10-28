using System;
using System.Collections.Generic;
using KSM._00.Scripts;
using UnityEngine;

namespace RYU.Script.Stage
{
    [Serializable]
    public struct StageData
    {
        public EnemySO enemy;
        public float nextWaitTime;
        public int count;
        public float percent;
        public bool isSpawn;
    }
    
    [CreateAssetMenu(fileName = "StageSO", menuName = "RYU/StageSO", order = 0)]
    public class StageSO : ScriptableObject
    {
       public List<StageData> stageDates;

     
    }
}