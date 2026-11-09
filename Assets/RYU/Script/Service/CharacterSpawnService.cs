using System.Collections.Generic;
using DevLib.ServiceLocator;
using RYU.Script.Charcter;
using RYU.Script.Serivce;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace RYU.Script.Service
{
    public class CharacterSpawnService : MonoBehaviour, ISpawnService
    {
        [SerializeField] private Transform baseSpawnPoint;
        private Dictionary<CharacterDataSO, float> _nextSpawnTime = new();

        private void Awake()
        {
            ServiceLocator.Register<ISpawnService>(this);
        }
        
        public bool TrySpawn(CharacterDataSO data)
        {
            if (_nextSpawnTime.TryGetValue(data, out float next) && Time.time < next) return false;

            IStorageService storage = ServiceLocator.Get<IStorageService>();
            GameObject character = Instantiate(data.prefab, baseSpawnPoint.position, Quaternion.identity);
            character.GetComponent<HealthSystem>().OnDeath += () => storage.MinusStorage(data.spawnStorage);
            _nextSpawnTime[data] = Time.time + data.spawnCoolTime;
            return true;
        }
    }
}