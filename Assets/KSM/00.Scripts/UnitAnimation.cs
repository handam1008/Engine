using System;
using UnityEngine;

public class UnitAnimation : MonoBehaviour
{
    public event Action OnAnimEnd;

    public void AnimEnd()
    {
        OnAnimEnd?.Invoke();
    }
}