using UnityEngine;

    [CreateAssetMenu(fileName = "SO", menuName = "UnitAttack", order = 0)]
    public abstract class AbstractAttackSO : ScriptableObject
    {
        public abstract void Attacking(AbstractUnit unit);
    }
