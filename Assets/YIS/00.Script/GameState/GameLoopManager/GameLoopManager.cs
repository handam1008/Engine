using UnityEngine;

public class GameLoopManager : GameLoop
{
    public static GameLoopManager Instance { get; private set; }

    GameStateMachine _machine = new GameStateMachine();
    [field:SerializeField] public GameObject BlueScreen { get; private set; }
    [field:SerializeField] public TestGameEndTrigger EndTrigger { get; private set; }
    [field:SerializeField] public GameObject KillingField { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _machine.AddState(CurrentGameType.Prepare, new BettlePrepare(_machine, this));
        _machine.AddState(CurrentGameType.Bettling, new Bettleing(_machine, this));
        _machine.AddState(CurrentGameType.Victory, new BettleWin(_machine, this));
        _machine.AddState(CurrentGameType.Defeat, new BettleDefeat(_machine, this));

        _machine.Initalize(CurrentGameType.Prepare);
    }

    

    public void ChangeState(CurrentGameType type)
    {
        _machine.ChangeState(type);
    }

    private void Update()
    {
        _machine.CurState.StateUpdate();
    }

   
}
