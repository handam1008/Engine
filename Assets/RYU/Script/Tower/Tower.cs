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

        protected override void Awake()
        {
            base.Awake();
            _healthSystem = GetComponent<HealthSystem>();
        }

        public void Start()
        {
            _healthSystem.OnDamged += HandleDamage;
        }

        private void HandleDamage()
        {
            foreach (var gg in stage.stageDates)
            {
                if (gg.isSpawn) return;
                if (_healthSystem.GetPercentage() > gg.percent)
                {
                }
            }
        }


        protected override void Update()
        {
            base.Update();
            if (!_canCreate) return;
            if (_count >= stage.stageDates.Count)
            {
                _count = 0;
            }
            StartCoroutine(CreateEnemy(stage.stageDates[_count].enemy.prefab, stage.stageDates[_count].nextWaitTime));
            
        }


        public IEnumerator CreateEnemy(GameObject gg, float waitTime)
        {
            _canCreate = false; 
            Instantiate(gg, transform.position, Quaternion.identity);
            _count++;
            yield return new WaitForSeconds(waitTime);
            _canCreate = true;
        }
        
    }
}