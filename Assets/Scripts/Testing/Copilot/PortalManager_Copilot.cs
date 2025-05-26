using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PortalManager_Copilot : MonoBehaviour, IInteractable_Copilot
{
    [Header("Portal Settings")]
    public float basePortalOpenTime = 10f; // Base time before closing (in seconds)
    public int mazeLevel = 1; // Current level (affects closing time)
    public GameObject portalObject; // The actual portal GameObject
    public GameObject interactionMenu; // UI Menu with 20 buttons
    public MazeGenerator_Copilot mazeGenerator; // Reference to MazeGenerator to access spawnableSprites
    public PauseManager_Copilot pauseManager; // Reference to MazeGenerator to access spawnableSprites
    public List<Button> selectionButtons = new List<Button>(); // Buttons inside the menu
    PlayerController_Copilot playerController;

    public List<Sprite> selectedSprites = new List<Sprite>(); // Sprites player has selected
    public bool isPortalOpen = false;
    public float portalTimer;

    // Called when the player interacts with this object.
    public void Interact(GameObject interactor)
    {
        InteractWithPortal();
    }

    private void Start()
    {
        interactionMenu.SetActive(false);
        //portalObject.SetActive(false);
        portalTimer = basePortalOpenTime + (mazeLevel - 1) * 10f; // Increase by 10s per level
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ConfirmSelection();
        }

            if (isPortalOpen && !pauseManager.isPaused)
        {
            portalTimer -= Time.deltaTime;
            if (portalTimer <= 0)
            {
                ClosePortal();
            }
        }
    }

    /*public void SetGamePause(bool paused)
    {
        isGamePaused = paused;
    }*/

    public void OpenPortal()
    {
        isPortalOpen = true;
        //portalObject.SetActive(true);
        portalTimer = basePortalOpenTime + (mazeLevel - 1) * 10f;
    }

    private void ClosePortal()
    {
        isPortalOpen = false;
        //portalObject.SetActive(false);
    }

    public void InteractWithPortal()
    {
        if (playerController == null)
        {
            GameObject aux = GameObject.FindGameObjectWithTag("Player");
            playerController = aux.GetComponent<PlayerController_Copilot>();
        }

        Time.timeScale = 0f;

        playerController.lockCameraRotation = true;

        // Show and unlock the cursor when paused.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (isPortalOpen)
        {
            GoToNextLevel();
        }
        else
        {
            ShowInteractionMenu();
        }
    }

    private void ShowInteractionMenu()
    {
        interactionMenu.SetActive(true);
        selectedSprites.Clear(); // Clear previous selections
        ResetButtonColors();
    }

    // This function is meant to be chained to each button's OnClick event.
    // Pass in the button and its associated sprite.
    public void OnIconButtonClicked(GameObject button)
    {
        Sprite iconSprite = button.GetComponent<Image>().sprite;

        // Mark the clicked button as selected by setting its color to white.
        //clickedButton.image.color = Color.white;

        // Add the sprite if it isn't already selected.
        if (!selectedSprites.Contains(iconSprite) && selectedSprites.Count < 5)
        {
            selectedSprites.Add(iconSprite);
            selectionButtons.Add(button.GetComponent<Button>());
            button.GetComponent<Image>().color = Color.white;

            if (selectedSprites.Count >= 5) ConfirmSelection();
        }
        else
        {
            selectedSprites.Remove(iconSprite);
            selectionButtons.Add(button.GetComponent<Button>());
            button.GetComponent<Image>().color = Color.black;
        }
    }

    // Resets all selection button colors to a dark color.
    private void ResetButtonColors()
    {
        foreach (Button btn in selectionButtons)
        {
            if (btn != null && btn.image != null)
            {
                btn.image.color = Color.black;  // Dark color for unselected.
            }
        }
    }

    // Called from the menu's confirm button.
    public void ConfirmSelection()
    {
        // Compare the selected sprites against the MazeGenerator's spawnableSprites list.
        if (AreSelectionsCorrect())
        {
            // Selection is valid: resume game and open the portal.
            pauseManager.isPaused = false;
            OpenPortal();
            interactionMenu.SetActive(false);

            Time.timeScale = 1f;

            playerController.lockCameraRotation = false;
            
            // Hide and lock the cursor when the game is resumed.
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // Wrong selection: clear previous selections and reset button colors.
            selectedSprites.Clear();
            ResetButtonColors();
        }
    }

    private bool AreSelectionsCorrect()
    {
        if (selectedSprites.Count != mazeGenerator.spawnedSprites.Count) return false;

        foreach (Sprite sprite in mazeGenerator.spawnedSprites)
        {
            if (!selectedSprites.Contains(sprite))
            {
                return false; // Not all required sprites were selected
            }
        }
        return true;
    }

    private void GoToNextLevel()
    {
        Debug.Log("Player moves to the next level!");
        mazeGenerator.nextLevel();
        pauseManager.isPaused = false;

    }
}
