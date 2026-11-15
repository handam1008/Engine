using KSM._00.Scripts.EnemyAttack;
using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "AppStoreEnemy", menuName = "AttackSO/AppStoreEnemy", order = 0)]
    public class AppStore : AbstractAttackSO
    {
        public Wave wavePrefab;
        public override void Attacking(AbstractUnit unit)
        {
            if (unit.target.TryGetComponent(out AbstractUnit target))
            {
                Wave wave = Instantiate(wavePrefab, new Vector3(target.transform.position.x,target.transform.position.y-0.5f), Quaternion.identity);
                wave.WavePower(unit.UnitSo.whatIsDetect);
                
                target.HpCompo.Damage(unit.UnitSo.attackDmg);
            }
            
            
            
            
        }
    }
}