using DevLib.ServiceLocator;
using DG.Tweening;
using RYU.Script.Service.GoldService;
using RYU.Script.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TestGameEndTrigger : MonoBehaviour
{
    [Header("스테이지 종류, 해금 관련")]
    [SerializeField] private GameObject stageParent;
    [SerializeField] private NetworkStageWindow _network;

    [Header("게임 결과 관련")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private TextMeshProUGUI vicotryText;
    [SerializeField] private Image fadePanel;


    public void End(int gold)
    {
      vicotryText.SetText($"Wow you've won!! {gold}Gold GIVEN!!!!!!");
      StartCoroutine(Wait());
      fadePanel.DOFade(1, 1);
      ServiceLocator.Get<IGoldService>().AddGold(gold);
    }

    private void StageUnlock()
    {


        fadePanel.DOFade(0, 1);
        _network.OnStageCleared();


    }

    

    IEnumerator Wait()
    {
        _victoryPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        foreach(Transform child in stageParent.transform) child.gameObject.SetActive(false);
        _victoryPanel.SetActive(false);
        foreach (var unit in FindObjectsByType<AbstractUnit>(FindObjectsSortMode.None)) Destroy(unit.gameObject);
        yield return new WaitForSeconds(1f);
        StageUnlock();

    }
}
