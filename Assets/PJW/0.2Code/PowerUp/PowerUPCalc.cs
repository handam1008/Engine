using UnityEngine;

public static class PowerUPCalc
{
    public static void PowerUp(ref int currentLevel,ref int currentMaxHp,ref int currentDmg)
    {
        currentLevel++;
        currentMaxHp += 10;
        currentDmg += 10;
    }
}
