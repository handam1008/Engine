using RYU.Script.Charcter;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum SupportType {HpUp, Speed, AttackUp }

[Serializable]
public class Effect
{
    [Header("콤보 명")]
    public string name; 
    [Header("지원관계")]
    public CharcterDataSo[] characters;

    [Header("보정치")]
    [SerializeField, Min(0)] public int correctionVal;
    
    [Header("지원 종류")]
    public SupportType stype;

    [NonSerialized] public string supportDiscription;
}


[CreateAssetMenu(fileName = "SupportEffect", menuName = " Support/SupportEffect")]
public class SupportEffect : ScriptableObject
{
    public List<Effect> effects = new List<Effect>();

    public CharacterDataSoList currentDeck;

    Dictionary<SupportType, ISupportAction> SupportDic = new Dictionary<SupportType, ISupportAction>()
    {
        {SupportType.HpUp, new HpSupport() },
        {SupportType.AttackUp, new DamageSupport()},
        {SupportType.Speed, new SpeedSupport()}
    };

    public void TrySupport()
    {
        for(int i = 0; i < effects.Count; ++i)
        {
            CharcterDataSo[] intersetion = effects[i].characters.Intersect(currentDeck.currentCharacter).ToArray();
            
            if (intersetion.Length != effects[i].characters.Length || effects[i].characters.Length == 0)
            {
                Debug.LogWarning("콤보 유닛이 없거나 부족함");
                continue;
            }
            GetSupport(i, intersetion);
        }
         
    }

    
    public void GetSupport(int index, CharcterDataSo[] charcterList)
    {
        
        Effect combo = effects[index];

        if(!SupportDic.TryGetValue(combo.stype, out ISupportAction doSuppoet))
        {
            Debug.LogWarning("타입이 없음 지정해줭");
            return;
        }
        Debug.Log("In");

        foreach(CharcterDataSo charcter in charcterList)
        {
            doSuppoet.ApplyCorrection(charcter, combo.correctionVal);
            combo.supportDiscription = doSuppoet.SupportDiscription(combo.correctionVal);
        }
            Debug.Log(combo.supportDiscription);

        //todo: ↑  이렇게 하면 So를 직접적으로 건드는거라 추후에 Ondead그거 상태 만들면 ""반드시"" 리셋 메서드를
        //쓰던가 뭔가를 해야함
    }
}
