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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (comfirmationUI.activeSelf)
            {
                // Nếu đang mở confirmation, ESC sẽ tắt nó
                comfirmationUI.SetActive(false);
            }
            else
            {
                // Nếu không mở confirmation, ESC sẽ bật/tắt pause menu
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
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
        Time.timeScale = 1f; // Đảm bảo trở lại tốc độ bình thường khi đổi scene
        SceneManager.LoadScene("Menu");
    }
}
