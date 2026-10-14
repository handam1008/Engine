using System.Collections.Generic;

public enum CurrentGameType {StartBettle, Bettling, EndBettle }
public class GameStateMachine
{
    public GameState CurState;
    Dictionary<CurrentGameType, GameState> gameStateDic = new Dictionary<CurrentGameType, GameState>();
    
    public void Initalize(CurrentGameType type)
    {
        CurState = gameStateDic[type];
        CurState.Enter();
    }

    public void AddState(CurrentGameType type, GameState state)
    {
        gameStateDic.Add(type, state);
    }

    public void ChangeState(CurrentGameType type)
    {
        GameState state = gameStateDic[type];
        CurState?.Exit();
        
        CurState = state;
        CurState?.Enter();
    }
}
