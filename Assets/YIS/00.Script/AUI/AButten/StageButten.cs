using UnityEngine;

public class StageButten : MonoBehaviour
{
    [SerializeField] private GameObject stage1;

    public void StageOneButten()
    {
        bool isIn = false;
        transform.parent.gameObject.SetActive(isIn);
        stage1.gameObject.SetActive(!isIn);
    }
}
