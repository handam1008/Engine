using KSM._00.Scripts;
using RYU.Script.Charcter;
using System;
using UnityEngine;
public enum UnitType
{
    Enemy,
    Charcter,
}
public abstract class AbstractUnit : MonoBehaviour
{
    [field:SerializeField]public UnitSO UnitSo { get; private set; }
    [field:SerializeField]public AbstractAttackSO Attack { get; private set; }
    public Animator AnimCompo { get; private set; }
    public UnitAnimation UnitAnimCompo { get; private set; }
    public HealthSystem HpCompo { get; private set; }
    public MoveSystem MoveCompo { get; private set; }
    public SpriteRenderer RenderCompo { get; private set; }
    public float Dir => UnitSo.unitType == UnitType.Enemy ? -1f : 1f;
    public Transform target;
    public float lastAttackTime;
    public StateMachine StateMachine { get; private set; }
    

    protected virtual void Awake()
    {
        AnimCompo = GetComponentInChildren<Animator>();
        AnimCompo.runtimeAnimatorController =  UnitSo.animatorController;
        UnitAnimCompo = GetComponentInChildren<UnitAnimation>();
        HpCompo = GetComponent<HealthSystem>();
        MoveCompo = GetComponent<MoveSystem>();
        RenderCompo = GetComponentInChildren<SpriteRenderer>();
        StateMachine = new();
        StateMachine.Add(State.Attack,new AttackState(StateMachine,this,"Attack"));
        StateMachine.Add(State.Chase,new ChaseState(StateMachine,this,"Chase"));
        StateMachine.Add(State.Hit,new HitState(StateMachine,this,"Hit"));
        StateMachine.Add(State.Dead, new DieState(StateMachine,this,"Dead"));
        StateMachine.Add(State.WaitAttack, new WaitAttakState(StateMachine,this,"Wait"));
        StateMachine.Init(State.Chase);
        InitInfo();
    }

 
    private void Start()
    {
        HpCompo.OnDeath += () => StateMachine.ChangeState(State.Dead);
        HpCompo.OnDamged +=  () => StateMachine.ChangeState(State.Hit);
    }

    protected virtual void Update()
    {
        StateMachine.CurrentState.UpdateState();
    }

    protected virtual void InitInfo()
    {
        HpCompo.InitHp(UnitSo.maxHp);
        MoveCompo.InitMove(UnitSo.moveSpeed,Dir);
    }

    public virtual void PrepareAttack()
    {
        
    }

 

    public Collider2D[] DetecPlayer()
    {
        return Physics2D.OverlapCircleAll(transform.position, UnitSo.attackRange,UnitSo.whatIsDetect);
    }

    public Collider2D NearTarget()
    {
        Collider2D[] targets = DetecPlayer();
        Collider2D nearTarget = null;
        float near = 500f;
        foreach (Collider2D c in targets)
        {
            float t = Mathf.Abs(c.transform.position.x - transform.position.x);
            if (t < near)
            {
                near = t;    
                nearTarget = c;
            }
            
        }
        return nearTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if(UnitSo == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, UnitSo.attackRange);
    }
}
