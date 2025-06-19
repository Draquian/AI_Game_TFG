using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] public AudioMixer masterMixer;

    // Local cached volume values (default value is 1 which represents 100%)
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    private void Awake()
    {
        // Singleton pattern ensures only one instance exists.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Getters
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSfxVolume() => sfxVolume;

    // Setters update both cache and AudioMixer values (if available)
    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        if (masterMixer != null)
        {
            // Convert volume to decibels
            masterMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (masterMixer != null)
        {
            masterMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        if (masterMixer != null)
        {
            masterMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
        }
    }
}
