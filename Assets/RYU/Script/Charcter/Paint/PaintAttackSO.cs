using UnityEngine;

namespace RYU.Script.Charcter.Paint
{
    [CreateAssetMenu(fileName = "PaintAttackSO", menuName = "SO/Attack/PaintAttackSO", order = 0)]
    public class PaintAttackSO : AbstractAttackSO
    {
        public Vector2 boxSize;
        private readonly int[] _attackMultiple = new int[] { 1, 2, 3 };
        [SerializeField] private GameObject[] arcPrefabs;
        [SerializeField] private GameObject[] particle;


        public override void Attacking(AbstractUnit unit)
        {
            var paint = unit as PaintCharacter;
            int combo = paint != null ? paint.AttackCount : 0;

            CreateEffect(unit, combo);
            Vector3 particlePos = unit.target != null
                ? unit.target.position
                : unit.transform.position + new Vector3(1, 0, 0);
            Instantiate(particle[combo], particlePos, Quaternion.identity);

           var col = Physics2D.OverlapBoxAll(unit.transform.position + new Vector3(1,0,0) , boxSize, 0f, unit.UnitSo.whatIsDetect);
           if (col.Length > 0)
           {
               foreach (Collider2D coll in col)
               {
                   if (coll.TryGetComponent(out HealthSystem healthSystem))
                   {
                      healthSystem.Damage(unit.UnitSo.attackDmg * _attackMultiple[combo]);
                      Debug.Log(unit.UnitSo.attackDmg * _attackMultiple[combo]);
                   }
               }
           }

           if (paint != null) paint.NextCombo();
        }

        private void CreateEffect(AbstractUnit unit, int combo)
        {
            Vector3 center = unit.transform.position + new Vector3(unit.Dir, 0, 0);
            var effect = Instantiate(arcPrefabs[combo], center, Quaternion.identity);
            Destroy(effect, 0.3f);
        }
    }
}