using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public Button continueButton;

    private void Start()
    {
        string lastScene = PlayerPrefs.GetString("LastScene", "");
        continueButton.interactable = !string.IsNullOrEmpty(lastScene);
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene("ScenceLevel2"); // ← Đặt lại nếu map đầu có tên khác
    }

    public void ContinueGame()
    {
        string lastScene = PlayerPrefs.GetString("LastScene", "");
        if (!string.IsNullOrEmpty(lastScene))
        {
            SceneManager.LoadScene(lastScene);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
