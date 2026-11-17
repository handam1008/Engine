using Unity.VisualScripting;
using UnityEngine;

public class ThrowBall : AbstractAttackSO 
{
    [SerializeField] private GameObject bigBall;
    public override void Attacking(AbstractUnit unit)
    {
        CreateThrowBall();
    }

    private void CreateThrowBall()
    {
        
    }
}
