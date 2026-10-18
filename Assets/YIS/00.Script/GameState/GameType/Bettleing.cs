using UnityEngine;

public class Bettleing : GameState
{
    public Bettleing(GameStateMachine machine, GameLoop loop) : base(machine, loop)
    {
    }

    public override void Enter()
    {
        Debug.Log("Bettling");
        base.Enter();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        // 대충 내 기지 체력나 적 기지 체력 다 닳았을 때를 기준으로 해서 하면 될듯
    }

    public override void Exit()
    {
        base.Exit();
    }
}
