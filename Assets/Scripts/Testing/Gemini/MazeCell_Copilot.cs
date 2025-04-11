using UnityEngine;

public class MazeCell_Copilot : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public Vector3 center;
    public bool hasRoom;
    public float roomWidth;   // Only valid if hasRoom is true.
    public float roomDepth;

    private float cellSize;
    private float floorThickness;
    private float wallThickness;
    private float wallHeight;
    private float roofThickness;

    // References to the generated parts (for room cells).
    private GameObject floorObject;
    private GameObject roofObject;
    private GameObject wallEast;
    private GameObject wallWest;
    private GameObject wallNorth;
    private GameObject wallSouth;

    /// <summary>
    /// Inits the MazeCell with provided parameters.
    /// </summary>
    public void Init(int x, int y, Vector3 center, float cellSize, float roomProbability,
                     float minRoomSize, float maxRoomSize, float floorThickness,
                     float wallThickness, float wallHeight, float roofThickness)
    {
        this.gridX = x;
        this.gridY = y;
        this.center = center;
        this.cellSize = cellSize;
        this.floorThickness = floorThickness;
        this.wallThickness = wallThickness;
        this.wallHeight = wallHeight;
        this.roofThickness = roofThickness;

        // Randomly decide if this cell will generate a room.
        hasRoom = (Random.value < roomProbability);

        if (hasRoom)
        {
            // Random room dimensions.
            roomWidth = Random.Range(minRoomSize, maxRoomSize);
            roomDepth = Random.Range(minRoomSize, maxRoomSize);
            CreateRoom();
        }
        else
        {
            CreatePassage();
        }
    }

    /// <summary>
    /// Create a room: floor, four walls, and a roof.
    /// </summary>
    void CreateRoom()
    {
        // Floor.
        floorObject = MazeUtils.CreatePrimitive("RoomFloor_" + gridX + "_" + gridY,
            center + new Vector3(0, floorThickness / 2f, 0),
            new Vector3(roomWidth, floorThickness, roomDepth),
            transform, MazeUtils.GetRandomColor());

        // Walls.
        // East wall.
        Vector3 posEast = new Vector3(center.x + roomWidth / 2 + wallThickness / 2, floorThickness + wallHeight / 2, center.z);
        wallEast = MazeUtils.CreatePrimitive("Wall_East_" + gridX + "_" + gridY, posEast,
            new Vector3(wallThickness, wallHeight, roomDepth + 2 * wallThickness), transform, Color.gray);

        // West wall.
        Vector3 posWest = new Vector3(center.x - roomWidth / 2 - wallThickness / 2, floorThickness + wallHeight / 2, center.z);
        wallWest = MazeUtils.CreatePrimitive("Wall_West_" + gridX + "_" + gridY, posWest,
            new Vector3(wallThickness, wallHeight, roomDepth + 2 * wallThickness), transform, Color.gray);

        // North wall.
        Vector3 posNorth = new Vector3(center.x, floorThickness + wallHeight / 2, center.z + roomDepth / 2 + wallThickness / 2);
        wallNorth = MazeUtils.CreatePrimitive("Wall_North_" + gridX + "_" + gridY, posNorth,
            new Vector3(roomWidth, wallHeight, wallThickness), transform, Color.gray);

        // South wall.
        Vector3 posSouth = new Vector3(center.x, floorThickness + wallHeight / 2, center.z - roomDepth / 2 - wallThickness / 2);
        wallSouth = MazeUtils.CreatePrimitive("Wall_South_" + gridX + "_" + gridY, posSouth,
            new Vector3(roomWidth, wallHeight, wallThickness), transform, Color.gray);

        // Roof.
        Vector3 roofPos = center + new Vector3(0, floorThickness + wallHeight + roofThickness / 2, 0);
        roofObject = MazeUtils.CreatePrimitive("Roof_" + gridX + "_" + gridY, roofPos,
            new Vector3(roomWidth + 2 * wallThickness, roofThickness, roomDepth + 2 * wallThickness), transform, Color.gray);
    }

    /// <summary>
    /// A passage cell simply gets a floor covering the entire cell.
    /// </summary>
    void CreatePassage()
    {
        floorObject = MazeUtils.CreatePrimitive("PassageFloor_" + gridX + "_" + gridY,
            center + new Vector3(0, floorThickness / 2, 0),
            new Vector3(cellSize, floorThickness, cellSize),
            transform, Color.gray);
    }

    // --- Public helper getters for door positions

    public Vector3 GetDoorPositionEast()
    {
        if (hasRoom)
            return new Vector3(center.x + roomWidth / 2, 0, center.z);
        else
            return new Vector3(center.x + cellSize / 2, 0, center.z);
    }

    public Vector3 GetDoorPositionWest()
    {
        if (hasRoom)
            return new Vector3(center.x - roomWidth / 2, 0, center.z);
        else
            return new Vector3(center.x - cellSize / 2, 0, center.z);
    }

    public Vector3 GetDoorPositionNorth()
    {
        if (hasRoom)
            return new Vector3(center.x, 0, center.z + roomDepth / 2);
        else
            return new Vector3(center.x, 0, center.z + cellSize / 2);
    }

    public Vector3 GetDoorPositionSouth()
    {
        if (hasRoom)
            return new Vector3(center.x, 0, center.z - roomDepth / 2);
        else
            return new Vector3(center.x, 0, center.z - cellSize / 2);
    }

    /// <summary>
    /// Called by the corridor generator to carve a door hole in the appropriate wall.
    /// </summary>
    public void CarveDoorHoleInWall(string direction, float doorWidth, float doorHeight)
    {
        // The MazeUtils.CarveDoorHole helper deletes the wall and rebuilds carved pieces.
        if (direction == "East" && wallEast != null)
        {
            wallEast = MazeUtils.CarveDoorHole(wallEast, "East", center, roomDepth,
                floorThickness, wallThickness, wallHeight, doorWidth, doorHeight, transform);
        }
        else if (direction == "West" && wallWest != null)
        {
            wallWest = MazeUtils.CarveDoorHole(wallWest, "West", center, roomDepth,
                floorThickness, wallThickness, wallHeight, doorWidth, doorHeight, transform);
        }
        else if (direction == "North" && wallNorth != null)
        {
            wallNorth = MazeUtils.CarveDoorHole(wallNorth, "North", center, roomWidth,
                floorThickness, wallThickness, wallHeight, doorWidth, doorHeight, transform);
        }
        else if (direction == "South" && wallSouth != null)
        {
            wallSouth = MazeUtils.CarveDoorHole(wallSouth, "South", center, roomWidth,
                floorThickness, wallThickness, wallHeight, doorWidth, doorHeight, transform);
        }
    }
}
