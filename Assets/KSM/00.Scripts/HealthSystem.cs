using System;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour ,IDamageAble
{
    [field:SerializeField] public int Health { get; private set; }
    [SerializeField] private int _maxHealth;
    public event Action OnKnockbacked;
    public event Action OnDamaged;
    public event Action OnDeath;
    public bool Dead => Health <= 0;
    private readonly List<float> _knockbacks = new();
    private int _count;
    public event Action OnReceive;
    public event Func<int, bool> CanTakedamage;


    public void Init(int health,UnitSO unit)
    {
        _maxHealth = health;
        Health = _maxHealth;
        _count = 0;
        _knockbacks.Clear();
        _knockbacks.AddRange(unit.knockbackList);
        _knockbacks.Sort();
        _knockbacks.Reverse();
    }

    public void Receive()
    {
        Health = _maxHealth / 2;
        OnReceive?.Invoke();
        while (_knockbacks.Count > _count && Health <= _maxHealth * _knockbacks[_count])
            _count++;
    }


    public void Damage(int dmg)
    {
        if (Dead) return;

        if (CanTakedamage?.Invoke(dmg) == false) return;

        Health -= dmg;

        if (Dead)
        {
            Debug.Log(gameObject.name +" 사망");
            OnDeath?.Invoke();
            return;
        }
        OnDamaged?.Invoke();
        bool canHit = false;
        while (_knockbacks.Count >_count&& Health <= _maxHealth * _knockbacks[_count])
        {
            _count++;
            canHit = true;
        }
        if (canHit) OnKnockbacked?.Invoke();


    }

    public float GetPercentage()
    {
        return (float)Health / _maxHealth * 100f;
    }
}
