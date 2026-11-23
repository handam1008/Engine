using Assets.PJW._0._4Clean_Up_Code;
using MoreMountains.Feedbacks;
using RYU.Script.Charcter;
using UnityEngine;

public class GachaResultDisplayMoudule : MonoBehaviour, IGachaEnterModule, IGachaExitModule
{
    [SerializeField] private GameObject fileui;
    [SerializeField] private MMF_Player vfx0030;
    
    public void Init()
    {
        vfx0030?.Initialization();
    }

    public void Enter()
    {
        if(this == null || vfx0030 == null) return;
        fileui.SetActive(true);
        vfx0030?.PlayFeedbacks();
    }

    public void Exit()
    {
        fileui.SetActive(false);
    }
}
