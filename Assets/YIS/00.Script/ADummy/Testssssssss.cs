using RYU.Script.Charcter;
using UnityEngine;

public class Testssssssss : MonoBehaviour
{
    [SerializeField] private SupportEffect effect;

    private void OnEnable()
    {
        effect.TrySupport();
    }


}
