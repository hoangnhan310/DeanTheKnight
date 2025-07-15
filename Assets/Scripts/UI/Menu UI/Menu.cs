using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private void Start()
    {
        chooseMapPanel.SetActive(false);
    }
    public void NewGame()
    {
        // Xoá toàn bộ dữ liệu đã lưu
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        SceneManager.LoadScene("ScenceLevel2"); 
    }

    public GameObject chooseMapPanel;
    public GameObject mainMenuPanel;

    public void OpenMapSelection()
    {
        chooseMapPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }
    public void BackToMainMenu()
    {
        chooseMapPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit(); // Sẽ hoạt động sau khi build

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Dừng play mode nếu đang chạy trong Editor
#endif
    }
}
