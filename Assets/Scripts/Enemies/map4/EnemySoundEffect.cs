using UnityEngine;

public class EnemySoundEffect : MonoBehaviour
{
    private AudioSource audioSource; // AudioSource để phát âm thanh
    public AudioClip slashSound; // Âm thanh chém của enemy

    void Start()
    {
        // Thêm và lấy AudioSource nếu chưa có
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Hàm phát âm thanh chém
    public void PlaySlashSound()
    {
        if (audioSource != null && slashSound != null)
        {
            audioSource.PlayOneShot(slashSound); // Phát âm thanh chém
        }
        else
        {
            Debug.LogWarning("AudioSource hoặc slashSound không được gán trên enemy: " + gameObject.name);
        }
    }
}