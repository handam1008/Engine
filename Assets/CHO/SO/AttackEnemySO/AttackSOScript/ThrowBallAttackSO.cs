using Unity.VisualScripting;
using UnityEngine;
using IPoolable = RYU.Script.PoolManager.IPoolable;

public class ThrowBallAttackSO : AbstractAttackSO ,IPoolable
{
    [SerializeField] private GameObject bigBall;
    public override void Attacking(AbstractUnit unit)
    {
        CreateThrowBall();
    }

    private void CreateThrowBall()
    {
        
    }

    [field: SerializeField] public string ItemName { get; private set; } = "BigBall";

    public GameObject GetGameObject()
    {
        return null;
    }

    public void ResetItem()
    {
        
    }
}
