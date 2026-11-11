using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestGameEndTrigger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI victoryText;

    private void Update()
    {
        if(Keyboard.current.pKey.wasPressedThisFrame)
        {
            victoryText.gameObject.SetActive(true);
            victoryText.SetText("WOW you've won!!! 1972Gold given!!!!");
            StartCoroutine(Delay());
        }
    }

    IEnumerator Delay()
    {
        transform.root.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        victoryText.gameObject.SetActive(false);
        transform.root.gameObject.SetActive(false);

    }
}
