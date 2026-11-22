using MoreMountains.Feedbacks;
using System;
using UnityEngine;

public class VFX_Manger : MonoBehaviour
{
    [SerializeField] private MMF_Player _vfx0010;
    [SerializeField] private MMF_Player _vfx0020;
    private int count = 0;

    private void Awake()
    {
        _vfx0010?.PlayFeedbacks();
    }

    public void Click_UnzipUI()
    {
        if(count <= 2)
        {
            _vfx0020?.PlayFeedbacks();
            count ++;
        }
    }
}
