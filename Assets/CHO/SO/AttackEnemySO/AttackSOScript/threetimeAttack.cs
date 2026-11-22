using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "3timeAttack", menuName = "SO/3timeAttack")]
public class threetimeAttack : AbstractAttackSO
{
    private int hitCount = 3;
    private float hitInterval = 0.3f;
    public override void Attacking(AbstractUnit unit)
    {
        unit.StartCoroutine(ComboRoutine(unit)); // SO는 코루틴을 못 돌리니까 유닛에게 맡김
    }
    
    private IEnumerator ComboRoutine(AbstractUnit unit)
    {
        for (int i = 0; i < hitCount; i++)
        {
            if (unit.HpCompo.Dead) yield break; // 콤보 도중 죽으면 중단

            Attack(unit);   

            if (i < hitCount - 1)
                yield return new WaitForSeconds(hitInterval);
        }
    }
    
    public void Attack(AbstractUnit unit)
    {
        Collider2D target = unit.NearTarget();
        if(target != null && target.TryGetComponent(out HealthSystem healthSystem))
            healthSystem.Damage(unit.UnitSo.attackDmg);
            
    }
}
