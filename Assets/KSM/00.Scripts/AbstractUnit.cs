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
    

    protected virtual void Awake()
    {
        AnimCompo = GetComponentInChildren<Animator>();
        AnimCompo.runtimeAnimatorController =  UnitSo.animatorController;
        UnitAnimCompo = GetComponentInChildren<UnitAnimation>();
        HpCompo = GetComponent<HealthSystem>();
        MoveCompo = GetComponent<MoveSystem>();
        RenderCompo = GetComponentInChildren<SpriteRenderer>();
        InitInfo();
    }

    protected virtual void InitInfo()
    {
        HpCompo.InitHp(UnitSo.maxHp);
        MoveCompo.InitMove(UnitSo.moveSpeed,Dir);
    }

 

    public Collider2D DetecPlayer()
    {
        return Physics2D.OverlapCircle(transform.position, UnitSo.attackRange,UnitSo.whatIsDetect);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, UnitSo.attackRange);
    }
}
