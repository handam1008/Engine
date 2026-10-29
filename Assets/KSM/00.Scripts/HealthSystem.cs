using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealthSystem : MonoBehaviour ,IDamageAble
{
    [field:SerializeField] public int Health { get; private set; }
    [SerializeField] private int _maxHealth;
    public event Action OnKnockbacked;
    public event Action OnDamaged;
    public event Action OnDeath;
    public bool Dead => Health <= 0;
    private UnitSO _unitSo;
    private int _count;
    private float _percent;
    

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
        if (Dead) return;
        
        Health -= dmg;
        OnDamaged?.Invoke();
        if (Dead)
        {
            Debug.Log(gameObject.name +" 사망");
            OnDeath?.Invoke();
            return;
        }
        bool canHit = false;
        while (_unitSo.knockbackList.Count >_count&& Health <= _maxHealth * _unitSo.knockbackList[_count])
        {
            _count++; 
            canHit = true;
        }
        if (canHit) OnKnockbacked?.Invoke();
           
        
    }
    
    public float GetPercentage()
    {
        _percent = (float)Health / _maxHealth *100;
        return _percent;

    }
}