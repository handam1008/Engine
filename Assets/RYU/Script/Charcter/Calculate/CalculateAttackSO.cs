using UnityEngine;

namespace RYU.Script.Charcter.Calculate
{
    [CreateAssetMenu(fileName = "CalculateSO", menuName = "SO/Character/Calculate", order = 0)]
    public class CalculateAttackSO : AbstractAttackSO
    {
        [SerializeField] private CalculateBullet calculateBullet;
        public override void Attacking(AbstractUnit unit)
        {
            CalculateBullet cal = Instantiate(calculateBullet, unit.transform.position, Quaternion.identity);
            cal.transform.position = new Vector3(unit.transform.position.x + 0.8f, unit.transform.position.y + 0.8f, unit.transform.position.z);
            cal.Init(unit.UnitSo.attackDmg);
            cal.onReadyEnd += _ => cal.Shoot(unit.target.transform);
        }

       
    }
}