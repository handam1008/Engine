using UnityEngine;

public class BettlePrepare : GameState
{
    public BettlePrepare(GameStateMachine machine, GameLoop loop) : base(machine, loop)
    {
    }

    public override void Enter()
    {
        base.Enter();
        GameLoopManager.Instance.KillingField.SetActive(true);
        Debug.Log("진격준비");
    }
    public override void StateUpdate()
    {
        base.StateUpdate();
        // 준비되면 전투 시작으로 넘어감
        
    }
}
