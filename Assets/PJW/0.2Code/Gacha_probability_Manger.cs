using JetBrains.Annotations;
using RYU.Script.Charcter;
using UnityEngine;

public class Gacha_probability_Manger : MonoBehaviour
{
    [field:SerializeField]public CharacterDataSoList CharacterData {  get;private set; }

    public void SelectCharacter()
    {
        int num = Random.Range(1, 101);



    }
}
