using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "UnitManyAttack", menuName = "AttackSO/UnitManyAttack", order = 0)]
    public class ManyEntityHitEnemy : AbstractAttackSO
    {
        public override void Attacking(AbstractUnit unit)
        {
            Collider2D[] col = unit.DetecPlayer();
            foreach (Collider2D c in col)
            {
                if(c.TryGetComponent<IDamageAble>(out IDamageAble d))d.Damage(unit.UnitSo.attackDmg);
            }
        }
    }
}