using DG.Tweening;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;

public class DOTween_FileBT_Manger : MonoBehaviour
{
    [SerializeField] private RectTransform[] bts;

    [SerializeField] private RectTransform panel;

    [SerializeField] private GameObject bar;

    [SerializeField] private RectTransform brid;

    [SerializeField] private TextMeshProUGUI remainingTime;

    private Sequence selectAnim;

    private Sequence bridSeq;

    private bool wasPlayed = false;

    [SerializeField]private Gacha_probability_Manger probabilityManger;

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
        int c = 0;

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
                c = 0;
                break;
            case 1:
                a = 0;
                b = 2;
                c = 1;
                break;
            case 2:
                a = 0;
                b = 1;
                c = 2;
                break;
        }

        selectAnim.Append(bts[a].DOMoveY(bts[a].position.y+840f,0.5f))
            .Join(bts[b].DOMoveY(bts[b].position.y+840,0.5f))
            .OnComplete(() =>
            {
                wasPlayed = true;
                bts[c].DOScale(0f, 0f);
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
        if(bridSeq == null || !bridSeq.IsActive())
        {
            bridSeq = DOTween.Sequence();
        }

        float timer = 0f;
        Vector3[] trms = new Vector3[] {brid.position+new Vector3(750,70),brid.position+Vector3.right *1500};
        bridSeq?.Append(brid.DOPath(trms, 2.7f, PathType.Linear))
        ?.Join(DOVirtual.DelayedCall(0.26f, () => { timer++; remainingTime.text = $"남은 시간: {10-timer}초"; }).SetLoops(10)).
        OnComplete(() =>
        {
            ShowCharacter();
        })?.Play();
    }

    public void ShowCharacter()
    {
        panel.transform.DOScale(0f, 0f);
        brid.transform.DOScale(0f, 0f);
        probabilityManger.SelectCharacter();
    }
}
