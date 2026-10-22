using System;
using UnityEngine;

namespace RYU.Script.Charcter.Paint
{
    public class PaintCharacter : AbstractUnit
    {
        private PaintAttackSO _paintAttack;
        [SerializeField] private AnimatorOverrideController[] attackControllers;
      
        private void OnDrawGizmos()
        {
            _paintAttack = Attack as PaintAttackSO;
            Vector3 center = transform.position + new Vector3(1, 0, 0);
            
            Vector3 boxsize = new Vector3(_paintAttack.boxSize.x, _paintAttack.boxSize.y, 0);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, boxsize);
        }
        
        public override void PrepareAttack()
        {
            AnimCompo.runtimeAnimatorController =
                attackControllers[_paintAttack.AttackCount];
        }
      

       
    }
}