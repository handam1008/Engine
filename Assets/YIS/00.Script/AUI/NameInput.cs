using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameInput : MonoBehaviour
{
   [SerializeField] private TMP_InputField nameField;
    string userName; 

    private void Awake()
    {
        userName = nameField.GetComponent<TMP_InputField>().text;
        nameField.onSubmit.AddListener(_ => OnInputEnd());
    }

   public void OnInputEnd()
    {
        userName = nameField.text;
        PlayerPrefs.SetString("PlayerName", userName);
        SceneChange();
    }

    private void SceneChange()
    {
        SceneManager.LoadScene("MainScene");
    }
}
