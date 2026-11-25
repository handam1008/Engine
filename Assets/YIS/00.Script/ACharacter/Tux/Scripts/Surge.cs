using System;
using System.Collections;
using UnityEngine;

public class Surge : MonoBehaviour
{
    [SerializeField] private float speed = 20;
    public event Action<Surge> GetReadyToFire;

    private int _attackDamage;
    private int _random;
    private Rigidbody2D _rigidbody2D;


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

    public void AfterShoot()
    {
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

}
