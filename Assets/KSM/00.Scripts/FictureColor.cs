using System;
using System.Collections;
using KSM._00.Scripts.Enemy;
using UnityEngine;

public class FictureColor : MonoBehaviour
{
    public SpriteRenderer inScreen;
    bool colorChanged;
    public void ChangeColor()
    {
        StartCoroutine(Change());
    }

    public IEnumerator Change()
    {
        inScreen.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        inScreen.color = new Color(0, 0, 0, 0);
    }
}
