using UnityEngine;

[CreateAssetMenu(fileName = "BudyBudySO", menuName = "SO/Attack/BudyBudy", order = 0)]
public class BudyBudyAttackSO : AbstractAttackSO
{
    [SerializeField] private BudyBullet Letter;
    [SerializeField] private BudybudyCharacter _bChar;

    public override void Attacking(AbstractUnit unit)
    {
        //_bChar._supportUnit.TryGetComponent<AbstractUnit>(out AbstractUnit support);

        // 유닛의 타겟을 나의 타겟으로 삼아서 공격 ㄱㄱ
        BudyBullet bule = Instantiate(Letter, unit.transform.position, Quaternion.identity);
        bule.transform.position = new Vector3(unit.transform.position.x + 0.8f, unit.transform.position.y, unit.transform.position.z);
        bule.Init(unit.UnitSo.attackDmg);
        bule.GetReadyToFire += _ => bule.Shoot(unit.target.transform);

        Debug.Log("아롤라까딸롤라 우라ㅏㅏㅏㅏㅏㅏ어택");

    }
}
