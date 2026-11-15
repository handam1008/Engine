using UnityEngine;

public class StageButten : MonoBehaviour
{
    [SerializeField] private GameObject stage; // 맵마다 열 스테이지

    public void StageOneButten()
    {
        if (stage == null) return;

        transform.parent.gameObject.SetActive(false);
        stage.SetActive(true);
    }


}
    