using RYU.Script.Charcter;
using UnityEngine;

public class PowerUpManger : MonoBehaviour
{
    [SerializeField]private CharacterDataSoList CharacterDataSoList;

    public int CharacterNumber { get; private set; }

    public void LevelPowerUp()
    {
        CharacterDataSO charater = CharacterDataSoList.charcterDatas[CharacterNumber];
        PowerUPCalc.PowerUp(ref charater.level,ref charater.maxHp,ref charater.attackDmg);
    }

    public void ChageNumberCharacter(int number)
    {
        CharacterNumber = number;
    }
}
