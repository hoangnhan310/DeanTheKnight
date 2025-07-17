using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject comfirmationUI;

    private bool isPaused = false;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
        comfirmationUI.SetActive(false);
    }

    public GameObject stageClearedCanvas;
    public GameObject upgradeCanvas;

    void Update()
    {
        // Nếu một trong hai canvas đang bật, vô hiệu phím ESC
        if ((stageClearedCanvas != null && stageClearedCanvas.activeSelf) ||
            (upgradeCanvas != null && upgradeCanvas.activeSelf))
        {
            return; // 🚫 Chặn ESC
        }

        // ESC hoạt động bình thường
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuUI.activeSelf || comfirmationUI.activeSelf)
            {
                // Nếu pause menu hoặc confirmation đang mở → ESC bị vô hiệu
                return;
            }

            // Nếu chưa mở gì → ESC dùng để mở pause
            Pause();
        }
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ToTitle()
    {
        comfirmationUI.SetActive(true);
    }

    public void No()
    {
        comfirmationUI.SetActive(false);
    }

    public void Yes()
    {
        // Lưu tên scene hiện tại trước khi thoát
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", currentSceneName);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    private void OnDestroy()
    {
        Time.timeScale = 1f; // reset an toàn khi scene unload
    }
}
