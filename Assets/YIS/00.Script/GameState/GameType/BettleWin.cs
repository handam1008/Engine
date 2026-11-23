using Unity.VisualScripting;
using UnityEngine;

public class BettleWin : GameState
{
    public BettleWin(GameStateMachine machine, GameLoop loop) : base(machine, loop)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("전투 승리!!");
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        // 전투 승리 해쓸때 넣어줘야 할것 ex: 돈

        // 보상 지급은 TestGameEndTrigger.End(int gold)에서 처리

        _machine.ChangeState(CurrentGameType.Prepare);
    }
}
