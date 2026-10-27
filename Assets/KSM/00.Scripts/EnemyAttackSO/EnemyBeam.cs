using System;
using KSM._00.Scripts.EnemyAttack;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitAttackBeam", menuName = "AttackSO/OneUnitAttackSO", order = 0)]

public class EnemyBeam : AbstractAttackSO
{
    public BeamEnemy skillPrefab;
    public override void Attacking(AbstractUnit unit)
    {
        BeamEnemy skill = Instantiate(skillPrefab,
            new Vector3(unit.transform.position.x, unit.transform.position.y + 2f, unit.transform.position.z),
            Quaternion.identity);
        skill.StartCoroutine(skill.Beam(unit));
    }
    
}
