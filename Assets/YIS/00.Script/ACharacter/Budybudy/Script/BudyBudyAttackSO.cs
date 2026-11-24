using UnityEngine;

[CreateAssetMenu(fileName = "BudyBudySO", menuName = "SO/Attack/BudyBudy", order = 0)]
public class BudyBudyAttackSO : AbstractAttackSO
{
    [SerializeField] private BudyBullet Letter;


    public override void Attacking(AbstractUnit unit)
    {
        BudybudyCharacter budy = unit as BudybudyCharacter;

        if (budy._supportUnit == null) return;
        AbstractUnit supportUnit = budy._supportUnit.GetComponentInParent<AbstractUnit>();


        // À¯´ÖÀÇ Å¸°ÙÀ» ³ªÀÇ Å¸°ÙÀ¸·Î »ï¾Æ¼­ °ø°Ý ¤¡¤¡
        
            BudyBullet bule = Instantiate(Letter, unit.transform.position, Quaternion.identity);
            bule.transform.position = new Vector3(unit.transform.position.x + 0.8f, unit.transform.position.y, unit.transform.position.z);
            bule.Init(unit.UnitSo.attackDmg);
            bule.GetReadyToFire += _ => bule.Shoot(supportUnit.target.transform);
        


    }
}
