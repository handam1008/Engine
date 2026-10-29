using RYU.Script.Charcter;
using System.Linq;
using UnityEngine;
public enum SupportType {HpUp, Speed, AttackUp }

[CreateAssetMenu(fileName = "SupportEffect", menuName = " Support/SupportEffect")]
public class SupportEffect : ScriptableObject
{
    public CharcterDataSo[] SupportableUnit;
    public CharacterDataSoList currentDeck;
    //public SupportType stype;

    private CharcterDataSo[] Intersetion;

    private void Awake()
    {
        TrySupport(SupportType.Speed);
    }

    public void TrySupport(SupportType type)
    {
        // 대충 이제 서포터블 유닛 내의 유닛과 내 덱의 유닛들 리스트 비교해서 나온 공통 분모에게 지원 효과 부여   
        Intersetion = SupportableUnit.Intersect(currentDeck.currentCharacter).ToArray();
        Debug.Log("Try");
        foreach(var unit in Intersetion)
        {
            Debug.Log($"{type} {Intersetion.Length}명 만큼 지원");
        }
        if(Intersetion.Length != 0)
        {
            GetSupport(type);
        }
    }

    public void GetSupport(SupportType type)
    {
        Debug.Log("TryGetSupport");
        if (Intersetion.Length == 0) return; // 지원관계유닛이 없으면 return;

        Debug.Log($"{type}의 효과");

    }
}
