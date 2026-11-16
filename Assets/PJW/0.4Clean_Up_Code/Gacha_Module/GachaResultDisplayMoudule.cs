using Assets.PJW._0._4Clean_Up_Code;
using MoreMountains.Feedbacks;
using UnityEngine;

public class GachaResultDisplayMoudule : MonoBehaviour, IGachaEnterModule, IGachaPlayModule, IGachaExitModule,IGachaMarkModule
{
    public void Init()
    {
        Debug.Log($"{this.name}가 초기화됨");
    }

    public void Enter()
    {
    }

    public void Play()
    {
    }

    public void Mark(int index)
    {

    }

    public void Exit()
    {
    }

}
