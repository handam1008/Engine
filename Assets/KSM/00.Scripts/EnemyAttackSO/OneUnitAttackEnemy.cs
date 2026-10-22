using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "OneUnitAttackEnemy", menuName = "AttackSO/OneUnitAttackEnemy", order = 0)]

    public class OneUnitAttackEnemy : AbstractAttackSO
    {
        public override void Attacking(AbstractUnit unit)
        {
            if(unit.target.TryGetComponent(out IDamageAble damageAble)) damageAble.Damage(unit.UnitSo.attackDmg);
            
        }
    }
}