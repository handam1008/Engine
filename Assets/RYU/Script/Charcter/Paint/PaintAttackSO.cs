using Unity.VisualScripting;
using UnityEngine;

namespace RYU.Script.Charcter.Paint
{
    [CreateAssetMenu(fileName = "PaintAttackSO", menuName = "SO/Attack/PaintAttackSO", order = 0)]
    public class PaintAttackSO : AbstractAttackSO
    {
        public Vector2 boxSize;
        private readonly int[] _attackMultiple = new int[] { 1, 2, 3 };
        [SerializeField] private GameObject[] arcPrefabs;
        [field:SerializeField] public int AttackCount { get; private set; } = 0;
        [SerializeField] private GameObject[] particle;
        
        
        public override void Attacking(AbstractUnit unit)
        {
            CreateEffect(unit); 
            Instantiate(particle[AttackCount], unit.target.transform.position, Quaternion.identity);
            
            AttackCount++;
            if (AttackCount > 2) AttackCount = 0; 
            
            
           var col = Physics2D.OverlapBoxAll(unit.transform.position + new Vector3(1,0,0) , boxSize, 0f, unit.UnitSo.whatIsDetect);
           if (col.Length > 0)
           {
               foreach (Collider2D coll in col)
               {
                   if (coll.TryGetComponent(out HealthSystem healthSystem))
                   {
                      healthSystem.Damage(unit.UnitSo.attackDmg * _attackMultiple[AttackCount]);
                      Debug.Log(unit.UnitSo.attackDmg * _attackMultiple[AttackCount]);
                   }
               }
           }
        }

        private void CreateEffect(AbstractUnit unit)
        {
            Vector3 center = unit.transform.position + new Vector3(unit.Dir, 0, 0);
            var effect = Instantiate(arcPrefabs[AttackCount], center, Quaternion.identity);
            Destroy(effect, 0.3f);
        }
    }
}