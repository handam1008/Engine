using Assets.PJW._0._4Clean_Up_Code;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using RYU.Script.Charcter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GachaOwner : MonoBehaviour
{
    private Dictionary<Type, IGachaModule> _moduleDict = new();

    //위코드 mmf_player가 igachamodule과 매칭되기 때문에 이렇게 나눌빠에는 구조체로 묶는게 좋다는 쌤의 조언

    private IGachaModule _currentModule;

    private TaskCompletionSource<bool> firstClickSource;

    private TaskCompletionSource<bool> finshThevfx0010;

    private TaskCompletionSource<bool> finshThevfx0020;

    private TaskCompletionSource<bool> lastClickSource;

    private TaskCompletionSource<bool> finshThevfx0030;


    private CharacterDataSO currentCharacterData;

    private bool isFristClick = false;

    private bool isLastClick = false;

    private int clickcount = 0;

    [SerializeField] private Material material;

    [SerializeField] private MMF_Player vfx0040;

    [SerializeField] private GameObject cutfilevfxvisual;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _moduleDict = GetComponentsInChildren<IGachaModule>().ToDictionary(igachamodule_value=>igachamodule_value.GetType(), igachamodule_value=> igachamodule_value);
        #region "[ToDictionarty]"
        //
        //foreach(var module in GetComponentsInChildren<IGachaModule>())
        //{
        //    _moduleDict.Add(module.GetType(), module);
        //}
        #endregion
        _currentModule = _moduleDict[typeof(GachaArchiveRevealModule)];

        firstClickSource = new TaskCompletionSource<bool>();
        lastClickSource = new TaskCompletionSource<bool>();
        finshThevfx0010 = new TaskCompletionSource<bool>();
        finshThevfx0020 = new TaskCompletionSource<bool>();
        finshThevfx0030 = new TaskCompletionSource<bool>();
    }

    public void InitModule<T>()
    {
        if (_currentModule is not IGachaEnterModule initmodule)
            throw new OperationCanceledException("지금 접근한 모듈은 진입점이 존재하지 않는 모듈입니다.");

        initmodule.Init();
        initmodule.Enter();
    }

    public void PlayModule<T>()
    {
        if (_currentModule is not IGachaPlayModule playmodule)
            throw new OperationCanceledException("지금 접근한 모듈은 실행점이 존재하지 않는 모듈입니다.");

        playmodule.Play();
    }

    public void MarkModule<T>()
    {
        if (_currentModule is not IGachaMarkModule markmodule)
            throw new InvalidOperationException("지금 접근한 모듈은 표시점이 존재하지 않는 모듈입니다.");

        markmodule.Mark(currentCharacterData);
    }

    public void ChangeModule<T> ()
    {
        if(!_moduleDict.TryGetValue(typeof(T), out var nextmodule))
            throw new OperationCanceledException("지금 접근한 모듈은 현제 등록되어 있지 않는 모듈입니다.");

        if ( nextmodule is not IGachaEnterModule entermodule)
            throw new OperationCanceledException("지금 접근한 모듈은 진입점이 존재하지 않는 모듈입니다.");

        if (ReferenceEquals(_currentModule, nextmodule))
            return;

        if(_currentModule is not null)
        {
            if(_currentModule is not IGachaExitModule exitmodule)
                throw new OperationCanceledException("지금 접근한 모듈은 종료점이 존재하지 않는 모듈입니다.");

            exitmodule.Exit();
        }

        _currentModule = nextmodule;

        entermodule.Init();
        entermodule.Enter();
    }

    async Task ShowArchiveAsync()
    {
        //-------------------[뽑기 시작]---------------------
        InitModule<GachaArchiveRevealModule>();//모듈에 시작점에 진입

        //파일 이름 초기화하기
        GachaFileTextManger.instance.NameFileTitle();

        //계산기 데이터 초기화 하기
        Gacha_probability_Manger.instance.Init();

        //계산기 뽑기
        currentCharacterData = Gacha_probability_Manger.instance.SelectCharacter();

        //파일 내려가는 연출
        PlayModule<GachaArchiveRevealModule>();//모듈에 실행점에 진입

        ////파일사용자 클릭 대기
        await firstClickSource.Task;

        //Debug.Log("앞축파일 숨기기");
        ////현제 모듈을 압축파일로 교체
        ChangeModule<GachaFileSelectionMoudule>();
    }

    async Task ShowFielAsync()
    {
        //압축파일 init(),Enter()는 ChangeModule<GachaFileSelectionMoudule>();에서 실행할때 실행된다.
        //=> GachaFileSelectionMoudule.init(),GachaFileSelectionMoudule.Enter() 실행
        //떨이지는 vfx0100
        await finshThevfx0010.Task;

        //앞축해제 애니매이션 실행
        PlayModule<GachaFileSelectionMoudule>();

        //압축파일 연출 완료후

        //파일 압축해제 진행
        await Task.Delay(1500);

        //압축파일 연출 종료
        //현제 모듈을 변경
        //현제 모듈은 완전히 종료되었을때
       ChangeModule<GachaResultDisplayMoudule>();
    }

    async Task InteractioFielAsync()
    {
        //클릭을 입력을때까지 대기
        await finshThevfx0030.Task;
        //현제 모듈을 결과표시 모듈로 교체
        ChangeModule<GachaRealResultDisplayMoudule>();
    }

    async Task ShowResultAsync()
    {
        MarkModule<GachaResultDisplayMoudule>();
    }

    public async void ShowGachaAsync()
    {
        await ShowArchiveAsync();
        await ShowFielAsync();
        await InteractioFielAsync();
        await ShowResultAsync();
    }

    public void OnFristClickAsync()
    {
        if (isFristClick) return;
        firstClickSource?.SetResult(true);
        isFristClick = true;
    }

    public void OnEndVfxAsync()
    {
        finshThevfx0010?.SetResult(true);
    }

    public void OnFinshedAsync()
    {
        finshThevfx0030?.SetResult(true);
    }



    public void OnLastClickAsync()
    {
        if (isLastClick) return;
        clickcount++;
        material.SetFloat("_VibrateOffset",clickcount*5);
        if(clickcount == 4)
        {
            clickcount = 0;
            cutfilevfxvisual.SetActive(true);
            vfx0040.PlayFeedbacks();
            isLastClick = true;
            material.SetFloat("_VibrateOffset", 0);
        }

    }
}
