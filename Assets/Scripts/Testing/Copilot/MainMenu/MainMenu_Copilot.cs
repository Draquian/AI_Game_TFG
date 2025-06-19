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

    public TMP_Text masterValue;
    public TMP_Text musicValue;
    public TMP_Text sfxValue;

    private PauseManager_Copilot pManager;
    private PlayerController_Copilot pControl;
    private Inventory_Copilot pInventory;
    private PlayerStats_Copilot pStats;

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

        if (pControl == null)
            pControl = FindAnyObjectByType<PlayerController_Copilot>();

        if (pInventory == null)
            pInventory = FindAnyObjectByType<Inventory_Copilot>();

        if(pStats ==null)
            pStats = FindAnyObjectByType<PlayerStats_Copilot>();

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Optionally initialize the UI elements with current game state values
        fullscreenToggle.isOn = Screen.fullScreen;
        masterSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetMasterVolume() : 1f;
        musicSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetMusicVolume() : 1f;
        sfxSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetSfxVolume() : 1f;
        uiScaleSlider.value = UIManager.Instance != null ? UIManager.Instance.GetScale() : 1f;
        masterValue.text = AudioManager.Instance != null ? AudioManager.Instance.masterVolume.ToString() : "100";
        musicValue.text = AudioManager.Instance != null ? AudioManager.Instance.musicVolume.ToString() : "100";
        sfxValue.text = AudioManager.Instance != null ? AudioManager.Instance.sfxVolume.ToString() : "100";
        mouseSensitivitySlider.value = pControl != null ? pControl.GetCameraSensitivity() : 1f;
    }

    // Called when "New Game" is triggered
    public void NewGame()
    {
        // Reset save file (must be implemented in your SaveSystem, for example using PlayerPrefs or file I/O)
        SaveSystem_Copilot.DeleteSaveFile();

        SaveSystem_Copilot.EnsureSaveFileExists();

        // Load the new game scene
        LoadingScreenManager_Copilot.Instance.LoadSceneWithLoadingScreen("Base");
        //SceneManager.LoadScene("Base"); // Replace "GameScene" with your actual scene name
    }

    public void MainMenu()
    {
        // Reset save file (must be implemented in your SaveSystem, for example using PlayerPrefs or file I/O)
        SaveManager.Instance.SaveGame(SceneManager.GetActiveScene().name);
        SaveManager.Instance.SaveGame(pControl.transform);
        SaveManager.Instance.SaveGame(pInventory);
        SaveManager.Instance.SaveGame(pStats.magicType);
        Debug.LogError(pStats.magicType);

        // Load the new game scene
        //LoadingScreenManager_Copilot.Instance.LoadSceneWithLoadingScreen("MainMenu");
        SceneManager.LoadScene("MainMenu");
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

    public void pauseButton()
    {
        if(pManager == null)
        {
            pManager = FindAnyObjectByType<PauseManager_Copilot>();
        }

        pManager.TogglePause();
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
        {
            AudioManager.Instance.SetMasterVolume(volume);
            masterValue.text = AudioManager.Instance.masterVolume.ToString("0.01");
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
            musicValue.text = AudioManager.Instance.musicVolume.ToString("0.01");
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(volume);
            sfxValue.text = AudioManager.Instance.sfxVolume.ToString("0.01");
        }
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
            pControl.SetCameraSensitivity(sensitivity);
    }
}
