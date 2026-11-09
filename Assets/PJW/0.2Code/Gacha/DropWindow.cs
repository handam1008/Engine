using DG.Tweening;
using System.Collections;
using System.Data;
using System.Reflection.PortableExecutable;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DropWindow : MonoBehaviour
{
    private Sequence sequence;

    public RectTransform box;

    public ParticleSystem particle;

    public Volume volume;

    private Bloom bloomEffect;


    private void Start()
    {
        box.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutQuint);
        GetVolume();
    }
    public void MM()
    {
        particle.Play();
        if (sequence != null && sequence.IsActive())
        {
            sequence.Kill();
        }
            sequence = DOTween.Sequence();
        sequence.Append(box.DOScale(box.localScale *1.5f, 0.1f)
            .SetEase(Ease.OutElastic))
            .Join(box.DORotate(Vector3.forward * 5, 0.1f)
            .SetLoops(2, LoopType.Yoyo));
            
    }

    public void GetVolume()
    {
        if(volume != null && volume.profile.TryGet<Bloom>(out bloomEffect))
        {
            Debug.Log("성공적으로 volume컴포넌트를 가져왔습니다.");
        }
        else
        {
            Debug.Log("volume컴포넌트가 없습니다.");
        }
    }

    public void BloomEffect()
    {
        if(bloomEffect != null) { StartCoroutine(SettingVolume()); }
    }

    IEnumerator SettingVolume()
    {
        bloomEffect.intensity.value = 20;
        yield return new WaitForSeconds(0.5f);
        bloomEffect.intensity.value = 0;
    }
}