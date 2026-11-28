using Assets.PJW._0._4Clean_Up_Code;
using RYU.Script.Charcter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaRealResultDisplayMoudule : MonoBehaviour,IGachaEnterModule,IGachaMarkModule,IGachaExitModule
{
    [SerializeField] private GameObject resultUi;

    [SerializeField] private Image imageUI;

    [SerializeField] private TextMeshProUGUI textUI;

    public void Init()
    {
        resultUi.SetActive(true);
    }

    public void Enter()
    {

    }

    public void Mark(CharacterDataSO characterDataSO)
    {
        Debug.Log($"{characterDataSO.Name}");
        imageUI.sprite = characterDataSO.sprite;
        textUI.text = $"{characterDataSO.charcterRank}";
    }

    public void Exit()
    {
        resultUi.SetActive(false);
    }
}
