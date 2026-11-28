using UnityEngine;

[CreateAssetMenu(fileName = "BlanderAttackSO", menuName = "SO/BlanderAttackSO")]
public class BlanderAttackSO : AbstractAttackSO
{
    [SerializeField] private PierceShpere Shapre;

    public override void Attacking(AbstractUnit unit)
    {
        PierceShpere px = Instantiate(Shapre, unit.transform.position, Quaternion.identity);
        px.Init(unit.UnitSo.attackDmg);
        px.GetReadyToFire += _ => px.Shoot();
    }
}
