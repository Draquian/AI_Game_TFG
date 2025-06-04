using UnityEngine;
using System.Collections.Generic;

public class PreLevelTrigger_Copilot : MonoBehaviour, IInteractable_Copilot
{
    [Header("References")]
    // Reference to your MazeGenerator which has the preLevel() function and (optionally) a list of rooms.
    public MazeGenerator_Copilot mazeGenerator;

    // (Assumption: your MazeGenerator stores rooms in a public List<Room> rooms)

    // Boolean flag to ensure interaction happens only once per event.
    private bool triggered = false;

    // This function is called when the player interacts with this object.
    public void PreLevelInteraction()
    {
        if (mazeGenerator != null)
        {
            Debug.LogError("INteracetd");

            // Call the preLevel() function in MazeGenerator.
            mazeGenerator.preLevel();
        }
        else
        {
            Debug.LogWarning("PreLevelTrigger: MazeGenerator reference is missing!");
        }
    }

    // When the player enters the trigger attached to this object, call PreLevelInteraction().
    /*private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            PreLevelInteraction();
        }
    }*/

    public void Interact(GameObject interactor)
    {
        triggered = true;
        PreLevelInteraction();
    }

    // This function places this object in the same room where the player starts.
    // It adapts the logic from your PlacePlayer() function.
    public void PlaceInStartRoom()
    {
        // Check that MazeGenerator and its room list are assigned.
        if (mazeGenerator == null)
        {
            Debug.LogWarning("PreLevelTrigger: MazeGenerator reference is missing!");
            return;
        }

        // Assuming MazeGenerator stores its rooms in a List<Room> and that the start room is the first one.
        MazeGenerator_Copilot.Room startRoom = mazeGenerator.startRoom;
        //List<Room> rooms = mazeGenerator.rooms;
        //if (rooms != null && rooms.Count > 0)

        if (startRoom != null)
        {
            //Room startRoom = rooms[0];  // Adjust this if you have a designated start room.
            // Place this object at the center of the start room.
            transform.position = new Vector3(startRoom.center.x, 6f, startRoom.center.z);
        }
        else
        {
            Debug.LogWarning("PreLevelTrigger: MazeGenerator.rooms is empty or not assigned.");
        }
    }
}
