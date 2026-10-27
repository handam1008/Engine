using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using RYU.Script.Stage;
using UnityEngine;

namespace RYU.Script.Tower
{
    public class Tower : MonoBehaviour
    {
        public StageSO stage;
        public int count= 0;
        private bool _canCreate = true;
        private HealthSystem _healthSystem;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
        }

        private void Start()
        {
            StartCoroutine(CreateEnemy(stage.stageDates[count].enemy.prefab, stage.stageDates[count].nextWaitTime));
        }

        private void Update()
        {
            if (!_canCreate) return;
            if (count >= stage.stageDates.Count)
            {
                count = 0;
                StartCoroutine(CreateEnemy(stage.stageDates[count].enemy.prefab, stage.stageDates[count].nextWaitTime));
            }
            
            
        }


        public IEnumerator CreateEnemy(GameObject gg, float waitTime)
        {
            _canCreate = false;
            Instantiate(gg, transform.position, Quaternion.identity);
            count++;
            yield return new WaitForSeconds(waitTime);
            _canCreate = true;
        }
    }
}