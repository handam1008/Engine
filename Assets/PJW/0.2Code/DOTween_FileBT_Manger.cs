using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DOTween_FileBT_Manger : MonoBehaviour
{
    [SerializeField] private RectTransform[] bts;

    [SerializeField] private RectTransform brid;

    private Sequence selectAnim;

    private void Awake()
    {
        brid.DOScale(0f, 0f);
    }

    public void CompareNumberAnim(int i)
    {
        switch (i)
        {
            case 1:
                SelectAnimPlay(0, 1, 2);
                break;

            case 2:
                SelectAnimPlay(1, 0, 2);
                break;

            case 3:
                SelectAnimPlay(2, 1, 0);
                break;

        }
    }

    public void SelectAnimPlay(int i,int j,int k)
    {
        if(selectAnim != null && selectAnim.IsActive())
        {
            selectAnim.Kill();
        }
        selectAnim = DOTween.Sequence();

        selectAnim.Append(
            bts[i].DOMoveX(
                bts[i].position.x - (i == 0 ? 0 : i == 1 ? 600 : 1200), 0.8f))
            .Join(
            bts[j].DOMoveY(
                bts[j].position.y + 840, 0.5f))
            .Join(bts[k].DOMoveY(
                bts[k].position.y + 840, 0.5f)).OnComplete(() =>
                {
                    FileBrid();
                });
        
    }

    public void FileBrid()
    {

        Vector3 startpos = brid.position+Vector3.up*50f;
        Vector3 endpos = startpos + Vector3.right * 1200;
        Vector3 midpos = (startpos + endpos) * 0.5f + Vector3.up * 250;
        Vector3[] trms = new Vector3[] { midpos, endpos };
        brid.DOScale(2.5f, 0f);
        brid.DOPath(trms, 10f, PathType.CatmullRom).SetEase(Ease.OutQuint);
    }
}
