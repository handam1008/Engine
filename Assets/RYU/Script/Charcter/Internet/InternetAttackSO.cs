using UnityEngine;
using RYU.Script.Pool;

namespace RYU.Script.Charcter.Internet
{
    [CreateAssetMenu(fileName = "InternetAttackSO", menuName = "SO/Attack/Internet", order = 0)]
    public class InternetAttackSO : AbstractAttackSO
    {
        [SerializeField] private InternetBullet bullet;
        public override void Attacking(AbstractUnit unit)
        {
            if (bullet == null || unit.target == null) return;

            var poolable = PoolManager.Instance.Pop(bullet.ItemName);

            var instance = (InternetBullet)poolable;
            instance.Init(unit.UnitSo.attackDmg, unit.UnitSo.whatIsDetect);
            instance.Shoot(unit.target);
            //instance.Move(unit.target);
        }
    }
}
