using UnityEngine;

namespace RYU.Script.Charcter.HardDisk
{
    [CreateAssetMenu(fileName = "HardDiskAttackSO", menuName = "SO/Character/HardDisk", order = 0)]
    public class HardDiskAttackSO : AbstractAttackSO
    {
        public override void Attacking(AbstractUnit unit)
        {
            unit.HpCompo.OnDeath += (() => Debug.Log("하드디스크 뒤짐!"));
        }
        
        
    }
}