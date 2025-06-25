using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class LoadingScreenManager_Copilot : MonoBehaviour
{
    public static LoadingScreenManager_Copilot Instance;

    [Tooltip("UI GameObject that represents the loading screen panel.")]
    public GameObject loadingScreen;
    public GameObject loadingprogresion;

    // (Optional) Add UI elements such as a progress bar:
    // public Slider progressBar;

    // In LoadingScreenManager_Copilot.cs
    /// <summary>
    /// Fired whenever a new scene has finished loading.
    /// </summary>
    public event Action<Scene, LoadSceneMode> OnSceneLoaded;

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

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        const float minDisplayTime = 5f;
        float startTime = Time.time;

        // Kick off the async load but don’t allow activation yet.
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // — Phase 1: Load up to 90% (Unity’s convention for “loaded but not activated”)
        while (op.progress < 0.9f)
        {
            // Update your progress‐bar alpha
            /*float normalized = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingProgressImage != null)
                loadingProgressImage.color = new Color(1, 1, 1, normalized);*/

            yield return null;
        }

        // Enforce a minimum display time
        float elapsed = Time.time - startTime;
        if (elapsed < minDisplayTime)
        {
            yield return new WaitForSeconds(minDisplayTime - elapsed);
        }

        // — Phase 2: Now allow the scene to activate
        op.allowSceneActivation = true;

        // Wait until the operation is fully done (scene has swapped in)
        yield return new WaitUntil(() => op.isDone);

        // Hide the loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // Fire your callback/event now that the scene is truly loaded
        OnSceneLoaded?.Invoke(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // This method is called by Unity whenever a scene is loaded.
    private void SceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        // Hide the loading screen in case it's still active
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // Fire our own event so other scripts can react
        OnSceneLoaded?.Invoke(scene, mode);
    }
}
