using UnityEngine;

[CreateAssetMenu(fileName = "Boom", menuName = "SO/Boom")]
public class Boom : AbstractAttackSO
{
    private Vector2 boomPos;
    private float boomRange;
    public override void Attacking(AbstractUnit unit)
    {
        int damage = unit.UnitSo.maxHp + 100;
        
        Collider2D[] target = DetectedBoomRange(unit);
        foreach (Collider2D c in target)
        {
            if (c.TryGetComponent(out AbstractUnit targetUnit))
            {
                targetUnit.HpCompo.Damage(unit.UnitSo.attackDmg);   
            }
        }
        
        unit.HpCompo.Damage(damage);
    }

    private Collider2D[] DetectedBoomRange(AbstractUnit unit)
    {
        Vector2 boomPos = unit.transform.position;
        Collider2D[] targets = Physics2D.OverlapCircleAll(boomPos, boomRange, unit.UnitSo.whatIsDetect);
        return targets;
    }
}
