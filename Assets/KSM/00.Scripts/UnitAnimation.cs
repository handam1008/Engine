using System;
using UnityEngine;

public class UnitAnimation : MonoBehaviour
{
    public event Action OnAnimEnd;
    public event Action OnAttacked;

    public void Attack()
    {
        OnAttacked?.Invoke();
    }
    

    public void AnimEnd()
    {
        OnAnimEnd?.Invoke();
    }
}