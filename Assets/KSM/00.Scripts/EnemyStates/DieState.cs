using UnityEngine;

namespace KSM._00.Scripts
{
    public class DieState : AbstractUnitState
    {
        public DieState(StateMachine stateMachine, AbstractUnit abstractUnit, string hashName) : base(stateMachine, abstractUnit, hashName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _abstractUnit.MoveCompo.InitMove(0,_abstractUnit.Dir);
            Debug.Log("Die");
            
        }

        public override void HandleAnimEnd()
        {
            base.HandleAnimEnd();
            if (_abstractUnit.UnitSo is EnemySO so)
            {
                Debug.Log($"백신 게이지 {so.vaccinegauge}오름");
            }
            Object.Destroy(_abstractUnit.gameObject);
            
        }
    }
}