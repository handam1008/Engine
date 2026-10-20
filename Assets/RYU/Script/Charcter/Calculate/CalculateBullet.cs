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
        public event Action<CalculateBullet> onReadyEnd;
        
        
        
        private TextMeshPro _text;
        private int _attackDamage;
        private int _random;
        private Tween _tween;
        private Rigidbody2D _rigidbody2D;
        
        
        private float _timer;
        private int _count;

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
            _timer += Time.deltaTime;
                
            if (_timer > scaleUpTimer)
            {
                _text.fontSize += 5;
                _count++;
                _timer = 0;
            }
            
            if(_count >= count)
                onReadyEnd?.Invoke(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Character")) return;
            if (other.TryGetComponent(out HealthSystem healthSystem))
            {
                healthSystem.Damage(_random);
                Destroy(gameObject);
            }
        }

        public void Shoot(Transform target)
        {
            Vector2 launchDir = (target.position - transform.position).normalized;
            
            _rigidbody2D.linearVelocity = launchDir * speed;
        }
        
        
    }
}