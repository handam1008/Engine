using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour ,IDamageAble
{
    [SerializeField] private int _health;
    private int _maxHealth;
    public event Action OnDamged;
    public event Action OnDeath;
    public bool Dead => _health <= 0;

    public void InitHp(int health)
    {
        _maxHealth = _health;
        _health = _maxHealth;
    }
    public void Damage(int dmg)
    {
        if(Dead) return;
        _health -= dmg;
        OnDamged?.Invoke();
        if(Dead)
            OnDeath?.Invoke();
    }
}