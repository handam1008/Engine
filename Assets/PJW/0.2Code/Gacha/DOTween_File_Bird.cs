using DG.Tweening;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DOTween_File_Bird : MonoBehaviour
{
    [SerializeField] private RectTransform brid;

    private void Awake()
    {
        FileBrid();
    }

    public void FileBrid()
    {
        Vector3 startpos = transform.position;
        Vector3 endpos = startpos+Vector3.right*600;
        Vector3 midpos = (startpos+endpos)*0.5f+Vector3.up*250;
        Vector3[] trms = new Vector3[] {midpos,endpos };

        brid.DOPath(trms, 10f, PathType.CatmullRom).SetEase(Ease.OutQuint);
    }
}
