using UnityEngine;

public class BettleDefeat : GameState
{
    public BettleDefeat(GameStateMachine machine, GameLoop loop) : base(machine, loop)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("패배....");
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        // 대충 졌을 때 해줘야 할거 

    }

}
