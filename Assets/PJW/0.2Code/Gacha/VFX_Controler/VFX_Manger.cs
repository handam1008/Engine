using MoreMountains.Feedbacks;
using UnityEngine;

public class VFX_Manger : MonoBehaviour
{
    [Header("Drop_UnZip_WindowPanel")]
    [SerializeField] private MMF_Player fristVFX;

    public void Init() { }

    public void Start()
    {
        fristVFX.PlayFeedbacks();
    }
}
