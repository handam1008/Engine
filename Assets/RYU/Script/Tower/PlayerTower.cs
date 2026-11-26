using DevLib.ServiceLocator;
using RYU.Script.Service.StorageService;
using UnityEngine;

namespace RYU.Script.Tower
{
    public class PlayerTower : AbstractUnit
    {
        [SerializeField] private float overflowGrace = 5f;
        private IStorageService _storage;
        private float _overflowTime;
        private bool _defeated;

        protected override void Awake()
        {
            base.Awake();
            HpCompo.OnDeath += TowerDead;
        }

        private void TowerDead()
        {
            _defeated = true;
            GameLoopManager.Instance.ChangeState(CurrentGameType.Defeat);
        }

        protected override void Update()
        {
            if (_defeated) return;
            _storage ??= ServiceLocator.Get<IStorageService>();
            float used = 1f - HpCompo.GetPercentage() / 100f + (float)_storage.CurrentStorage / _storage.MaxStorage;
            if (used < 1f)
            {
                _overflowTime = 0f;
                return;
            }

            _overflowTime += Time.deltaTime;
            if (_overflowTime < overflowGrace) return;

            TowerDead();
        }

        private void OnDisable()
        {
            HpCompo.Init(UnitSo.maxHp, UnitSo);
            StateMachine.ChangeState(State.Chase);
            _overflowTime = 0f;
            _defeated = false;
            _storage.MinusStorage(_storage.CurrentStorage);
        }
    }
}
