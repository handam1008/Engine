using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealthSystem : MonoBehaviour ,IDamageAble
{
    [field:SerializeField] public int Health { get; private set; }
    [SerializeField] private int _maxHealth;
    public event Action OnDamged;
    public event Action OnDeath;
    public bool Dead => Health <= 0;
    private UnitSO _unitSo;
    private int _count;
    private int _percent;
    

    public void Init(int health,UnitSO unit)
    {
        _maxHealth = health;
        Health = _maxHealth;
        _unitSo = unit;
        unit.knockbackList.Sort();
        unit.knockbackList.Reverse();
    }



    public void Damage(int dmg)
    {
        if(Dead) return;
        Health -= dmg;
        if (_unitSo.knockbackList.Count > _count)
        {
            if(Health < _maxHealth *_unitSo.knockbackList[_count])
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
    
    public int GetPercentage()
    {
        _percent = (Health / _maxHealth)*100;
        return _percent;

    }
}