using System.Collections.Generic;
using UnityEngine;
using UnityPipeline.Microsoft.CodeAnalysis.FlowAnalysis;

namespace KSM._00.Scripts.Enemy
{
    public class MailEnemy : AbstractUnit
    {
        [SerializeField] private int count;

        private static readonly Vector2 SlamSize = new Vector2(2f, 2f);

        private Vector2 SlamCenter =>
            (Vector2)transform.position + Vector2.right * (Dir * UnitSo.attackRange);

        protected override void Awake()
        {
            base.Awake();
            UnitAnimCompo.OnAttacked += HandleAttacked;
        }

        public override void EndAttack()
        {
            base.EndAttack();
            count = 0;
        }

        private void HandleAttacked()
        {
            count++;

            if (count == 4)
            {
                Slam();
                return;
            }

            if (count > 3 || target == null) return;

            if (target.TryGetComponent(out AbstractUnit v))
            {
                v.HpCompo.Damage(UnitSo.attackDmg, this);
            }
        }

        private void Slam()
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                SlamCenter, SlamSize, 0f, UnitSo.whatIsDetect);

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out AbstractUnit v))
                    v.HpCompo.Damage(UnitSo.attackDmg * 2, this);
            }
        }

        private void OnDestroy()
        {
                UnitAnimCompo.OnAttacked -= HandleAttacked;
                EndAttack();
            
        }

        private void OnDrawGizmos()
        {
            if (UnitSo == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(SlamCenter, SlamSize);
        }
    }
}