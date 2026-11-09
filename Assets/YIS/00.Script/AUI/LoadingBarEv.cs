using System.Threading;
using UnityEngine;

public class LoadingBarEv : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
  

    public void LoadingState()
    {
        transform.root.gameObject.SetActive(false);
        Thread.Sleep(1000);
        canvas.gameObject.SetActive(true);
    }
}
