using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource; 
    public AudioClip slashSound; 
    public AudioMixer audioMixer;
    void Start()
    {
       audioSource = GetComponent<AudioSource>();
        if (PlayerPrefs.HasKey("volume"))
        {
            float volume = PlayerPrefs.GetFloat("volume");
            audioMixer.SetFloat("volume", volume);
        }
        if (audioSource != null && audioSource.clip != null)
    {
        audioSource.loop = true; 
        audioSource.Play();
    }
    else
    {
        Debug.LogWarning("AudioSource hoặc AudioClip không được gán!");
    }
    }

    void Update()
    {
    }


    public void PlaySlashSound()
    {
        if (audioSource != null && slashSound != null)
        {
            audioSource.PlayOneShot(slashSound); // đòn tán công
        }
        else
        {
            Debug.LogWarning("AudioSource hoặc slashSound không được gán!");
        }
    }
}