using UnityEngine;

/// <summary>
/// Any object that wants to be interactable must implement this interface.
/// </summary>
public interface IInteractable_Copilot
{
    /// <summary>
    /// Called when a player interacts with the object.
    /// </summary>
    /// <param name="interactor">The GameObject that is initiating the interaction (usually the player).</param>
    void Interact(GameObject interactor);
}