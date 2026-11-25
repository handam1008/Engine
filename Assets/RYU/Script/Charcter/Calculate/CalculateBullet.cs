using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RYU.Script.Charcter.Calculate
{
    public class CalculateBullet : MonoBehaviour
    {
        [SerializeField] private int count = 4;
        [SerializeField] private float scaleUpTimer = 1.5f;
        [SerializeField] private float speed = 20;
        [SerializeField] private float lifeTime = 5f;
        public event Action<CalculateBullet> onReadyEnd;
        
        
        
        private TextMeshPro _text;
        private int _attackDamage;
        private int _random;
        private Tween _tween;
        private Rigidbody2D _rigidbody2D;
        
        
        private float _timer;
        private int _count;
        private bool _shot;
        private bool _hit;

        private void Awake()
        {
            _text = GetComponent<TextMeshPro>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void Init(int attackdamage)
        {
            _attackDamage = attackdamage;
            _random = Random.Range(attackdamage - 10, attackdamage);
            _text.SetText(_random.ToString());

        }

        private void Update()
        {
            if (_shot) return;
            _timer += Time.deltaTime;
                
            if (_timer > scaleUpTimer)
            {
                _text.fontSize += 5;
                _count++;
                _timer = 0;
            }
            
            if (_count >= count)
            {
                _shot = true;
                onReadyEnd?.Invoke(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Character")) return;
            if (_hit) return;
            if (other.TryGetComponent(out AbstractUnit healthSystem) && !healthSystem.HpCompo.Dead)
            {
                _hit = true;
                healthSystem.HpCompo.Damage(_random,healthSystem);
                Destroy(gameObject);
            }
        }

        public void Shoot(Transform target, float fallbackDir)
        {
            Vector2 launchDir = target != null
                ? ((Vector2)(target.position - transform.position)).normalized
                : Vector2.right * fallbackDir;

            _rigidbody2D.linearVelocity = launchDir * speed;
            Destroy(gameObject, lifeTime);
        }
        
        
    }
}