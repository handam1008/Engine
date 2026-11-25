using Assets.PJW._0._4Clean_Up_Code;
using MoreMountains.Feedbacks;
using RYU.Script.Charcter;
using UnityEngine;

public class GachaResultDisplayMoudule : MonoBehaviour, IGachaEnterModule, IGachaExitModule
{
    [SerializeField] private GameObject fileui;
    [SerializeField] private MMF_Player vfx0030;
    [SerializeField] private Material material;
    [SerializeField] private GameObject cutfileui;
    
    public void Init()
    {
        vfx0030?.Initialization();
        cutfileui.SetActive(false);
    }

    public void Enter()
    {
        if(this == null || vfx0030 == null) return;
        fileui.SetActive(true);

        CharcterRank charcterRank = Gacha_probability_Manger.instance.CharacterDataSO.charcterRank;
        float value = 0;
        if (charcterRank == CharcterRank.NORMAL)
        {
            value = 1;
        }
        else if (charcterRank == CharcterRank.RARE)
        {
            value = -0.08f;
        }
        else if(charcterRank == CharcterRank.HIDDEN)
        {
            value = -8.23f;
        }
        material.SetFloat("_Saturation", value);
        
        vfx0030?.PlayFeedbacks();
    }

    public void Exit()
    {
        fileui.SetActive(false);
    }
}
