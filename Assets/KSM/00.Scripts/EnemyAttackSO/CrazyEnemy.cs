using TMPro;
using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "CrazyEnemy", menuName = "AttackSO/CrazyEnemy", order = 0)]

    public class CrazyEnemy : AbstractAttackSO
    {
        [SerializeField]private TextMeshPro crazyGage;
        public override void Attacking(AbstractUnit unit)
        {
            
        }
    }
}