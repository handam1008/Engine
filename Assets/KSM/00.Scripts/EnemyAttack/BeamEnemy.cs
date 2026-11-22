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
                if (unit == null)
                {
                    DieObject(unit);
                    yield break;
                }

                Collider2D near = unit.NearTarget();
                if (near == null)
                {
                    DieObject(unit);
                    yield break;
                }
                unit.target = near.transform;
 
                transform.position = new Vector3(unit.transform.position.x-1.5f, unit.transform.position.y , unit.transform.position.z);
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
 
            DieObject(unit);  
        }
 
        public void DieObject(AbstractUnit unit)
        {
            if (unit != null) unit.UnitAnimCompo.AnimEnd();  
            Destroy(gameObject, 0.05f);
        }
    }
}