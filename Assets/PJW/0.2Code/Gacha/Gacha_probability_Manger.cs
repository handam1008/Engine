using DG.Tweening;
using JetBrains.Annotations;
using RYU.Script.Charcter;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Gacha_probability_Manger : MonoBehaviour
{
    [field:SerializeField]public CharacterDataSoList CharacterData {  get;private set; }

    [SerializeField] private TextMeshProUGUI rankText;

    public ParticleSystem showCH;

    public Image showCharcter;

    private Dictionary<int, float> weighsDic = new();

    private float sumWeigh;

    private Sequence showCharaterAnim;

    private void Awake()
    {
        showCharaterAnim = DOTween.Sequence();
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        weighsDic.Clear();
        sumWeigh = 0;
        showCharcter.transform.DOScale(0f, 0f);

        for (int i = 0; i < CharacterData.charcterDatas.Length;i++)
        {
            sumWeigh += CharacterData.charcterDatas[i].weight;
        }

        for(int i = 0; i < CharacterData.charcterDatas.Length;i++)
        {
            weighsDic.Add(i,CharacterData.charcterDatas[i].weight);
        }

        rankText.DOFade(0, 1);
    }

    public void SelectCharacter()
    {
        if (sumWeigh <= 0) return;

        float n = Random.Range(0, sumWeigh);

        foreach (var pair in weighsDic)
        {
            if(n<pair.Value)
            {
                StartCoroutine(StartParticle());
                StartCoroutine(ShowCharacter(pair.Key));
                return;
            }
            n -= pair.Value;
        }
    }


    IEnumerator StartParticle()
    {
        showCH.Play();
        yield return new WaitForSeconds(0.49f);
        showCH.Pause();
    }

    IEnumerator ShowCharacter(int value)
    {
        yield return new WaitForSeconds(0.5f);
        showCharcter.sprite = CharacterData.charcterDatas[value].sprite;
        showCharcter.transform.DOScale(1f, 0f);
        rankText.DOFade(1, 1);
        rankText.text = $"{CharacterData.charcterDatas[value].charcterRank}";
    }
}
