using RYU.Script.Stage;
using UnityEngine;
using System.Collections;

namespace RYU.Script.Tower
{
    public class Tower : AbstractUnit
    {
        [SerializeField] private StageSO stage;
        [SerializeField] private TestGameEndTrigger trigger;
        private int _count= 0;
        private bool _canCreate = true;

        protected override void Awake()
        {
            base.Awake();
            HpCompo.OnDeath += TowerDead;
        }

        private void OnEnable()
        {
            HpCompo.Init(UnitSo.maxHp, UnitSo);
            _canCreate = true;
            _count = 0;
            foreach (var data in stage.stageDates)
            {
                data.isSpawn = false;
            }

        }
                
        

        private void TowerDead()
        {
            GameLoopManager.Instance.ChangeState(CurrentGameType.Victory);
            trigger.End(stage.getGold);
        }

        public void Start()
        {
            base.Update();
            foreach (var data in stage.stageDates)
            {
                data.isSpawn = false;
            }
            HpCompo.OnDamaged += HandleDamage;
        }

        private void HandleDamage()
        {
            foreach (var data in stage.stageDates)
            {
                if (data.percent <= 0 || Mathf.Approximately(data.percent, 100f) || data.isSpawn) continue;
                if (HpCompo.GetPercentage() > data.percent) continue;

                data.isSpawn = true;
                StartCoroutine(SpawnGroup(data));
            }
        }


        protected override void Update()
        {
            if (!_canCreate) return;
            for (int i = 0; i < stage.stageDates.Count; i++)
            {
                if (_count >= stage.stageDates.Count)
                    _count = 0;

                StageData data = stage.stageDates[_count++];

                if (!Mathf.Approximately(data.percent, 100f) || data.isSpawn)
                    continue;

                StartCoroutine(CreateEnemy(data));
                break;
            }

        }


        private IEnumerator CreateEnemy(StageData data)
        {
            _canCreate = false;
            data.isSpawn = data.once;
            yield return SpawnGroup(data);
            yield return new WaitForSeconds(data.nextWaitTime);
            _canCreate = true;
        }

        private IEnumerator SpawnGroup(StageData data)
        {
            for (int i = 0; i < Mathf.Max(1, data.count); i++)
            {
                if (i > 0) yield return new WaitForSeconds(data.countInterval);
                GameObject enemy = Instantiate(data.enemy.prefab, transform.position, Quaternion.identity);
                Debug.Log(enemy.name + "소환!");
                if (data.reviveEnemy != null)
                    enemy.GetComponent<HealthSystem>().OnReceive += () => Instantiate(data.reviveEnemy.prefab, transform.position, Quaternion.identity);
            }
        }

        private void OnDisable()
        {
            HpCompo.Init(UnitSo.maxHp, UnitSo);
        }

    }
}
