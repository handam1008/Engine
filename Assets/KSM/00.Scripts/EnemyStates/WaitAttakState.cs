using UnityEngine;

namespace KSM._00.Scripts
{
    public class WaitAttakState : AbstractUnitState
    {
        public bool _firstAttack = true;

        public WaitAttakState(StateMachine stateMachine, AbstractUnit abstractUnit, string hashName) : base(
            stateMachine, abstractUnit, hashName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Wait");
        }

        public override void UpdateState()
        {
            base.UpdateState();
            Collider2D target = _abstractUnit.NearTarget();
            if (target == null)
            {
                _stateMachine.ChangeState(State.Chase);
                return;
            }

            _abstractUnit.target = target.transform;

            if ((target != null && _abstractUnit.lastAttackTime + _abstractUnit.UnitSo.attackCoolTime < Time.time) 
                || _firstAttack)
            {
                _stateMachine.ChangeState(State.Attack);
                _firstAttack = false;
                return;
            }
            
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}