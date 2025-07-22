using UnityEngine;
using UnityEngine.SceneManagement;

public class StageClearedUI : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "SceneLevel1";

    [Header("Canvas Switching")]
    [SerializeField] private GameObject stageClearedCanvas;
    [SerializeField] private GameObject upgradeCanvas;

    public void ToNextMap()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(sceneToLoad);
    }

    public void OpenUpgradeCanvas()
    {
        Time.timeScale = 0f;
        if (stageClearedCanvas != null)
            stageClearedCanvas.SetActive(false);

        if (upgradeCanvas != null)
            upgradeCanvas.SetActive(true);
    }
}
