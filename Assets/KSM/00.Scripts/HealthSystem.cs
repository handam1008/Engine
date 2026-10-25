using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour ,IDamageAble
{
    [SerializeField] private int _health;
    private int _maxHealth;
    public event Action OnDamged;
    public event Action OnDeath;
    public bool Dead => _health <= 0;
    private UnitSO _unitSo;
    private int _count;

    public void Init(int health,UnitSO unit)
    {
        _maxHealth = _health;
        _health = health;
        _unitSo = unit;
        unit.knockbackList.Sort();
        unit.knockbackList.Reverse();
    }
    public void Damage(int dmg)
    {
        if(Dead) return;
        _health -= dmg;
        if (_unitSo.knockbackList.Count > _count)
        {
            if(_health < _maxHealth *_unitSo.knockbackList[_count])
            { 
                OnDamged?.Invoke();
                _count++;
            }
        }

        if (Dead)
        {
            Debug.Log(gameObject.name +" 사망");
            OnDeath?.Invoke();
            
        }
    }
}