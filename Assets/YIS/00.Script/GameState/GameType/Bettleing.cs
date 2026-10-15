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
    }

    public override void Exit()
    {
        base.Exit();
    }
}
