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
    }

}
