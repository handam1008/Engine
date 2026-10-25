using UnityEngine;

namespace RYU.Script
{
    public class TestDummy : MonoBehaviour
    {
        private HealthSystem _healthSystem;
        private void Start()
        {
            HealthSystem health = GetComponent<HealthSystem>();

           

            health.OnDamged += () => Debug.Log("샌드백 피격");
            health.OnDeath += () => Debug.Log("샌드백 사망");
        }
    }
}