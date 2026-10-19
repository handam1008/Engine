using UnityEngine;

namespace KSM._00.Scripts
{
    public class HitState : AbstractUnitState
    {
        public HitState(StateMachine stateMachine, AbstractUnit abstractUnit, string hashName) : base(stateMachine, abstractUnit, hashName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _abstractUnit.MoveCompo.InitMove(2f,-_abstractUnit.Dir);
            Debug.Log("Hit");
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void HandleAnimEnd()
        {
            base.HandleAnimEnd();
            _stateMachine.ChangeState(State.Chase);
        }
    }
}