using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using DG.Tweening;
using System.Collections;
using System.Data;
using System.Reflection.PortableExecutable;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class DropWindow : MonoBehaviour
{
    private Sequence sequence;

    public RectTransform box;

    public ParticleSystem particle;

    public Volume volume;

    private Bloom bloomEffect;

    public SoundClipSO soundClipSo;

    private int count = 0;

    //public Slider slider;

    //public GameObject fill_Area;

    private void Start()
    {
        count = 0;
        //box.DOAnchorPosY(box.localPosition.y-1800, 0.5f).SetEase(Ease.OutQuint);
        GetVolume();
    
    }
    public void MM()
    {
        if(count < 3)
        { 
            particle.Play();
            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }
            if (bloomEffect != null) { StartCoroutine(SettingVolume()); }
                sequence = DOTween.Sequence();
            sequence.Append(box.DOScale(box.localScale *1.1f, 0.1f)
                .SetEase(Ease.OutElastic))
                .Join(box.DORotate(Vector3.forward * 5, 0.1f)
                .SetLoops(2, LoopType.Yoyo));
            count++;
        }
        else
        {
        }
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

    IEnumerator SettingVolume()
    {
            ServiceLocator.Get<IAudioService>().PlaySfx(soundClipSo);
            bloomEffect.intensity.value = 20;
            yield return new WaitForSeconds(0.3f);
            bloomEffect.intensity.value = 0;
    }

    //public void SettingSlider()
    //{
    //    if(count == 1)
    //    {
    //        fill_Area.SetActive(true);
    //    }
    //    slider.value += (float)1/3;
    //    Debug.Log($"Slider value: {slider.value}"); 
    //}
}