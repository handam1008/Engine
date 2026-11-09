using UnityEngine;

public class BettlePrepare : GameState
{
    bool _ready;
    public BettlePrepare(GameStateMachine machine, GameLoop loop) : base(machine, loop)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("진격준비");
    }
    public override void StateUpdate()
    {
        base.StateUpdate();
        // 준비되면 전투 시작으로 넘어감
        if(_ready)
        {
            _machine.ChangeState(CurrentGameType.StartBettle);
        }
    }
}
