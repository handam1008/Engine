using Assets.PJW._0._4Clean_Up_Code;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using MoreMountains.Feedbacks;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GachaArchiveRevealModule : MonoBehaviour, IGachaEnterModule, IGachaPlayModule, IGachaExitModule
{
    [Header("File_GameObject")]
    [SerializeField] private GameObject[] _file = new GameObject[3];
    [SerializeField] private Transform[] _fileTrm = new Transform[3];

    [Header("Selet_CanvasGroup")]
    [SerializeField] private CanvasGroup[] _seletimage = new CanvasGroup[3];

    private Sequence _showUnzipFieldSeq;
    private Sequence _unzipFieldscaleSeq;
    private Sequence _selectFieldSeq;

    private TaskCompletionSource<bool> _exit;

    private int filenumber = 0;

    public void Init()
    {
        if (_file[0] == null || _file[1] == null || _file[2] == null)
            throw new OperationCanceledException("현제 파일UI가 null상태입니다.");

        if (_seletimage[0] == null || _seletimage[1] == null || _seletimage[2] == null)
            throw new OperationCanceledException("현제 선택UI이미지가 null상태입니다.");

        _exit = new TaskCompletionSource<bool>();
    }

    public void Enter()
    {
        foreach (GameObject file in _file)
        {
            file.SetActive(true);
        }

        foreach (CanvasGroup seletimage in _seletimage)
        {
            seletimage.alpha = 0;
        }
    }

    public void Play()
    {
        ShowFielSelect();
    }

    public void ShowFielSelect()
    {
        foreach(GameObject file in _file)
        {
            if(!_showUnzipFieldSeq.IsActive() || _showUnzipFieldSeq == null)
                _showUnzipFieldSeq  = DOTween.Sequence();

            _showUnzipFieldSeq.Append(file.transform
                .DOMoveY(0f,0.5f)
                .SetEase(Ease.InOutBack))
                .Insert(0.3f,file.transform
                .DOScale(new Vector2(1.2f, 0.8f), 0.15f)
                .SetEase(Ease.InOutBack))
                .Append(file.transform
                .DOScale(Vector2.one, 0.15f)
                .SetEase(Ease.InOutBack))
                .OnComplete(UnzipFieldscaleSeq);
        }
    }

    public void UnzipFieldscaleSeq()
    {
        foreach (Transform file in _fileTrm)
        {
            if (!_unzipFieldscaleSeq.IsActive() || _unzipFieldscaleSeq == null)
                _unzipFieldscaleSeq = DOTween.Sequence();

            _unzipFieldscaleSeq
                .Append(file
                .DOScale(1.03f, 0.18f)
                .SetEase(Ease.OutQuint))
                .Append(file
                .DOScale(1f, 0.5f)
                .SetEase(Ease.OutQuint))
                .SetLoops(-1, LoopType.Restart)
                .Play();
        }
    }

    public async Task SelectFieldSeq()
    {
        if(!_selectFieldSeq.IsActive() || _selectFieldSeq == null)
            _selectFieldSeq = DOTween.Sequence();

        if (filenumber == 0)
        {
            _selectFieldSeq.Append(_fileTrm[1]
                .DOLocalMoveY(800, 0.5f))
                .SetEase(Ease.InOutBack)
                .Join(_fileTrm[2]
                .DOLocalMoveY(800, 0.5f))
                .SetEase(Ease.InOutBack)
                .Join(_fileTrm[0]
                .DOLocalMoveX(600,0.5f))
                .Play();
        }
        else if (filenumber == 1)
        {
            _selectFieldSeq.Append(_fileTrm[0]
                .DOLocalMoveY(800, 0.5f))
                .SetEase(Ease.InOutBack)
                .Join(_fileTrm[2]
                .DOLocalMoveY(800, 0.5f))
                .SetEase(Ease.InOutBack)
                .Play();
        }
        else if (filenumber == 2)
        {
            _selectFieldSeq.Append(_fileTrm[0]
                .DOLocalMoveY(800, 0.5f))
                .SetEase(Ease.InOutBack)
                .Join(_fileTrm[1]
                .DOLocalMoveY(800, 0.5f))
                .SetEase(Ease.InOutBack)
                .Join(_fileTrm[2]
                .DOLocalMoveX(-600,0.5f))
                .Play();
        }
        await Task.Delay(500);
    }

    public void Point_Enter(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 1;
    }

    public void Point_Exit(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0;
    }


    public async void Exit()
    {
        _showUnzipFieldSeq.Kill();
        _showUnzipFieldSeq.Kill();

        await SelectFieldSeq();
        foreach(GameObject file in _file)
        {
            file.SetActive(false);
        }
    }

    public void FileNumberSelet(int count)
    {
        filenumber = count;
    }
}
