using UnityEngine;

public class BudybudyCharacter : AbstractUnit
{
    public Transform _supportUnit;
    private LineRenderer _lr;
    [SerializeField] private LayerMask _whatIsUnit;

    
    private void Start()
    {
        UnitSo.attackCoolTime = 0.5f;

        _lr = GetComponent<LineRenderer>();
        _lr.useWorldSpace = true;

        if (FindNearTarget() == null) return;

        if (_supportUnit == null)
        {
            _supportUnit = FindNearTarget().transform;
            Debug.Log($"{_supportUnit.name}ÀÌ(°¡) Áö¿øµÊ");
        }

    }

    private void FixedUpdate()
    {
        if (FindNearTarget() == null)
        {
            _lr.positionCount = 0;
            return;
        }

        if (_supportUnit == null)
        {
            _supportUnit = FindNearTarget().transform;
  
            Debug.Log($"{_supportUnit.name}ÀÌ(°¡) Áö¿øµÊ");

        }
            _lr.positionCount = 2;
            _lr.SetPosition(0, transform.position);
            _lr.SetPosition(1, _supportUnit.transform.position);


    }

    public Collider2D FindNearTarget()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, 10, _whatIsUnit);
        Collider2D realTarget = null;
        float nearestDistance = float.MaxValue;


        foreach (Collider2D target in targets)
        {
            float dis = (target.transform.position - transform.position).magnitude;
            if (dis < nearestDistance && dis != 0)
            {
                nearestDistance = dis;
                realTarget = target;
            }
        }
        return realTarget;
    }
}
