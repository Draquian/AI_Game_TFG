using UnityEngine;

public class BaseManager_Copilot : MonoBehaviour
{
    [Header("Room Dimensions")]
    [Tooltip("Width of the room floor along the X-axis.")]
    public float roomWidth = 20f;
    [Tooltip("Depth of the room floor along the Z-axis.")]
    public float roomDepth = 20f;
    [Tooltip("Thickness of the floor (and roof).")]
    public float floorThickness = 0.25f;
    [Tooltip("Thickness of the walls.")]
    public float wallThickness = 0.3f;
    [Tooltip("Height of the walls above the floor.")]
    public float wallHeight = 8f;
    [Tooltip("Thickness of the roof.")]
    public float roofThickness = 0.3f;

    [Header("Portal Settings")]
    [Tooltip("Portal size as a fraction of the room's smallest dimension (used only for positioning if needed).")]
    public float portalSizeFactor = 0.2f;
    [Tooltip("Distance behind the player to place the portal.")]
    public float portalBehindOffset = 2f;
    [Tooltip("Prefab for the portal that will be instantiated.")]
    public GameObject portalPrefab;

    void Start()
    {
        GenerateSingleRoom();
        PlacePlayerInRoom();
        PlacePortalInRoom();
    }

    /// <summary>
    /// Generates one single room composed of a floor, four walls, and a roof.
    /// All parts will be parented to the GameObject this script is attached to.
    /// </summary>
    void GenerateSingleRoom()
    {
        // Use this GameObject as the parent
        Transform parent = this.transform;

        // --- Floor ---
        // Create a floor as a cube scaled to our desired room size.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "RoomFloor_0";
        floor.transform.parent = parent;
        // Position: center the floor at (0, floorThickness/2, 0)
        floor.transform.position = new Vector3(0, floorThickness / 2f, 0);
        floor.transform.localScale = new Vector3(roomWidth, floorThickness, roomDepth);
        // Optionally, assign a random color.
        floor.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.yellow, Random.value);

        // --- Walls ---
        // Compute the Y center for the walls.
        float wallCenterY = floorThickness + wallHeight / 2f;

        // East Wall (right side)
        GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        eastWall.name = "Room_0_Wall_East";
        eastWall.transform.parent = parent;
        // Position the wall to the right of the floor.
        eastWall.transform.position = new Vector3(roomWidth / 2f + wallThickness / 2f, wallCenterY, 0);
        // Scale: wallThickness (X), wallHeight (Y) and roomDepth plus extra thickness (Z)
        eastWall.transform.localScale = new Vector3(wallThickness, wallHeight, roomDepth + wallThickness * 2f);
        eastWall.GetComponent<Renderer>().material.color = Color.gray;

        // West Wall (left side)
        GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        westWall.name = "Room_0_Wall_West";
        westWall.transform.parent = parent;
        westWall.transform.position = new Vector3(-roomWidth / 2f - wallThickness / 2f, wallCenterY, 0);
        westWall.transform.localScale = new Vector3(wallThickness, wallHeight, roomDepth + wallThickness * 2f);
        westWall.GetComponent<Renderer>().material.color = Color.gray;

        // North Wall (front)
        GameObject northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        northWall.name = "Room_0_Wall_North";
        northWall.transform.parent = parent;
        northWall.transform.position = new Vector3(0, wallCenterY, roomDepth / 2f + wallThickness / 2f);
        northWall.transform.localScale = new Vector3(roomWidth, wallHeight, wallThickness);
        northWall.GetComponent<Renderer>().material.color = Color.gray;

        // South Wall (back)
        GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        southWall.name = "Room_0_Wall_South";
        southWall.transform.parent = parent;
        southWall.transform.position = new Vector3(0, wallCenterY, -roomDepth / 2f - wallThickness / 2f);
        southWall.transform.localScale = new Vector3(roomWidth, wallHeight, wallThickness);
        southWall.GetComponent<Renderer>().material.color = Color.gray;

        // --- Roof ---
        // Position the roof on top of the walls.
        float roofY = floorThickness + wallHeight + roofThickness / 2f;
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Room_0_Roof";
        roof.transform.parent = parent;
        roof.transform.position = new Vector3(0, roofY, 0);
        // Extend the roof slightly beyond the floor to cover the walls.
        roof.transform.localScale = new Vector3(roomWidth + wallThickness * 2f, roofThickness, roomDepth + wallThickness * 2f);
        roof.GetComponent<Renderer>().material.color = Color.gray;
    }

    /// <summary>
    /// Places the player at the center of the generated room.
    /// The player is assumed to be tagged as "Player".
    /// </summary>
    void PlacePlayerInRoom()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Check if a save file exists
            if (SaveSystem_Copilot.SaveFileExists())
            {
                // Load saved game data (this can include the last scene name, player stats, etc.)
                SaveData_Copilot data = SaveSystem_Copilot.Load();

                player.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
            }
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure the player GameObject is tagged 'Player'.");
        }
    }

    /// <summary>
    /// Instantiates the portal prefab and positions it a little behind the player's current position.
    /// If the player is not found, the portal is placed at the room center.
    /// </summary>
    void PlacePortalInRoom()
    {
        Vector3 portalPosition;

        // Find the player GameObject.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Calculate the new position as a bit behind the player relative to its forward direction.
            //portalPosition = player.transform.position - player.transform.forward.normalized * portalBehindOffset;
            // Set the Y coordinate slightly above the floor level.
            portalPosition = new Vector3(-1.5f, floorThickness + 10f, -8f);
            portalPosition.y = floorThickness + 1f;
        }
        else
        {
            // Fallback: if no player is found, use the room center.
            portalPosition = new Vector3(0, floorThickness + 1f, 0);
        }

        // Now instantiate the portal using the prefab.
        if (portalPrefab != null)
        {
            // Instantiate the portal prefab at the computed position, set its rotation (here rotated to lie flat),
            // and parent it to the same GameObject that holds this script.
            GameObject portal = Instantiate(portalPrefab, portalPosition, Quaternion.Euler(0f, 0f, 0f), this.transform);
            Debug.Log("Portal prefab instantiated at " + portalPosition);
        }
        else
        {
            Debug.LogWarning("Portal prefab not assigned! Please assign the prefab in the Inspector.");
        }
    }
}
