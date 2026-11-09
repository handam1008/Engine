using UnityEngine;

public class BettleStart : GameState
{
    public BettleStart(GameStateMachine machine, GameLoop loop) : base(machine, loop)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("StartBettle");
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        // 전투 시작하면 넣어줘야 될것 근데 굳이 스타트 없어도 베틀링의 엔터에서 해도 될듯


    }

}
