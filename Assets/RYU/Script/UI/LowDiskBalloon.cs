using RYU.Script.Tower;
using UnityEngine;

namespace RYU.Script.UI
{
    //컴퓨터의 바이러스 사용량이 기준을 넘으면 XP 알림 영역에 '디스크 공간 부족' 풍선을 띄운다
    //기준 아래로 내려갔다가 다시 넘으면 또 띄운다. 항상 켜져 있는 오브젝트(알림 영역)에 붙인다
    public class LowDiskBalloon : MonoBehaviour
    {
        [SerializeField] private TowerHealth tower;
        [SerializeField] private GameObject balloon;
        [SerializeField, Range(0f, 1f)] private float warnRatio = 0.7f;
        [SerializeField] private float showTime = 6f;

        private bool _warned;
        private float _hideTime;

        private void Update()
        {
            bool inBattle = tower.isActiveAndEnabled;
            bool low = inBattle && tower.VirusRatio >= warnRatio;

            if (low && !_warned)
            {
                balloon.SetActive(true);
                _hideTime = Time.unscaledTime + showTime;
            }
            _warned = low;

            if (balloon.activeSelf && (!inBattle || Time.unscaledTime >= _hideTime))
                balloon.SetActive(false);
        }

        //풍선의 닫기(X) 버튼 onClick
        public void Close()
        {
            balloon.SetActive(false);
        }
    }
}
