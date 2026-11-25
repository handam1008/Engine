using System;
using Unity.Burst.Intrinsics;
using UnityEngine;

[CreateAssetMenu(fileName = "JuggernautAttack", menuName = "SO/JuggernautAttack")]
public class JuggernautAttack : AbstractAttackSO
{
    public Vector2 boxSize;
    private int killCounter;

    public override void Attacking(AbstractUnit unit)
    {
        var col = Physics2D.OverlapBoxAll(unit.transform.position + new Vector3(-1, 0, 0), boxSize, 0f, unit.UnitSo.whatIsDetect);
        if (col.Length > 0)
        {
            foreach (Collider2D coll in col)
            {
                if (coll.TryGetComponent(out HealthSystem healthSystem))
                {
                    healthSystem.Damage(unit.UnitSo.attackDmg );
                    CountKillUnits(healthSystem, unit);
                    Debug.Log(unit.UnitSo.attackDmg);
                }
            }
        }

    }

    private void CountKillUnits(HealthSystem healthSystem, AbstractUnit unit)
    {
        if (!healthSystem.Dead) return;
        ++killCounter;

        if (killCounter == 10)
        {
            unit.UnitSo.attackCoolTime = 0;
            unit.UnitSo.attackDmg = 30;
            unit.UnitSo.moveSpeed *= 4;
        }



    }
}
