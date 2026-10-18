using UnityEngine;

namespace KSM._00.Scripts
{
    public class ChaseState : AbstractUnitState
    {
        public ChaseState(StateMachine stateMachine, AbstractUnit abstractUnit, string hashName) : base(stateMachine, abstractUnit, hashName)
        {
        }

        public override void Enter()
        {
            base.Enter();
                _abstractUnit.MoveCompo.InitMove(_abstractUnit.UnitSo.moveSpeed,_abstractUnit.Dir);
        }

        public override void Exit()
        {
            base.Exit();
            _abstractUnit.MoveCompo.InitMove(0,_abstractUnit.Dir);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            

            Collider2D target = _abstractUnit.NearTarget();
            if (target == null) return;
            _abstractUnit.target = target.transform;
            
            if (_abstractUnit.target != null)
            {
                _stateMachine.ChangeState(State.WaitAttack);
            }
            
        }
    }
}