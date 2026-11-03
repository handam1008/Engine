using DG.Tweening;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;

public class DropWindow : MonoBehaviour
{
    private Sequence sequence;

    public RectTransform box;

    private void OnEnable()
    {
        sequence = DOTween.Sequence();
    }

    private void Start()
    {
        box.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutQuint);
    }
    public void MM()
    {
        sequence.Append(box.DOScale(box.localPosition * 1.1f, 0.5f)).SetLoops(2, LoopType.Yoyo).Join(box.DORotate(Vector3.forward * 2, 0.5f).SetLoops(2, LoopType.Yoyo));
    }

}