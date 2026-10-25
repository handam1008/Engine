using UnityEngine;

namespace RYU.Script.Charcter
{
    [CreateAssetMenu(fileName = "SO", menuName = "RYU/CharacterList", order = 0)]
    public class CharacterDataSoList : ScriptableObject
    {
        public CharacterDataSO[] charcterDatas; 
        
        public CharacterDataSO[] currentCharacter; 
    }
}