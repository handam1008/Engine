using System.Collections;
using UnityEngine;

public class WelcomeUI : MonoBehaviour
{
    [SerializeField] private GameObject[] xpLogo;
    public void OnEnable()
    {
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);

        gameObject.SetActive(false);
        for (int i = 0; i < xpLogo.Length; ++i)
        {
            if (xpLogo[i] == null) continue;
            xpLogo[i].gameObject.SetActive(true);
        }

    }
}
