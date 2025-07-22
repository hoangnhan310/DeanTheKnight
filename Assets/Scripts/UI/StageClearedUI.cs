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
        SceneManager.LoadScene(sceneToLoad);
        Time.timeScale = 1f;
    }

    public void OpenUpgradeCanvas()
    {
        if (stageClearedCanvas != null)
            stageClearedCanvas.SetActive(false);

        if (upgradeCanvas != null)
            upgradeCanvas.SetActive(true);
    }
}
