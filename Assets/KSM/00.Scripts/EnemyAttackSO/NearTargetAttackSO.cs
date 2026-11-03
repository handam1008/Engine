using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "HappyMacEnemy", menuName = "AttackSO/HappyMacEnemy", order = 0)]
    public class NearTargetAttackSO : AbstractAttackSO
    {
        public override void Attacking(AbstractUnit unit)
        {
            Collider2D target = unit.NearTarget();
            if(target != null && target.TryGetComponent(out HealthSystem healthSystem))
                healthSystem.Damage(unit.UnitSo.attackDmg);
            
        }
    }
}