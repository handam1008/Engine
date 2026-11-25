using DG.Tweening;
using UnityEngine;

namespace KSM._00.Scripts.EnemyAttack
{
    public class Wave : MonoBehaviour
    {
        [SerializeField] private int dmg;
        [SerializeField] private int pulseCount = 4;
        [SerializeField] private float pulseScale = 0.1f;
        [SerializeField] private float pulseTime = 0.3f;

        public void WavePower(LayerMask mask)
        {
            float baseY = transform.localScale.y;      
            Sequence seq = DOTween.Sequence().SetLink(gameObject); 

            for (int i = 0; i < pulseCount; i++)
            {
                float targetY = i % 2 == 0 ? baseY + pulseScale : baseY - pulseScale;
                seq.Append(transform.DOScaleY(targetY, pulseTime));
                seq.AppendCallback(() => HitAll(mask));
            }

            seq.OnComplete(() => Destroy(gameObject));        
        }

        private void HitAll(LayerMask mask)
        {
            foreach (var c in Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0, mask))
            {
                if (c.TryGetComponent(out AbstractUnit u)) u.HpCompo.Damage(dmg, u);
            }
        }
    }
}