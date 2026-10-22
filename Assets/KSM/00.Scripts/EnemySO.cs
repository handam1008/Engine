using System;
using UnityEngine;

namespace KSM._00.Scripts
{
    [CreateAssetMenu(fileName = "EnemySO", menuName = "KSM/EnemySO", order = 0)]
    public class EnemySO : UnitSO
    {
        public int vaccinegauge; //백신게이지임 적 죽였을 때 차는거
        public EnemyType type;

    }

    [Flags]
    public enum EnemyType
    {
        None       = 0,
        Virus      = 1 << 0,   // 1
        Wiper      = 1 << 1,   // 2
        Worm       = 1 << 2,   // 4
        Ransomware = 1 << 3,   // 8
        Adware     = 1 << 4,   // 16
    }
}