using DG.Tweening;
using TMPro;
using UnityEngine;

public class Item_Text_Manger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] texts;

    private string[] fileTitles = {"zip","메모장","Suhang.zip","Assets","아무거나","새폴더","다운로드","제목없음","임시파일","찐찐찐최종","이름없음"};

    private void Start()
    {
        NameFileTitle();
    }

    private void NameFileTitle()
    {
        for(int i  = 0; i < texts.Length;i++)
        {
            texts[i].text = fileTitles[(Random.Range(0, fileTitles.Length))];
        }
    }
}
