using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LoadingScreenManager_Copilot : MonoBehaviour
{
    public static LoadingScreenManager_Copilot Instance;

    [Tooltip("UI GameObject that represents the loading screen panel.")]
    public GameObject loadingScreen;

    // (Optional) Add UI elements such as a progress bar:
    // public Slider progressBar;

    private void Awake()
    {
        // Set up the singleton instance to persist across scenes.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this function to load a scene asynchronously with a loading screen.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Begin asynchronous scene loading.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            // (Optional) Update the progress bar based on asyncLoad.progress.
            // Example: progressBar.value = asyncLoad.progress;

            // When progress is near 0.9, the scene is almost ready.
            if (asyncLoad.progress >= 0.9f)
            {
                // Allow the scene to activate.
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // Once scene is loaded, turn off the loading screen.
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }
}
