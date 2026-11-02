using UnityEngine;

namespace KSM._00.Scripts
{
    public class CorpseState : AbstractUnitState
    {
        public float receiveTime = 3f;
        private float currentTime = 0f;
        public CorpseState(StateMachine stateMachine, AbstractUnit abstractUnit, string hashName) : base(stateMachine, abstractUnit, hashName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            currentTime = 0;
            _abstractUnit.ColliderCompo.enabled = false;
        }

        public override void Exit()
        {
            base.Exit();
            _abstractUnit.ColliderCompo.enabled = true;
        }

        public override void UpdateState()
        {
            base.UpdateState();
            currentTime += Time.deltaTime;
            if (currentTime >= receiveTime)
            {
                _abstractUnit.HpCompo.Receive();
                _stateMachine.ChangeState(State.Chase); 
            }
        }
    }
}