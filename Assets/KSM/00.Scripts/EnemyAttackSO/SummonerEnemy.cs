using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "SummonerEnemy", menuName = "AttackSO/SummonerEnemy", order = 0)]

    public class SummonerEnemy : AbstractAttackSO
    {
        //복사할 타입 쓰기
        
        public override void Attacking(AbstractUnit unit)
        {
            //해당 타입 복사및 해야할 행동하기
           // Instantiate()
        }
    }
}