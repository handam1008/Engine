using UnityEngine;

namespace CHO.SO.AttackEnemySO.AttackSOScript
{
    public class ThrowBallAttackSO : AbstractAttackSO 
    {
        [SerializeField] private GameObject bigBall;
        public override void Attacking(AbstractUnit unit)
        {
            CreateThrowBall();
        }

        private void CreateThrowBall()
        {
            
        }

    
    }
}
