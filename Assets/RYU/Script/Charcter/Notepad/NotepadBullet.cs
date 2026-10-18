using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RYU.Script.Charcter.Notepad
{
    public class NotepadBullet : MonoBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        private int _attackDamage;
        private TextMeshPro _text;
        public List<string> texts = new List<string>();
        [field: SerializeField] public float TextSpeed { get; private set; } = 5f;
        [SerializeField] private float backDistance = 0.4f;
        [SerializeField] private float backDuration = 0.15f;
        [SerializeField] private ParticleSystem hitParticles;
        private Tween _backTween;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _text = GetComponent<TextMeshPro>();
        }

        public void Init(int damage)
        {
            _attackDamage = damage;
        }

        private void Start()
        {
            int random = Random.Range(0, texts.Count);
            _text.text = texts[random];
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Charcter")) return;
            if (other.TryGetComponent(out HealthSystem healthSystem))
            {
                healthSystem.Damage(_attackDamage);
                Destroy(gameObject);
                Instantiate(hitParticles, other.transform.position, transform.rotation);
            }
        }

        public void Shoot(Transform target, float speed, float height, float launchDelay)
        {
           _rigidbody2D.linearVelocity = Vector2.zero;
           
           Vector3 dir = (target.position - transform.position).normalized;
           Vector3 backPosition = transform.position - dir * backDistance + Vector3.up * height;

           _backTween = Tween.Position(
               transform,
               endValue: backPosition,
               duration: backDuration,
               ease: Ease.InQuad,
               endDelay: launchDelay

           ).OnComplete(() => Set(target, speed));
        }

        public void Set(Transform target,float speed)
        {
            _rigidbody2D.position = transform.position;
            _rigidbody2D.simulated = true;

            Vector2 launchDir = ((Vector2)target.position - _rigidbody2D.position).normalized;
            
            _rigidbody2D.linearVelocity = launchDir * speed;
        }

    }
}