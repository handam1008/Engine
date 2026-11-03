using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "MacWriteEnemy", menuName = "AttackSO/MacWriteEnemy", order = 0)]
    
    public class MacWriteAttack : AbstractAttackSO
    {
        public Vector2 attackRange;
        public override void Attacking(AbstractUnit unit)
        {
            Vector2 center = (Vector2)unit.transform.position + new Vector2(unit.Dir * attackRange.x/2, 0f);
            Collider2D[] hit =
                Physics2D.OverlapBoxAll(center, attackRange,0, unit.UnitSo.whatIsDetect);
            foreach (var col in hit)
            {
                
                if (col.TryGetComponent(out HealthSystem heal)) heal.Damage(unit.UnitSo.attackDmg);
                
            }
            
        }
    }
}