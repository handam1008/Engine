using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TestGameEndTrigger : MonoBehaviour
{
    [Header("스테이지 종류, 해금 관련")]
    [SerializeField] private GameObject stageParent;
    [SerializeField] private TestStageManager _sm;
    [SerializeField] private GameObject _stageBtnParent;

    [Header("게임 결과 관련")]
    [SerializeField] private Canvas _stageCanvas;
    [SerializeField] private TextMeshProUGUI vicotryText;
    private int stageCount = 1;
    [SerializeField] private Image fadePanel;
    [SerializeField] private MoneyManager mm;


    private void Awake()
    {
        vicotryText.SetText("Wow you've won!! 1972Gold GIVEN!!!!!!");
    }

    public void End()
    {
      StartCoroutine(Wait());
      fadePanel.DOFade(1, 1);
      mm.PlusMoney(1972);
    }

    private void StageUnlock()
    {


        _sm.StageOpen(stageCount);
        ++stageCount;
        fadePanel.DOFade(0, 1);
        _stageBtnParent.SetActive(true);


    }

    

    IEnumerator Wait()
    {
        _stageCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        foreach(Transform child in stageParent.transform) child.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        StageUnlock();

    }
}
