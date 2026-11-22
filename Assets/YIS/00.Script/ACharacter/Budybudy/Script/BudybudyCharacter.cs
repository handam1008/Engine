using UnityEngine;

public class BudybudyCharacter : AbstractUnit
{
    public Transform _supportUnit;
    [SerializeField] private LayerMask _whatIsUnit;

    private void OnEnable()
    {
        if (FindNearTarget() == null) return;

        if (_supportUnit == null)
        {
            _supportUnit = FindNearTarget().transform;
            Debug.Log($"{_supportUnit.name}ÀÌ(°¡) Áö¿øµÊ");
        }

    }

    private void FixedUpdate()
    {
        if (FindNearTarget() == null) return;

        if (_supportUnit == null)
        {
            _supportUnit = FindNearTarget().transform;
            Debug.Log($"{_supportUnit.name}ÀÌ(°¡) Áö¿øµÊ");
        }

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
