using Unity.VisualScripting;
using UnityEngine;

public class BettleWin : GameState
{
    MoneyManager moneyManager;

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

        moneyManager.PlusMoney(1000); // <= 이건 언제든지 변동 ㄱㄴ

        _machine.ChangeState(CurrentGameType.Prepare);
    }
}
