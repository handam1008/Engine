using NUnit.Framework;
using RYU.Script.Charcter;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum SupportType {HpUp, Speed, AttackUp }

[Serializable]
public class Effect
{
    public CharcterDataSo[] characters;
    public SupportType stype;
}


[CreateAssetMenu(fileName = "SupportEffect", menuName = " Support/SupportEffect")]
public class SupportEffect : ScriptableObject
{
    public List<Effect> effects = new List<Effect>();

    public CharacterDataSoList currentDeck;
    
    private CharcterDataSo[] Intersetion;


    
    public void TrySupport()
    {
        for(int i = 0; i < effects.Count; ++i)
        {
            Intersetion = effects[i].characters.Intersect(currentDeck.currentCharacter).ToArray();
            if (Intersetion.Length != effects[i].characters.Length)
            {
                Debug.LogWarning("콤보 유닛이 없거나 부족함");
            }
            
                foreach (var charcter in Intersetion)
                {
                    GetSupport(effects[i].stype);
                }
        }
         
    }

    public void GetSupport(SupportType type)
    {
        Debug.Log($"{type}의 효과를 받았다.");
    }
}
