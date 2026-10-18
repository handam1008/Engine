using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public AbstractUnitState CurrentState { get; private set; }
    public Dictionary<State, AbstractUnitState> Dict = new();
        
    public void Add(State key, AbstractUnitState value)
    {
        Dict.Add(key, value);
    }

    public void ChangeState(State state)
    {
        if(CurrentState != null)
            CurrentState.Exit();
        CurrentState = Dict[state];
        if(CurrentState != null)
            CurrentState.Enter();
    }

    public void Init( State state)
    {
        CurrentState = Dict[state];
        CurrentState.Enter();
    }

}

  

public enum State
{
    Chase,
    Attack,
    Hit,
    Dead,
    WaitAttack,
}