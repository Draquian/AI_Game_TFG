using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Importing TextMeshPro namespace

public class MainMenu_Copilot : MonoBehaviour
{
    // Audio Settings UI elements
    [Header("Audio Settings")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    // Video Settings UI elements
    [Header("Video Settings")]
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown; // Changed to TMP_Dropdown
    public TMP_Dropdown graphicsDropdown;   // Changed to TMP_Dropdown

    // Gameplay Settings UI elements
    [Header("Gameplay Settings")]
    public Slider uiScaleSlider;
    public Slider mouseSensitivitySlider;
    
    // Resolutions array for video settings
    private Resolution[] resolutions;

    // Start is called before the first frame update
    private void Start()
    {
        // Set initial video settings (populate resolution options)
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions(); // TMP_Dropdown supports ClearOptions()
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Optionally initialize the UI elements with current game state values
        fullscreenToggle.isOn = Screen.fullScreen;
        masterSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetMasterVolume() : 1f;
        musicSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetMusicVolume() : 1f;
        sfxSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetSfxVolume() : 1f;
        uiScaleSlider.value = UIManager.Instance != null ? UIManager.Instance.GetScale() : 1f;
        //mouseSensitivitySlider.value = PlayerController.Instance != null ? PlayerController.Instance.GetMouseSensitivity() : 1f;
    }

    // Called when "New Game" is triggered
    public void NewGame()
    {
        // Reset save file (must be implemented in your SaveSystem, for example using PlayerPrefs or file I/O)
        SaveSystem_Copilot.DeleteSaveFile();

        // Load the new game scene
        SceneManager.LoadScene("GameScene"); // Replace "GameScene" with your actual scene name
    }

    // Called when "Continue" is triggered
    public void ContinueGame()
    {
        // Check if a save file exists
        if (SaveSystem_Copilot.SaveFileExists())
        {
            // Load saved game data (this can include the last scene name, player stats, etc.)
            SaveData_Copilot data = SaveSystem_Copilot.Load();

            // Here, you might want to set up the game scene with the loaded save data.
            // For example, changing player position, stat values, etc.
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            Debug.Log("No save file found. Starting a new game...");
            NewGame();
        }
    }

    // Exit function - Closes the game
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    // Audio settings management functions

    public void SetMasterVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxVolume(volume);
    }

    // Video settings management functions

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= resolutions.Length)
            return;

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    // Gameplay settings management functions

    public void SetUIScale(float uiScale)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.SetScale(uiScale);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        if (PlayerController_Copilot.Instance != null)
            PlayerController_Copilot.Instance.SetCameraSensitivity(sensitivity);
    }
}
