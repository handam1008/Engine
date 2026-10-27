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


     


        public void SetCharacter()
        {
            RemoveAllCharcters();
            foreach (var data in charcters.charcterDatas)
            {
                GameObject slot = Instantiate(characterSlot, transform);
                slot.GetComponent<CharacterSlot>().SetCharcterSlot(data.Name, data.sprite);
            }
        }

        public void AddCharcter(CharacterDataSO character)
        {
            charcters.charcterDatas.Append(character);
        }
        
        public void RemoveAllCharcters()
        {
            if (transform.childCount == 0) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
