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


    private void Update()
    {
        if(Keyboard.current.eKey.wasPressedThisFrame)
            GetPercentage();
    }


    public void Damage(int dmg)
    {
        if (Dead) return;
        
        Health -= dmg;
       
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
        if (canHit) OnDamged?.Invoke();
           
        
    }
    
    public void GetPercentage()
    {
        _percent = (Health / _maxHealth)*100;
        Debug.Log(_percent);

    }
}