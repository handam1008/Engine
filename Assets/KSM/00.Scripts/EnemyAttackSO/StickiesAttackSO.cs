using KSM._00.Scripts.EnemyAttack;
using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "UnitAttackStickies", menuName = "AttackSO/UnitAttackStickies", order = 0)]
    public class StickiesAttackSO : AbstractAttackSO
    {
        [SerializeField] private float slowRate = 0.5f;
        [SerializeField] private float slowTime = 2f;
        [SerializeField] private GameObject notePrefab;

        public override void Attacking(AbstractUnit unit)
        {
            Collider2D near = unit.NearTarget();
            if (near == null) return;

            if (near.TryGetComponent(out HealthSystem hp)) hp.Damage(unit.UnitSo.attackDmg,unit);
            if (near.TryGetComponent(out AbstractUnit target)) target.ApplySlow(slowRate, slowTime, unit);
        }
    }
}