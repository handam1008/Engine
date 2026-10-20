using UnityEngine;

namespace RYU.Script.Charcter
{
    [CreateAssetMenu(fileName = "SO", menuName = "RYU/CharacterList", order = 0)]
    public class CharacterDataSoList : ScriptableObject
    {
        public CharcterDataSo[] charcterDatas; 
        
        public CharcterDataSo[] currentCharacter; 
    }
}