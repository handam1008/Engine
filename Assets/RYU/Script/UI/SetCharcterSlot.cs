using System.Linq;
using RYU.Script.Charcter;
using UnityEngine;

namespace RYU.Script.UI
{
    public class SetCharcterSlot : MonoBehaviour
    {
        [SerializeField] private CharacterDataSoList charcters;
        [SerializeField] private GameObject characterSlot;

        private void OnEnable()
        {
            SetCurrentCharacter();
        }

        public void SetHaveCharacter()
        {
            RemoveAllCharcters();
            foreach (var data in charcters.charcterDatas)
            {
                GameObject slot = Instantiate(characterSlot, transform);
                slot.GetComponent<CharacterSlot>().SetCharcterSlot(data, canSpawn: false);
            }
        }
        
        public void SetCurrentCharacter()
        {
            RemoveAllCharcters();
            foreach (var data in charcters.currentCharacter)
            {
                GameObject slot = Instantiate(characterSlot, transform);
                slot.GetComponent<CharacterSlot>().SetCharcterSlot(data, canSpawn: true);
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
