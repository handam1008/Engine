using System.Collections.Generic;
using KSM._00.Scripts;
using UnityEngine;

[CreateAssetMenu(fileName = "TypeChangeAttack(RawTherapee)", menuName = "SO/TypeChangeAttack(RawTherapee)")]
public class TypeChangeAttack : AbstractAttackSO
{
    [SerializeField] private EnemySO _enemySo;
    private readonly Color[] colors       = { Color.red, Color.blue, Color.green };
    private readonly float[] damageRate   = { 3f,        0.5f,       1f };
    private readonly float[] cooldownRate = { 3f,        0.5f,       1f };

    // 유닛별로 뽑은 타입 번호
    private readonly Dictionary<AbstractUnit, int> unitTypes = new();
    public override void Attacking(AbstractUnit unit)
    {
        int type = TypeChoice(unit);
        
        Collider2D target = unit.NearTarget();
        if (target != null && target.TryGetComponent(out HealthSystem hp))
        {
            float damage = unit.UnitSo.attackDmg * damageRate[type];   // ← UnitSO 공격력 변수 이름으로
            hp.Damage(Mathf.RoundToInt(damage));                      // ← HealthSystem 데미지 함수 이름으로
        }
        
        float cooldown = unit.UnitSo.attackCoolTime;                // ← UnitSO 공격 쿨 변수 이름으로
        unit.lastAttackTime = Time.time + cooldown * (cooldownRate[type] - 1f);
    }

    private int TypeChoice(AbstractUnit unit)
    {
        if (!unitTypes.TryGetValue(unit, out int type))
        {
            type = Random.Range(0, 3);
            unitTypes.Add(unit, type);
            unit.RenderCompo.color = colors[type];
        }
        return type;
    }
}
