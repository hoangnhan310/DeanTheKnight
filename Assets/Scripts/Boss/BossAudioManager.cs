using UnityEngine;

public class BossAudioManager : MonoBehaviour
{
    public static BossAudioManager Instance { get; private set; }
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayBossMusic()
    {
        if (audioSource != null && bossMusic != null)
        {
            audioSource.clip = bossMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource or Boss Music clip is not assigned.");
        }
    }

    public void StopBossMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
