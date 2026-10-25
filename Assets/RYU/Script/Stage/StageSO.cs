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
        public float waitTime;
        public int count;
    }
    
    [CreateAssetMenu(fileName = "StageSO", menuName = "RYU/StageSO", order = 0)]
    public class StageSO : ScriptableObject
    {
       public List<StageData> stageDates;

     
    }
}