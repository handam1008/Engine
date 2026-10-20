using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInput : MonoBehaviour
{
   [SerializeField] private TMP_InputField nameField;
    string userName; 

    private void Awake()
    {
        userName = nameField.GetComponent<TMP_InputField>().text; 
    }

   public void OnInputEnd()
    {
        userName = nameField.text;
        PlayerPrefs.SetString("PlayerName", userName);
    }
}
