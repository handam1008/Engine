using System.Collections.Generic;
using KSM._00.Scripts;
using RYU.Script.Stage;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.MacDesktop
{
    
    public class MacDock : MonoBehaviour
    {
        [SerializeField] private StageSO _stage;
        [SerializeField] private RectTransform _slotTemplate;
        [SerializeField] private float _cropSize = 220f; 

        private void Start()
        {
            var shown = new HashSet<EnemySO>();
            foreach (StageData data in _stage.stageDates)
            {
                if (data.enemy == null || !shown.Add(data.enemy)) continue;

                RectTransform slot = Instantiate(_slotTemplate, _slotTemplate.parent);
                Image image = slot.GetComponentInChildren<Image>();
                image.sprite = data.enemy.sprite;
                slot.gameObject.SetActive(true);
            }
        }
    }
}
