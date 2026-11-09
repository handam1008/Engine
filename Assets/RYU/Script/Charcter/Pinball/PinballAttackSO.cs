using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RYU.Script.Charcter.PinBoll
{
    [CreateAssetMenu(fileName = "PinballAttackSO", menuName = "SO/Attack/PinballAttackSO", order = 0)]
    public class PinballAttackSO : AbstractAttackSO
    {
        [SerializeField] private int delay = 1000;
        public override void Attacking(AbstractUnit unit)
        {
            _ = Destroy(unit);
        }

        async UniTaskVoid Destroy(AbstractUnit unit)
        {
            unit.MoveCompo.InitMove(0, unit.Dir);
            await UniTask.Delay(delay);
            unit.StateMachine.ChangeState(State.Dead);
        }
    }
}