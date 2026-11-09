using System;
using DevLib.ServiceLocator;
using RYU.Script.Charcter;
using RYU.Script.Serivce;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class CharacterSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI storageText;
        [SerializeField] private Image image;
        private CharacterDataSO _data;
        private Button _createBtn;


        private void Awake()
        {
            _createBtn = GetComponent<Button>();
        }

        private void Start()
        {
            _createBtn.onClick.AddListener(() => ServiceLocator.Get<ISpawnService>().TrySpawn(_data));
        }


        public void SetCharcterSlot(CharacterDataSO data)
        {
            _data = data;
            nameText.text = data.Name;
            image.sprite = data.sprite;
            Debug.Log(data.name + " " + data.sprite);
            storageText.SetText("생산 저장공간 " + data.spawnStorage);
            image.GetComponent<Animator>().runtimeAnimatorController = data.animatorController;
        }
    }
}