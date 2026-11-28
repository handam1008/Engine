using System;
using System.Collections.Generic;
using KSM._00.Scripts;
using RYU.Script.Charcter;
using UnityEngine;

public class TypeDamageManager : MonoBehaviour
{
    public static TypeDamageManager instance;
    private void Awake()
    {
        if(instance == null)
        instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
            
    }

    public int TypeChecking(AbstractUnit hitTarget,AbstractUnit attacker , int dmg)
    {
        if (attacker == null || hitTarget == null) return dmg;

        if (attacker.UnitSo is CharacterDataSO ally && hitTarget.UnitSo is EnemySO enemy)
            return EnemyTypeChecking(enemy, ally, dmg);

        if (attacker.UnitSo is EnemySO attackingEnemy && hitTarget.UnitSo is CharacterDataSO defendingAlly)
            return CharaterChecking(defendingAlly, attackingEnemy, dmg);

        return dmg;

    }

    public int CharaterChecking(CharacterDataSO hitTarget,EnemySO attacker, int dmg)
    {
        foreach (var rule in hitTarget.WhatIsStrong)
        {
            if ((rule.charaterType & attacker.charaterType) != CharaterType.None)
            {
                return Mathf.CeilToInt((float)dmg / rule.scale);
            }
        }

        return dmg;
    }

    public int EnemyTypeChecking(EnemySO hitTarget,CharacterDataSO attacker, int dmg)
    {
        foreach (var rule in attacker.WhatIsStrong)
        {
            if ((rule.charaterType & hitTarget.charaterType) != CharaterType.None)
            {
                return dmg * Mathf.Max(1, rule.scale);
            }
        }

        return dmg;
    }
    
}

[Flags]
public enum CharaterType
{
    None       = 0,
    Virus      = 1 << 0,   // 1
    Wiper      = 1 << 1,   // 2
    Worm       = 1 << 2,   // 4
    Ransomware = 1 << 3,   // 8
    Adware     = 1 << 4,   // 16
}

[Serializable]
public struct TypeDamage
{
    public CharaterType charaterType;
    public int scale;
}