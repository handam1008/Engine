using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Uis
{
    [SerializeField] public Image[] spirtes;
    [SerializeField] public TextMeshProUGUI nameText; // 이거는 콤보 이름
    [SerializeField] public TextMeshProUGUI discriptionText; // 이거는 콤보 효과

}

public class UiTestssssdsdsd : MonoBehaviour
{
    public List<Uis> SOUiList = new List<Uis>();
    [SerializeField] private SupportEffect effet;


    
    private void Start()
    {
        for(int i = 0; i < effet.effects.Count; ++i)
        {
            var effect = effet.effects[i];
            try
            {
            SOUiList[i].nameText.SetText(effect.name);
            SOUiList[i].discriptionText.SetText(effect.supportDiscription);
            }
            catch(ArgumentException)
            {
                Debug.LogError("SO의 배열과 이 배열의 길이가 일치하지 않으니 재점검 ㄱㄱ");
                return;
            }
            
          
            for(int j = 0; j < effect.characters.Length; ++j)
            {
                SOUiList[i].spirtes[j].sprite = effect.characters[j].sprite;
            }
        }
    }
}
