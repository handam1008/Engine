using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "ShootingEnemyAttack", menuName = "AttackSO/ShootingEnemyAttack", order = 0)]
    public class EnemyShooting : AbstractAttackSO
    {
        public GameObject bulletPrefab;
        [SerializeField]private float speed;
        [SerializeField] private float destroyTime;
        public override void Attacking(AbstractUnit unit)
        {
            GameObject go =  Instantiate(bulletPrefab,unit.transform.position,unit.transform.rotation);
            if (go.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.linearVelocityX = speed *unit.Dir;
            }
        }
    }
}