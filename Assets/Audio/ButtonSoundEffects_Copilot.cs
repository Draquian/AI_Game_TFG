using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio; // Needed for AudioMixerGroup

public class ButtonSoundEffects_Copilot : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Tooltip("The sound clip to play when the button is hovered over.")]
    public AudioClip hoverClip;

    [Tooltip("The sound clip to play when the button is clicked.")]
    public AudioClip clickClip;

    [Tooltip("Volume for both the hover and click sounds.")]
    public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Try to get an AudioSource component; add one if it doesn't exist.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            // Set the output of the AudioSource to the SFX group from the AudioManager.
            if (AudioManager.Instance != null && AudioManager.Instance.masterMixer != null)
            {
                audioSource.outputAudioMixerGroup = AudioManager.Instance.masterMixer.FindMatchingGroups("Master")[2];
            }
            else
            {
                Debug.LogWarning("AudioManager instance or its sfxGroup is not set. " +
                                 "SFX sounds may not be routed correctly.");
            }
        }
    }

    // Plays a sound when the pointer hovers over the button.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip != null)
        {
            audioSource.PlayOneShot(hoverClip, volume);
        }
    }

    // Plays a sound when the button is clicked.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickClip != null)
        {
            audioSource.PlayOneShot(clickClip, volume);
        }
    }
}
