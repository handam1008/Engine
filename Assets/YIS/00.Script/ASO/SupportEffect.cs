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

    [Header("UI용")]
    public TextMeshProUGUI ComboNameText;
    public Image[] CharacterSpriteUI;
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
            if (Intersetion.Length != effects[i].characters.Length || effects[i].characters.Length == 0)
            {
                Debug.LogWarning("콤보 유닛이 없거나 부족함");
            }
            
            DosomethingUI();
            GetSupport(effects[i].stype, Intersetion);
                
        }
         
    }

    private void DosomethingUI()
    {
        for(int i = 0; i < effects.Count; ++i)
        {
            effects[i].ComboNameText.SetText(effects[i].name);

                
            for(int j = 0; j < effects[i].CharacterSpriteUI.Length; ++j)
            {
                try
                {
                 effects[i].CharacterSpriteUI[j].sprite = Intersetion[j].sprite;
                    Debug.Log("Changed");
                }
                catch(IndexOutOfRangeException)
                {
                    effects[i].CharacterSpriteUI[j].sprite = null;
                    return;
                }
            }
        }
    }


    public void GetSupport(SupportType type, CharcterDataSo[] charcterList)
    {
        //switch (type)
        //{ 
        //    case SupportType.HpUp:
        //        for(int i = 0; i < charcterList.Length; ++i)
        //        {
        //            charcterList[i].maxHp += effects[i].correctionVal;
        //        }
        //        break;
        //    case SupportType.Speed:
        //        for(int i = 0; i < charcterList.Length; ++i)
        //        {
        //            charcterList[i].moveSpeed += effects[i].correctionVal;
        //        }
        //        break;
        //    case SupportType.AttackUp:
        //        for (int i = 0; i < charcterList.Length; ++i)
        //        {
        //            charcterList[i].attackDmg += effects[i].correctionVal;
        //        }
        //        break;
                
        //}
        //todo: ↑  이렇게 하면 So를 직접적으로 건드는거라 추후에 Ondead그거 상태 만들면 ""반드시"" 리셋 메서드를
        //쓰던가 뭔가를 해야함
    }
}
