
public abstract class GameState
{
    public GameStateMachine _machine;
    public GameLoop _gameLoop;

    protected GameState(GameStateMachine machine, GameLoop loop)
    {
        _machine = machine;
        _gameLoop = loop;
    }

    public virtual void Enter()
    {

    }

    public virtual void StateUpdate()
    {

    }

    public virtual void Exit()
    {

    }
}
