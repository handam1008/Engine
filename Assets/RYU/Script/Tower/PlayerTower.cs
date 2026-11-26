namespace RYU.Script.Tower
{
    public class PlayerTower : AbstractUnit
    {
        protected override void Awake()
        {
            base.Awake();
            HpCompo.OnDeath += TowerDead;
        }

        private void TowerDead()
        {
            GameLoopManager.Instance.ChangeState(CurrentGameType.Defeat);
        }

        protected override void Update()
        {
        }

        private void OnDisable()
        {
            HpCompo.Init(UnitSo.maxHp, UnitSo);
            StateMachine.ChangeState(State.Chase);
        }
    }
}
