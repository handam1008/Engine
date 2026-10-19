using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UIElements;
using Unity.Properties;
using System.Collections.Generic;

public class DOTween_Sequce_Maker : MonoBehaviour
{

    private Sequence showItemAnim;
    private Sequence scaleAnim;
    private CanvasGroup selectImage;
    //private Sequence pointEnterAnim;
    //private Sequence pointExitAnim;


    private void OnEnable()
    {
        showItemAnim = DOTween.Sequence();
        scaleAnim = DOTween.Sequence();
        //pointEnterAnim = DOTween.Sequence();
        //pointExitAnim = DOTween.Sequence();
        ShowSequenceMake();
        PointEnterAnimMake();
        pointExitAnimMake();
    }

    private void Awake()
    {
    }

    private void Start()
    {
        selectImage = transform.parent.Find("SeletImage").GetComponent<CanvasGroup>();
        selectImage.alpha = 0f;
        PlaySequence();
        ScaleAnimMake();
    }

    private void Update()
    {

    }

    public void PlaySequence()
    {
        showItemAnim.Restart();
    }

    public void Point_Enter()
    {

       if (!showItemAnim.IsActive() || !showItemAnim.IsPlaying())
            //pointEnterAnim.Restart();
            selectImage.DOFade(1f, 0.01f);
        
    }

    public void Point_Exit()
    {
        //pointExitAnim.Restart();
        selectImage.DOFade(0f, 0.01f);
    }

    public void ScaleAnimPlay()
    {
        scaleAnim.Play();
    }

    public void ShowSequenceMake()
    {
        showItemAnim.Append(transform.parent.DOMoveY(transform.parent.position.y - 800, 0.5f).SetEase(Ease.InOutBack))
            .Insert(0.3f, transform.DOScale(new Vector2(1.2f, 0.8f), 0.15f).SetEase(Ease.InOutBack))
            .Append(transform.DOScale(Vector2.one, 0.15f).SetEase(Ease.InOutBack)).OnComplete(ScaleAnimPlay);
    }

    public void PointEnterAnimMake()
    {
        //pointEnterAnim.Append(transform.DORotate(Vector3.forward*60,0.5f)).Pause();
    }

    public void pointExitAnimMake()
    {
        //pointExitAnim.Append(transform.DORotate(Vector3.zero, 0.5f)).Pause();
    }

    public void ScaleAnimMake()
    {
        scaleAnim.Append(transform.parent.DOScale(1.03f, 0.18f).SetEase(Ease.OutQuint)).Append(transform.parent.DOScale(1f, 0.5f).SetEase(Ease.OutQuint)).SetLoops(-1, LoopType.Restart).Pause();
    }

    private void OnDisable()
    {
        showItemAnim.Kill();
        //pointEnterAnim.Kill();
        //pointExitAnim.Kill();
    }
}
