using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public Button continueButton;
    public GameObject settingPanel;
    private void Start()
    {
        string lastScene = PlayerPrefs.GetString("LastScene", "");
        continueButton.interactable = !string.IsNullOrEmpty(lastScene);
    }

    public void NewGame()
    {
        // Save setting
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.5f);
        int savedQuality = PlayerPrefs.GetInt("QualitySetting", 1);

        PlayerPrefs.DeleteAll();

        // Load setting
        PlayerPrefs.SetFloat("Volume", savedVolume);
        PlayerPrefs.SetInt("QualitySetting", savedQuality);
        PlayerPrefs.Save();

        SceneManager.LoadScene("ScenceLevel2");
    }

    public void ContinueGame()
    {
        string lastScene = PlayerPrefs.GetString("LastScene", "");
        if (!string.IsNullOrEmpty(lastScene))
        {
            SceneManager.LoadScene(lastScene);
        }
    }
    public void openSetting()
    {
        mainMenuPanel.SetActive(false);
        settingPanel.SetActive(true);
    }
    public void backToMenu()
    {
        settingPanel.SetActive(false);
        mainMenuPanel.SetActive(true );
    }
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
