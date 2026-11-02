using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using RYU.Script.Stage;
using Unity.VisualScripting;
using UnityEngine;

namespace RYU.Script.Tower
{
    public class Tower : AbstractUnit
    {
        [SerializeField] private StageSO stage;
        private int _count= 0;
        private bool _canCreate = true;
        private HealthSystem _healthSystem;
        private IEnumerator _enumerator;

        protected override void Awake()
        {
            base.Awake();
            _healthSystem = GetComponent<HealthSystem>();
        }

        public void Start()
        {
            base.Update();
            foreach (var data in stage.stageDates)
            {
                data.isSpawn = false;
            }
            _healthSystem.OnDamaged += HandleDamage;
        }

        private void HandleDamage()
        {
            foreach (var data in stage.stageDates)
            {
                if (data.percent <= 0 || Mathf.Approximately(data.percent, 100f) || data.isSpawn) continue;
                if (_healthSystem.GetPercentage() > data.percent) continue;

                data.isSpawn = true;
                Instantiate(data.enemy.prefab, transform.position, Quaternion.identity);
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

                if (!Mathf.Approximately(data.percent, 100f)) 
                    continue;

                StartCoroutine(CreateEnemy(data.enemy.prefab, data.nextWaitTime));
                break;
            }
            
        }


        private IEnumerator CreateEnemy(GameObject gg, float waitTime)
        {
            _canCreate = false; 
            Instantiate(gg, transform.position, Quaternion.identity);
            Debug.Log(gg.name + "소환!");
            _count++;
            yield return new WaitForSeconds(waitTime);
            _canCreate = true;
        }
        
    }
}