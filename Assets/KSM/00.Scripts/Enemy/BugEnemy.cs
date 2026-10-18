using System;
using KSM._00.Scripts;
using UnityEngine;

    public class BugEnemy : AbstractUnit
    {
        private StateMachine _stateMachine;
        protected override void Awake()
        {
            base.Awake();
            _stateMachine = new();
            _stateMachine.Add(State.Attack,new AttackState(_stateMachine,this,"Attack"));
            _stateMachine.Add(State.Chase,new ChaseState(_stateMachine,this,"Chase"));
            _stateMachine.Add(State.Hit,new HitState(_stateMachine,this,"Hit"));
            _stateMachine.Add(State.Dead, new DieState(_stateMachine,this,"Dead"));
            _stateMachine.Add(State.WaitAttack, new WaitAttakState(_stateMachine,this,"WaitAttack"));
            _stateMachine.Init(State.Chase);
            
        }

        private void Start()
        {
            HpCompo.OnDeath += () => _stateMachine.ChangeState(State.Dead);
            HpCompo.OnDamged +=  () => _stateMachine.ChangeState(State.Hit);
        }

        private void Update()
        {
            _stateMachine.CurrentState.UpdateState();
        }
    }