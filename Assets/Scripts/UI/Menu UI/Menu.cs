using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("ScenceLevel2"); // Đảm bảo đã add vào Build Settings
    }
}
