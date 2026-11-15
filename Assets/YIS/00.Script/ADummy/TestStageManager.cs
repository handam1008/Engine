using System;
using UnityEngine;

public class TestStageManager : MonoBehaviour
{
    [SerializeField] private GameObject[] buttens;


    public void StageOpen(int index)
    {
        try
        {
            buttens[index].SetActive(true);
        }
        catch(IndexOutOfRangeException)
        {
            index = buttens.Length;
            Debug.Log($"스테이지 전부 해금!!!!{index}");
            return;
        }

    }
}
