using Assets.PJW._0._4Clean_Up_Code;
using DG.Tweening;
using TMPro;
using UnityEngine;

//폴더 선택 후, 압축 해제 전에 뜨는 "압축 풀기 마법사" 창. 확인 버튼은 GachaOwner.OnWizardConfirm 호출
public class GachaUnzipWizardModule : MonoBehaviour, IGachaEnterModule, IGachaExitModule
{
    [SerializeField] private GameObject _wizardWindow;
    [SerializeField] private TextMeshProUGUI _bodyText;
    [SerializeField, TextArea(3, 8)] private string _bodyFormat;
    [SerializeField] private float _showDelay = 0.5f; 

    public void SelectFile(TMP_Text label)
    {
        string fileName = label.text.EndsWith(".zip") ? label.text : label.text + ".zip";
        _bodyText.text = string.Format(_bodyFormat, fileName);
    }

    public void Init()
    {
        _wizardWindow.transform.localScale = Vector3.zero;
        _wizardWindow.SetActive(true);
    }

    public void Enter()
    {
        _wizardWindow.transform.DOScale(1f, 0.25f).SetDelay(_showDelay).SetEase(Ease.OutBack);
    }

    public void Exit()
    {
        _wizardWindow.transform.DOKill();
        _wizardWindow.SetActive(false);
    }
}
