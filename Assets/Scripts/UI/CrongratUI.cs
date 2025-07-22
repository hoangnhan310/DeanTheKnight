using System.Collections;
using UnityEngine;

public class CrongratUI : MonoBehaviour
{
    public static CrongratUI Instance;

    [SerializeField] private GameObject congratulationUI;

    private void Awake()
    {
        Instance = this;
    }

    public void TriggerWin()
    {
        if (congratulationUI != null)
        {
            congratulationUI.SetActive(true);
        }

        StartCoroutine(DelayStopGame());
    }

    private IEnumerator DelayStopGame()
    {
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 0f;
    }
}
