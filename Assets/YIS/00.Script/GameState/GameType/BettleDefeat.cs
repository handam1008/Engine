using UnityEngine;
using UnityEngine.InputSystem;

public class BettleDefeat : GameState
{

    public BettleDefeat(GameStateMachine machine, GameLoop loop) : base(machine, loop)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("패배....");
        GameLoopManager.Instance.KillingField.SetActive(true);
        GameLoopManager.Instance.BlueScreen.SetActive(true);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (!keyPressed && !mousePressed) return;   

        GameLoopManager.Instance.EndTrigger.Lose();
        _machine.ChangeState(CurrentGameType.Prepare);
    }
    public override void Exit()
    {
        base.Exit();
        Time.timeScale = 1f;
        GameLoopManager.Instance.BlueScreen.SetActive(false);
    }
}
