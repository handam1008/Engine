using Unity.VisualScripting;
using UnityEngine;

namespace CHO.SO.AttackEnemySO.AttackSOScript
{
    public class ThrowBallAttackSO : AbstractAttackSO ,IPoolable
    {
        [SerializeField] private GameObject bigBall;
        public override void Attacking(AbstractUnit unit)
        {
            CreateThrowBall();
        }

        private void CreateThrowBall()
        {
            
        }

        [field: SerializeField] public string ItemName { get; private set; } = "BigBall";

    

        public void New()
        {
            throw new System.NotImplementedException();
        }

        public void Free()
        {
            throw new System.NotImplementedException();
        }
    }
}
