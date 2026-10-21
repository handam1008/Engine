using System.Threading;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DefeatScreen : MonoBehaviour
{
    AudioSource _as;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _as.Play();
    }
}
