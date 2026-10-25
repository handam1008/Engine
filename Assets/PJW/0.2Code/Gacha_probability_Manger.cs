using JetBrains.Annotations;
using RYU.Script.Charcter;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gacha_probability_Manger : MonoBehaviour
{
    [field:SerializeField]public CharacterDataSoList CharacterData {  get;private set; }

    private Dictionary<string, float> weighsDic = new();

    private float sumWeigh;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        weighsDic.Clear();
        sumWeigh = 0;

        for(int i = 0; i < CharacterData.charcterDatas.Length;i++)
        {
            sumWeigh += CharacterData.charcterDatas[i].weight;
        }
        for(int i = 0; i < CharacterData.charcterDatas.Length;i++)
        {
            weighsDic.Add($"{CharacterData.charcterDatas[i].name}", ((CharacterData.charcterDatas[i].weight)*sumWeigh*100));
        }
    }

    public void SelectCharacter()
    {
        if (sumWeigh <= 0) return;

        float n = Random.Range(0, sumWeigh);

        foreach (var pair in weighsDic)
        {
            if(n<pair.Value)
            {
                Debug.Log(pair.Key);
                return;
            }

            n -= pair.Value;
        }
    }
}
