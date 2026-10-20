using UnityEngine;

    public class AttackState : AbstractUnitState
    {
        
        public AttackState(StateMachine stateMachine, AbstractUnit abstractUnit, string hashName) : base(stateMachine, abstractUnit, hashName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("attack");
        }

        public override void Exit()
        {
            base.Exit();
             _abstractUnit.lastAttackTime = Time.time;
             
        }

        public override void HandleAnimEnd()
        {
            base.HandleAnimEnd();
            _stateMachine.ChangeState(State.Chase);
        }

        public override void HandleAttacked()
        {
            base.HandleAttacked();
            _abstractUnit.Attack.Attacking(_abstractUnit);
        }
    }