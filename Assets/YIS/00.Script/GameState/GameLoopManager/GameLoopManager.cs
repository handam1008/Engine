using System;
using System.Threading;
using UnityEngine;

public class GameLoopManager : GameLoop
{
    GameStateMachine _machine = new GameStateMachine();
    [SerializeField] private GameObject BlueScreen;
    public bool isdefeated = false;

    private void Awake()
    {
        _machine.AddState(CurrentGameType.Prepare, new BettlePrepare(_machine, this));
        _machine.AddState(CurrentGameType.StartBettle, new BettleStart(_machine, this));
        _machine.AddState(CurrentGameType.Bettling, new Bettleing(_machine, this));
        _machine.AddState(CurrentGameType.Victory, new BettleWin(_machine, this));
        _machine.AddState(CurrentGameType.Defeat, new BettleDefeat(_machine, this));

        _machine.Initalize(CurrentGameType.Prepare);
    }

    

    private void Update()
    {
        _machine.CurState.StateUpdate();
        BlueScreen?.SetActive(isdefeated);
        if(isdefeated)
        {
            _machine.ChangeState(CurrentGameType.Defeat);
            Thread.Sleep(1000);
        }
    }

   
}
