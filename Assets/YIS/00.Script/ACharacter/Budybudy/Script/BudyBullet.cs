using RYU.Script.Charcter.Calculate;
using System;
using TMPro;
using UnityEngine;

public class BudyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 20;
    public event Action<BudyBullet> GetReadyToFire;

    private int _attackDamage;
    private int _random;
    private Rigidbody2D _rigidbody2D;

    private readonly float _timer;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    

    public void Init(int attackdamage)
    {
        _attackDamage = attackdamage;
        _random = UnityEngine.Random.Range(attackdamage - 3, attackdamage);
    }

    private void Update()
    {
       GetReadyToFire?.Invoke(this);
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