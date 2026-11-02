using UnityEngine;

namespace RYU.Script.Charcter.WasteBasket
{
    [CreateAssetMenu(fileName = "TrashAttackSO", menuName = "SO/Attack/TrashAttackSO", order = 0)]
    public class TrashAttackSO : AbstractAttackSO
    {
        public Vector2 boxSize;
        public override void Attacking(AbstractUnit unit)
        {
            var col = Physics2D.OverlapBoxAll(unit.transform.position + new Vector3(1,0,0) , boxSize, 0f, unit.UnitSo.whatIsDetect);
            if (col.Length > 0)
            {
                foreach (Collider2D coll in col)
                {
                    if (coll.TryGetComponent(out HealthSystem healthSystem))
                    {
                        healthSystem.Damage(unit.UnitSo.attackDmg);
                    }
                }
            }
        }
    }
}