using System;
using System.Collections;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace KSM._00.Scripts.Enemy
{
    public class FaceTimeEnemy :AbstractUnit
    {
        public int count;
        public FictureColor photo;
        private Vector2 range;
        private FictureColor v;

        protected override void Awake()
        {
            base.Awake();
            UnitAnimCompo.OnAttacked += HandleAttacked;
            UnitAnimCompo.OnAnimEnd += HandleAnimEnd;
        }
       

        private void HandleAnimEnd()
        {
            count = 0;
            Destroy(v.gameObject);
        }

        private void OnDestroy()
        {
            UnitAnimCompo.OnAttacked -= HandleAttacked;
        }

        public void DetectTarget(Vector2 pos,Vector2 boxSize,  int dmg)
        {
            Collider2D[] targets = Physics2D.OverlapBoxAll(pos, boxSize, 0, UnitSo.whatIsDetect);
            foreach (Collider2D t in targets)
            {
                if (t.TryGetComponent(out AbstractUnit health))
                {
                    health.HpCompo.Damage(dmg, health);
                }
            }
        }

       
        

        private void HandleAttacked()
        {
            if (target == null) return;
            count++;
            if (count == 1)
            {
                v = Instantiate(photo, new Vector2(target.transform.position.x, transform.position.y+1f), Quaternion.identity);
                range = new Vector2(photo.transform.localScale.x*3f, target.transform.localScale.y*3f);
               v.ChangeColor();
                    DetectTarget(target.transform.position, range,UnitSo.attackDmg+UnitSo.attackDmg/2);
                
            }
            else
            {
                DetectTarget(target.transform.position, range,UnitSo.attackDmg);
                v.ChangeColor();
                
            }
            
        }
    }
}