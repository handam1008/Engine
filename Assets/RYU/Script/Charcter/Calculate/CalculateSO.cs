using UnityEngine;

namespace RYU.Script.Charcter.Calculate
{
    [CreateAssetMenu(fileName = "CalculateSO", menuName = "SO/Charcter/Calculate", order = 0)]
    public class CalculateSO : AbstractAttackSO
    {
        private int _random1;
        private int _random2;
        public override void Attacking(AbstractUnit unit)
        {
            _random1 = Random.Range(0, unit.UnitSo.attackDmg);
            _random2 = Random.Range(0, unit.UnitSo.attackDmg);
        }
    }
}