using UnityEngine;

namespace RYU.Script.Charcter.Paint
{
    public class PaintCharacter : AbstractUnit
    {
        [SerializeField] private AnimatorOverrideController[] attackControllers;

        public int AttackCount { get; private set; }

        public void NextCombo() => AttackCount = (AttackCount + 1) % 3;

        private void OnDrawGizmos()
        {
            Vector3 center = transform.position + new Vector3(1, 0, 0);

            if (Attack is PaintAttackSO paintAttack)
            {
                Vector3 boxsize = new Vector3(paintAttack.boxSize.x, paintAttack.boxSize.y, 0);
            
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(center, boxsize);
            }
        }
        
        public override void PrepareAttack()
        {
            AnimCompo.runtimeAnimatorController =
                attackControllers[AttackCount];
        }
      

       
    }
}