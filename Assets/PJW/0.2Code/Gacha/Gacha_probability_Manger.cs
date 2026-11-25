using RYU.Script.Charcter;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Gacha_probability_Manger : MonoBehaviour
{
    [SerializeField] private CharacterDataSoList CharacterData;

    [SerializeField] private int fristCharacterNumber = 0;

    private Dictionary<int, float> weighsDic = new();

    private float sumWeigh;
    
    public static Gacha_probability_Manger instance;

    public CharacterDataSO CharacterDataSO { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        weighsDic.Clear();
        sumWeigh = 0;

        for (int i = 0; i < CharacterData.charcterDatas.Length;i++)
        {
            sumWeigh += CharacterData.charcterDatas[i].Weight;
        }

        for(int i = 0; i < CharacterData.charcterDatas.Length;i++)
        {
            weighsDic.Add(i,CharacterData.charcterDatas[i].Weight);
        }
    }

    public CharacterDataSO SelectCharacter()
    {
        if (sumWeigh <= 0) return null;

        float n = Random.Range(0, sumWeigh);

        foreach (var pair in weighsDic)
        {
            if(n<pair.Value)
            {
                if(CharacterData.currentCharacter.Length <= 1)
                {
                    CharacterDataSO = CharacterData.charcterDatas[fristCharacterNumber];
                    CharacterData.charcterDatas.Append(CharacterDataSO);
                    return CharacterData.charcterDatas[fristCharacterNumber];
                }
                CharacterDataSO = CharacterData.charcterDatas[pair.Key];
                CharacterData.charcterDatas.Append(CharacterDataSO);
                return CharacterData.charcterDatas[pair.Key];
            }
            n -= pair.Value;
        }

        return null;
    }
}
