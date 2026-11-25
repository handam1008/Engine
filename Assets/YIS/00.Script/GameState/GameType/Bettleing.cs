using DevLib.ServiceLocator;
using RYU.Script.Service.PanelService;
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
        GameLoopManager.Instance.KillingField.SetActive(false);
        IPanelService panel = ServiceLocator.Get<IPanelService>();

        if (!panel.WindowPanel.activeSelf)
        {
            panel.SetWindowPanelActive();
        }
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
