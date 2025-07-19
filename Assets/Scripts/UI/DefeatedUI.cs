using UnityEngine;
using UnityEngine.SceneManagement;

public class DefeatedUI : MonoBehaviour
{
    private void OnEnableDefeated()
    {
        Time.timeScale = 0f;
    }

    private void OnDisableDefeated()
    {
        Time.timeScale = 1f;
    }

    private void UpdateDefeated()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }
    }

    public void ToMenuDefeated()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", currentSceneName);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void RestartDefeated()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
