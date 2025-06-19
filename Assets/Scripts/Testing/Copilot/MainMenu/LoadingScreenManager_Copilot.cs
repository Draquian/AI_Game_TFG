using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LoadingScreenManager_Copilot : MonoBehaviour
{
    public static LoadingScreenManager_Copilot Instance;

    [Tooltip("UI GameObject that represents the loading screen panel.")]
    public GameObject loadingScreen;
    public GameObject loadingprogresion;

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
        float minLoadingTime = 5f;
        float startTime = Time.time;

        // Begin asynchronous scene loading.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        //Image imageColor = loadingprogresion.GetComponent<Image>();
        while (!asyncLoad.isDone)
        {
            // (Optional) Update progress UI if needed.
            //imageColor.color = new Color(1,1,1, asyncLoad.progress);

            if (asyncLoad.progress >= 0.9f)
            {
                // Calculate elapsed time
                float elapsed = Time.time - startTime;
                if (elapsed < minLoadingTime)
                {
                    // Wait the remaining time so the loading screen shows for at least 5 seconds.
                    yield return new WaitForSeconds(minLoadingTime - elapsed);
                }
                // Allow the scene to activate.
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }
}
