using System.Collections;
using UnityEngine;
 
namespace KSM._00.Scripts.EnemyAttack
{
    public class BeamEnemy : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private GameObject beamPos;
        [SerializeField] private float time = 5f;
        [SerializeField] private int dmg;
        [Tooltip("도트딜이여서 0.2f이정도로 넣으셈")]
        [SerializeField] private float attackCool;
        [SerializeField] private float finishAttackRange;
        [SerializeField] private int finishAttackDmg;
        private float currentTime;
 
        public IEnumerator Beam(AbstractUnit unit)
        {
            currentTime = 0;
            lineRenderer.positionCount = 2;
            float hitTime = attackCool;
 
            while (currentTime < time)
            {
                // 시전자가 죽었으면 끝
                if (unit == null)
                {
                    DieObject(unit);
                    yield break;
                }
 
                // 가장 가까운 적으로 타겟 갱신, 없으면 끝
                Collider2D near = unit.NearTarget();
                if (near == null)
                {
                    DieObject(unit);
                    yield break;
                }
                unit.target = near.transform;
 
                transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y + 2f, unit.transform.position.z);
                lineRenderer.SetPosition(0, beamPos.transform.position);
 
                currentTime += Time.deltaTime;
                if (currentTime > hitTime)
                {
                    if (unit.target.TryGetComponent<IDamageAble>(out IDamageAble target)) target.Damage(dmg);
                    hitTime += attackCool;
                }
                lineRenderer.SetPosition(1, unit.target.position);
 
                yield return null;
            }
 
            yield return new WaitForSeconds(0.1f);
 
            // 기다리는 사이에 시전자나 타겟이 죽었으면 끝
            if (unit == null || unit.target == null)
            {
                DieObject(unit);
                yield break;
            }
 
            // 마지막 공격
            Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(unit.target.position, finishAttackRange, unit.UnitSo.whatIsDetect);
            Debug.Log("마지막공격");
            foreach (var collider2D in collider2Ds)
            {
                if (collider2D.TryGetComponent<IDamageAble>(out IDamageAble target)) target.Damage(finishAttackDmg);
            }
 
            DieObject(unit);   // AnimEnd는 이 안에서 한 번만
        }
 
        public void DieObject(AbstractUnit unit)
        {
            if (unit != null) unit.UnitAnimCompo.AnimEnd();   // 시전자가 살아 있을 때만
            Destroy(gameObject, 0.05f);
        }
    }
}