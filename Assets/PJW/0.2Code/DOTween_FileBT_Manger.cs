using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DOTween_FileBT_Manger : MonoBehaviour
{
    [SerializeField] private List <Transform> bts = new();
    private Sequence selectAnim0;
    private Sequence selectAnim1;
    private Sequence selectAnim2;

    private void Awake()
    {
        selectAnim0 = DOTween.Sequence();
        selectAnim1 = DOTween.Sequence();
        selectAnim2 = DOTween.Sequence();
    }
    public void SelectAnimPlay(int i)
    {
        switch (i)
        {
            case 1:
                selectAnim0.Restart();
            break;

            case 2:
                selectAnim1.Restart();
            break;

            case 3:
                selectAnim2.Restart();
            break;

        }           
    }
}
