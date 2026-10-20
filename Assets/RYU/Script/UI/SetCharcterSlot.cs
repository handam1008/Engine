using System;
using System.Linq;
using RYU.Script.Charcter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class SetCharcterSlot : MonoBehaviour
    {
        [SerializeField] private CharacterDataSoList charcters;
        [SerializeField] private GameObject characterSlot;

        private Transform _parent;

        private void Awake()
        {
            _parent = transform;
        }


        public void SetCharacter()
        {
            RemoveAllCharcters();
            foreach (var data in charcters.charcterDatas)
            {
                GameObject slot = Instantiate(characterSlot, _parent);
                slot.GetComponent<CharacterSlot>().SetCharcterSlot(data.name, data.sprite);
            }
        }

        public void AddCharcter(CharcterDataSo charcter)
        {
            charcters.charcterDatas.Append(charcter);
        }
        
        public void RemoveAllCharcters()
        {
            if (_parent.childCount == 0) return;
            for (int i = 0; i < _parent.childCount; i++)
            {
                Destroy(_parent.GetChild(i).gameObject);
            }
        }
    }
}
