using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Awake()
    {
        vicotryText.SetText("Wow you've won!! 1972Gold GIVEN!!!!!!");
    }

    private void Update()
    {
        if(Keyboard.current.pKey.wasPressedThisFrame)
        {
            StartCoroutine(Wait());
        }
    }

    private void StageUnlock()
    {
        foreach(Transform child in stageParent.transform) child.gameObject.SetActive(false);
        _sm.StageOpen(stageCount);
        ++stageCount;
        _stageBtnParent.SetActive(true);
        
    }

    IEnumerator Wait()
    {
        _stageCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        StageUnlock();

    }
}
