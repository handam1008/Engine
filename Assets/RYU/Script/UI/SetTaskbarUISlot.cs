using System;
using RYU.Script.Charcter;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.UI
{
    public class SetTaskbarUISlot : MonoBehaviour
    {
        [SerializeField] private CharacterDataSoList charcters;
        [SerializeField] private GameObject icon;

        private void Start()
        {
            SetUI();
        }

        public void SetUI()
        {
            RemoveUI();
            foreach (var charcter in charcters.currentCharacter)
            {
               var obj = Instantiate(icon, transform);
                obj.GetComponent<Image>().sprite = charcter.sprite;
                obj.GetComponent<Animator>().runtimeAnimatorController = charcter.animatorController;
            }
        }

        public void RemoveUI()
        {
            if (transform.childCount == 0) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}