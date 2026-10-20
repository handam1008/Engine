using DG.Tweening;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class DOTween_FileBT_Manger : MonoBehaviour
{
    [SerializeField] private RectTransform[] bts;

    [SerializeField] private RectTransform panel;

    [SerializeField] private GameObject bar;

    [SerializeField] private RectTransform brid;

    private Sequence selectAnim;

    private bool wasPlayed = false;

    private void Awake()
    {
        panel.DOScale(0f, 0f);
        brid.DOScale(0f, 0f);
    }

    public void CompareNumberAnim(int i)
    {
        if (wasPlayed) return;  
        int a = 0;
        int b = 0;

                if(selectAnim != null && selectAnim.IsActive())
                {
                    selectAnim.Kill();
                }
                selectAnim = DOTween.Sequence();
        switch(i)
        {
            case 0:
                a = 1;
                b = 2;
                break;
            case 1:
                a = 0;
                b = 2;
                break;
            case 2:
                a = 0;
                b = 1;
                break;
        }

        selectAnim.Append(bts[a].DOMoveY(bts[a].position.y+840f,0.5f))
            .Join(bts[b].DOMoveY(bts[b].position.y+840,0.5f))
            .OnComplete(() =>
            {
                wasPlayed = true;
                ShowFile();
            });
    }


    public void ShowFile()
    {
        panel.DOScale(2.5f, 0f).OnComplete(() => { StartCoroutine(Loding()); brid.DOScale(0.4f, 0); Brid(); });
    }

    public  IEnumerator Loding()
    {
        for (int i = 0; i < 42; i++)
        {
            Instantiate(bar, bar.transform.parent);
            yield return new WaitForSeconds(0.055f);
        }
    }


    public void Brid()
    {
        Vector3[] trms = new Vector3[] {brid.position+new Vector3(750,70),brid.position+Vector3.right *1500};
        brid.DOPath(trms,3.5f,PathType.Linear);
    }
}
