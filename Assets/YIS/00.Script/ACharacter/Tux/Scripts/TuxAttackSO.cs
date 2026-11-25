using UnityEngine;

[CreateAssetMenu(fileName = "TuxAttackSO", menuName = "SO/TuxAttackSO")]
public class TuxAttackSO : AbstractAttackSO
{
    [SerializeField] private Surge wave;

    public override void Attacking(AbstractUnit unit)
    {
       
        var w = Instantiate(wave, unit.transform.position, Quaternion.identity);
        w.transform.position = new Vector3(unit.transform.position.x + 0.8f, unit.transform.position.y, unit.transform.position.z);
        w.Init(unit.UnitSo.attackDmg);
        w.GetReadyToFire += _ => w.AfterShoot();
    }
}
