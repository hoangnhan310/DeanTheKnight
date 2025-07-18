using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource; // Tham chiếu đến AudioSource
    public AudioClip slashSound; // Kéo file âm thanh chém vào đây
    void Start()
    {
        // Lấy thành phần AudioSource gắn với GameObject
        audioSource = GetComponent<AudioSource>();

        // Đảm bảo âm thanh phát khi game bắt đầu
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource hoặc AudioClip không được gán!");
        }
    }

    void Update()
    {
        // Có thể thêm logic điều khiển âm thanh ở đây (tạm thời để trống)
    }


    public void PlaySlashSound()
    {
        if (audioSource != null && slashSound != null)
        {
            audioSource.PlayOneShot(slashSound); // Phát âm thanh chém
        }
        else
        {
            Debug.LogWarning("AudioSource hoặc slashSound không được gán!");
        }
    }
}