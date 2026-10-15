using UnityEngine;

public class BettleEnd : GameState
{
    public BettleEnd(GameStateMachine machine, GameLoop loop) : base(machine, loop)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("end");
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        //뭔가가 빠라락 나오고 해당 시간 지나면 다시 전투 준비로 가든가
    }

}
