using DG.Tweening;
using UnityEngine;

namespace KSM._00.Scripts.EnemyAttackSO
{
    [CreateAssetMenu(fileName = "PushEnemy", menuName = "AttackSO/PushEnemy", order = 0)]

    public class PushEnemy : AbstractAttackSO
    {
        public override void Attacking(AbstractUnit unit)
        {
            if (unit.target.TryGetComponent<AbstractUnit>(out AbstractUnit target))
            {

                target.MoveCompo.InitMove(0, 0);

                target.transform.DOMoveX(unit.target.position.x - 2, 0.5f).OnComplete(() =>
                {
                    Debug.Log("뒤로밀침");
                    target.MoveCompo.InitMove(target.UnitSo.moveSpeed, target.Dir);
                });
                
                   
                
            }
        }
    }
}