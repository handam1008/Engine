using UnityEngine;
using UnityEngine.UI;

public class ButtenUIClick : MonoBehaviour
{
    [SerializeField] private GameObject[] xpLogo;
    public void OnWelcomeBtnClick()
    {
        gameObject.SetActive(false);
        for (int i = 0; i < xpLogo.Length; ++i)
        {
            if (xpLogo[i] == null) continue;
            xpLogo[i].gameObject.SetActive(true);
        }
    }
}
