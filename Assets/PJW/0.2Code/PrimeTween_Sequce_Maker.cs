using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrimeTween_Sequce_Maker : MonoBehaviour
{
    private Sequence showitem;
    

    private void Awake()
    {
            PlayShowItemSequce();
    }

    private void Update()
    {
        if(Keyboard.current.aKey.wasPressedThisFrame)
        {
            showitem.
        }

        if(Keyboard.current.bKey.wasPressedThisFrame)
        {
            transform.localPosition = Vector2.up * 540;
        }
    }

    private void  PlayShowItemSequce()
    {
        showitem = Sequence.Create().
            Group(Tween.LocalPosition(
                transform,
                endValue: transform.localPosition + Vector3.down * 540,
                duration: 0.5f,
                ease: Ease.InOutBack
            ))
                .Insert(atTime: 0.3f, Sequence.Create())
                .Group(Tween.Scale(
                    transform,
                    endValue: new Vector2(1.2f, 0.8f),
                    duration: 0.15f,
                    ease: Ease.InOutBack
                ))
                    .Chain(Tween.Scale(transform,
                    endValue: new Vector2(1f, 1f),
                    duration: 0.15f,
                    ease: Ease.InOutBack
                ));

    }
}
