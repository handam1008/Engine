
using UnityEngine;

public abstract class AbstractUnitState
{
    protected int _hashName;
    protected AbstractUnit _abstractUnit;
    protected StateMachine _stateMachine;
    public AbstractUnitState(StateMachine stateMachine, AbstractUnit abstractUnit, string hashName)
    {
        _stateMachine = stateMachine;
        _abstractUnit = abstractUnit;
        _hashName = Animator.StringToHash(hashName);
    }
    public virtual void Enter()
    {
        _abstractUnit.AnimCompo.SetBool(_hashName,true);
        _abstractUnit.UnitAnimCompo.OnAnimEnd += HandleAnimEnd;
        _abstractUnit.UnitAnimCompo.OnAttacked += HandleAttacked;
    }

    public virtual void HandleAttacked()
    {
        
    }

    public virtual void HandleAnimEnd()
    {
            
    }

    public virtual void Exit()
    {
        _abstractUnit.AnimCompo.SetBool(_hashName,false);
        _abstractUnit.UnitAnimCompo.OnAnimEnd -= HandleAnimEnd;
        _abstractUnit.UnitAnimCompo.OnAttacked -= HandleAttacked;

    }

    public virtual void UpdateState()
    {
            
    }
}