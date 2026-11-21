using DevLib.ServiceLocator;
using RYU.Script.Charcter;
using RYU.Script.Pool;
using RYU.Script.Service.StorageService;
using UnityEngine;
using System.Collections.Generic;

namespace RYU.Script.Service.SpawnService
{
    public class CharacterSpawnService : MonoBehaviour, ISpawnService, IPoolable
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

        public string ItemName => "CharacterSpawnService";
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void ResetItem()
        {
            
        }
    }
}