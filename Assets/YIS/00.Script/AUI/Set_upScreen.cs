using UnityEngine;

public class Set_upScreen : MonoBehaviour
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
