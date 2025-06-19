using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseManager_Copilot : MonoBehaviour
{
    public bool isPaused = false;
    private GameObject pauseCanvas;

    // Appearance settings for the pause menu.
    private Color panelColor = new Color(0, 0, 0, 0.8f); // 80% opaque black
    private string pausedTitle = "Paused";
    private string resumeText = "Resume";

    PlayerController_Copilot playerController;

    void Start()
    {
        CreatePauseMenu();
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        // Hide the cursor and lock it during gameplay.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Toggle pause when the Escape key is pressed.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (playerController == null)
        {
            GameObject aux = GameObject.FindGameObjectWithTag("Player");
            playerController = aux.GetComponent<PlayerController_Copilot>();
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseCanvas.SetActive(true);

            playerController.lockCameraRotation = true;

            // Show and unlock the cursor when paused.
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Ensure an EventSystem is present.
            if (EventSystem.current == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }
        else
        {
            Time.timeScale = 1f;
            pauseCanvas.SetActive(false);

            playerController.lockCameraRotation = false;

            // Hide and lock the cursor when the game is resumed.
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

        }
    }

    /// <summary>
    /// Creates a basic pause menu UI using code only.
    /// </summary>
    private void CreatePauseMenu()
    {
        // Create a new Canvas.
        /*pauseCanvas = new GameObject("PauseCanvas");
        Canvas canvas = pauseCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.AddComponent<CanvasScaler>();
        pauseCanvas.AddComponent<GraphicRaycaster>();

        // Create a full-screen panel.
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(pauseCanvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = panelColor;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create a "Paused" title.
        GameObject titleObj = new GameObject("PausedText");
        titleObj.transform.SetParent(panel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = pausedTitle;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 48;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(300, 100);

        // Create a Resume button.
        GameObject buttonObj = new GameObject("ResumeButton");
        buttonObj.transform.SetParent(panel.transform, false);
        Button resumeButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.white;
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(160, 40);

        // Add the button label.
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = resumeText;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.black;
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        // Set the Resume button to call TogglePause() when clicked.
        resumeButton.onClick.AddListener(() => { TogglePause(); });*/

        pauseCanvas = GameObject.FindGameObjectWithTag("PauseMenu");
    }
}