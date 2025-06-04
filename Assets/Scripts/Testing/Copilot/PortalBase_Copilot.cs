using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PortalBase_Copilot : MonoBehaviour, IInteractable_Copilot
{
    [Tooltip("Name of the scene to load when the player interacts with the portal.")]
    public string sceneToLoad = "NextScene";

    [Tooltip("Key the player must press to activate the portal when in range.")]
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    // Optional: display an on-screen prompt.
    private void OnGUI()
    {
        if (playerInRange)
        {
            GUI.Label(new Rect(250, 100, 500, 100), "Press " + interactKey + " to activate the portal...");
        }
    }

    // When the player enters the portal's trigger area.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // When the player leaves the trigger area.
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void Interact(GameObject interactor)
    {
        // Instead of directly loading the scene, delegate to the loading screen manager.
        LoadingScreenManager_Copilot.Instance.LoadSceneWithLoadingScreen(sceneToLoad);
    }
}
