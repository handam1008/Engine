using Assets.PJW._0._4Clean_Up_Code;
using MoreMountains.Feedbacks;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GachaFileSelectionMoudule : MonoBehaviour, IGachaEnterModule, IGachaPlayModule, IGachaExitModule
{
    [SerializeField] private GameObject[] _unzipPercentPrefabs = new GameObject[43];

    [SerializeField] private GameObject _unzipWindow;

    [SerializeField] private MMF_Player _vfx0010;

    [SerializeField] private MMF_Player _vfx0020;

    [SerializeField] private TextMeshProUGUI _timeText;

    [SerializeField] private TextMeshProUGUI _fileNameText;

    [SerializeField] private string[] _passingFileNames;

    [SerializeField] private string _finalFileName = "???.exe";

    public async void Init()
    {
        _fileNameText.text = _passingFileNames[0];
        _unzipWindow.SetActive(true);
        foreach (var item in _unzipPercentPrefabs)
        {
            item.SetActive(false);
        }

        _vfx0010.Initialization();
        _vfx0020.Initialization();
    }

    public async void Enter()
    {
        await Task.Delay(500);
        _vfx0010.PlayFeedbacks();
        _vfx0020.PlayFeedbacks();
    }

    public async void Play()
    {
        float timer = 0;

        for (int i = 0; i < _unzipPercentPrefabs.Length; i++)
        {
            var item = _unzipPercentPrefabs[i];
            await Task.Delay(34);
            item.SetActive(true);
            _fileNameText.text = i == _unzipPercentPrefabs.Length - 1
                ? _finalFileName
                : _passingFileNames[i * _passingFileNames.Length / _unzipPercentPrefabs.Length];
            timer += 0.233f;
            _timeText.text = $"남은 시간: {10-(int)timer}초";
        }
    }

    public void Exit()
    {
        _unzipWindow.SetActive(false);
    }
}
