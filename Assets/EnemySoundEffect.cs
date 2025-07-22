using UnityEngine;

public class EnemySoundEffect : MonoBehaviour
{
    private AudioSource audioSource; 
    public AudioClip slashSound; 

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

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