using System;
using UnityEngine;

namespace RYU.Script.PoolManager
{
    public class PoolableParticle : MonoBehaviour, IPoolable
    {
        private ParticleSystem particles;
        private Transform _transform;
        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
        }


        [field:SerializeField] public string ItemName { get; private set; }
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void OnParticleSystemStopped()
        {
            PoolManager.Instance.Push(this);
        }

        public void ResetItem()
        {
            particles.Clear();
        }

        public void SetTransform(Transform trm)
        {
            _transform.position = trm.position;
        }
    }
}