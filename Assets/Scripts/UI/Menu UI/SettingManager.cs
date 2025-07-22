using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class SettingManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public string exposedParamName = "volume"; 
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        // For duplicate
        if (FindObjectsOfType<AudioManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        //Volume
        if (PlayerPrefs.HasKey("Volume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("Volume");
            SetVolume(savedVolume);

            if (volumeSlider != null)
            {
                volumeSlider.value = savedVolume;
            }
        }
        //setting
        if (PlayerPrefs.HasKey("QualitySetting"))
        {
            int savedQuality = PlayerPrefs.GetInt("QualitySetting");
            QualitySettings.SetQualityLevel(savedQuality);

            // Set dropdown
            if (qualityDropdown != null)
            {
                qualityDropdown.value = savedQuality;
                qualityDropdown.RefreshShownValue(); 
            }
        }
    }
    //setting volume
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);

        PlayerPrefs.SetFloat("Volume", volume);
    }
    //setting quality
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualitySetting", qualityIndex);
    }
}
