using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator_Copilot : MonoBehaviour
{
    //Reference to the Boss Room to ensure the player place not contiguose
    public Room bossRoom = null;
    public Room passagewayRoom = null;
    public Room specialRoom = null;
    public Room returnRoom = null;
    public Room startRoom = null;

    public float specialLootSpawnChance = 0.25f; // 50% chance for the special room to spawn

    // Maze grid dimensions and cell size.
    private int gridSize = 3;
    private float cellSize = 30f;

    // Floor thickness for room and corridor floors.
    private float floorThickness = 0.25f;

    // Width of corridors connecting cells.
    private float corridorWidth = 5f;

    // Wall and roof parameters.
    private float wallThickness = 0.3f;
    private float wallHeight = 8f;
    private float roofThickness = 0.3f;

    // Door parameters.
    private float doorHeightRatio = 0.7f;
    private float doorThickness = 0.2f;  // Used for door object scale.

    // In this version, every cell will have a room.
    // Parent container for the entire maze.
    private GameObject mazeParent;

    //List of sprites to open the passageway has a code
    public List<Sprite> spawnableSprites;
    public List<Sprite> spawnedSprites;

    // Default sprites for non-special rooms.
    public Sprite defaultFloorSprite;  // For floor parts.
    public Sprite defaultWallSprite;   // For walls/rooftops.

    // Sprites for special boss room.
    public Sprite bossRoomFloorSprite;
    public Sprite bossRoomWallSprite;

    // Sprites for special portal room.
    public Sprite portalRoomFloorSprite;
    public Sprite portalRoomWallSprite;

    // Sprites for special return room.
    public Sprite returnRoomFloorSprite;
    public Sprite returnRoomWallSprite;

    // Sprites for special room (new special room).
    public Sprite specialRoomFloorSprite;
    public Sprite specialRoomWallSprite;

    // The Room class represents a cell on the grid.
    // Every cell gets a room (a floor, walls, and a roof).
    public class Room
    {
        public Vector3 center;   // Center of the cell.
        public float width;      // Room floor width.
        public float depth;      // Room floor depth.
        public int gridX;        // Horizontal cell index.
        public int gridY;        // Vertical cell index.
        public bool hasRoom;     // Always true.
    }

    // A helper class representing an edge between adjacent cells.
    // Edges can be horizontal or vertical.
    private class Edge
    {
        public int roomAIndex;
        public int roomBIndex;
        public bool horizontal;  // True if the edge is horizontal.
    }

    // Array for storing all cells.
    private Room[] rooms;

    void Update()
    {
        // Press "S" Start
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            startRun();
        }

        // Press "N" to increase grid size and regenerate the maze.
        if (Input.GetKeyDown(KeyCode.N))
        {
            nextLevel();
        }
        // Press "P" to decrease grid size (minimum 3) and regenerate the maze.
        else if (Input.GetKeyDown(KeyCode.P))
        {
            preLevel();
        }
    }

    public void startRun()
    {
        GenerateMaze();

        GameObject player = GameObject.FindWithTag("Player");
        ItemSO_Copilot item = player.GetComponent<Inventory_Copilot>().slots[0].item;

        if (item != null && item.itemType == ItemSO_Copilot.ItemType.Weapon)
        {
            WeaponStats weapon = item.stats as WeaponStats;
            player.GetComponent<PlayerStats_Copilot>().playerClass = weapon.classTpye;
            Debug.Log("Class type: " + weapon.classTpye);
        }
        else
        {
            Debug.Log("Non weapon was in the first slot");
        }
    }

    public void nextLevel()
    {
        gridSize++;
        GenerateMaze();
    }

    public void preLevel()
    {
        if (gridSize > 3)
        {
            gridSize--;
            GenerateMaze();
        }
        else
        {
            LoadingScreenManager_Copilot.Instance.LoadSceneWithLoadingScreen("Base");
        }
    }

    // GenerateMaze creates the grid of cells and connects them via corridors.
    void GenerateMaze()
    {
        DestroyPreviousSymbols();

        // Immediately destroy any previous maze.
        if (mazeParent != null)
        {
            DestroyImmediate(mazeParent);
        }
        mazeParent = new GameObject("MazeContainer");

        // Create each cell in the grid – every cell gets a room.
        rooms = new Room[gridSize * gridSize];
        float minRoomSize = cellSize * 0.3f;
        float maxRoomSize = cellSize * 0.8f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int index = y * gridSize + x;
                Room room = new Room();
                room.gridX = x;
                room.gridY = y;
                room.center = new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2);
                room.hasRoom = true; // Every cell is a room.
                room.width = Random.Range(minRoomSize, maxRoomSize);
                room.depth = Random.Range(minRoomSize, maxRoomSize);
                CreateRoomFloor(room, index);
                CreateRoomWallsAndRoof(room, index);
                rooms[index] = room;

                // NEW CODE: Attach RoomEnemySpawner and add a trigger collider whose effective size covers the room.
                GameObject roomFloor = GameObject.Find("RoomFloor_" + index);
                if (roomFloor != null)
                {
                    // Add the enemy spawner component (if not already present).
                    roomFloor.AddComponent<RoomEnemySpawner_Copilot>();

                    // --- Collider Setup ---
                    // A cube created by CreateRoomFloor() (via GameObject.CreatePrimitive) already has a BoxCollider.
                    // We want to set it as a trigger and ensure its effective size matches the room.
                    BoxCollider boxCol = roomFloor.GetComponent<BoxCollider>();
                    //if (boxCol == null)
                    //{
                        boxCol = roomFloor.AddComponent<BoxCollider>();
                    //}
                    // Mark this collider as a trigger.
                    boxCol.isTrigger = true;

                    // When you create a primitive cube, its mesh is 1 unit in each direction.
                    // In CreateRoomFloor(), you set the room floor’s transform.localScale = new Vector3(room.width, floorThickness, room.depth).
                    // Because the BoxCollider’s "size" property is defined in the local space,
                    // its default value (Vector3.one) already yields an effective dimension equal to (room.width, floorThickness, room.depth)
                    // due to the scale.
                    // Nevertheless, we explicitly re-set these values:
                    boxCol.center = new Vector3(0,1,0);  // ensure the collider is centered
                    boxCol.size = new Vector3(1, 3, 1); ;       // so that effective collider size = Vector3.one * transform.localScale.
                }
                else
                {
                    Debug.LogWarning("Room floor not found: RoomFloor_" + index);
                }
            }
        }



        // Build list of possible adjacent edges (horizontal & vertical) between cells.
        List<Edge> possibleEdges = new List<Edge>();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int index = y * gridSize + x;
                if (x < gridSize - 1)
                {
                    Edge edge = new Edge();
                    edge.roomAIndex = index;
                    edge.roomBIndex = index + 1;
                    edge.horizontal = true;
                    possibleEdges.Add(edge);
                }
                if (y < gridSize - 1)
                {
                    Edge edge = new Edge();
                    edge.roomAIndex = index;
                    edge.roomBIndex = index + gridSize;
                    edge.horizontal = false;
                    possibleEdges.Add(edge);
                }
            }
        }

        // Shuffle the list using Fisher–Yates.
        Shuffle(possibleEdges);

        // Use union–find (Kruskal's algorithm) to form a Minimum Spanning Tree (MST) connecting all cells.
        int totalCells = gridSize * gridSize;
        int[] parent = new int[totalCells];
        for (int i = 0; i < totalCells; i++)
        {
            parent[i] = i;
        }
        List<Edge> mstEdges = new List<Edge>();
        foreach (var edge in possibleEdges)
        {
            int a = Find(parent, edge.roomAIndex);
            int b = Find(parent, edge.roomBIndex);
            if (a != b)
            {
                mstEdges.Add(edge);
                parent[b] = a;
            }
        }

        // Generate corridors along each MST edge.
        foreach (var edge in mstEdges)
        {
            if (edge.horizontal)
                GenerateHorizontalCorridor(rooms[edge.roomAIndex], rooms[edge.roomBIndex]);
            else
                GenerateVerticalCorridor(rooms[edge.roomAIndex], rooms[edge.roomBIndex]);
        }

        // Designate a random room as the BossRoom and spawn the Boss.
        PlaceBoss();

        // Select a random Passageway Room and spawn the Portal.
        PlacePortal();

        Debug.LogError(Random.value);

        // After placing boss and player, spawn the special loot with a probability.
        if (Random.value < specialLootSpawnChance)
        {
        // Finally, place the special loot in a random room.
            PlaceSpecialLoot();
            Debug.LogError("PlaceLOOOOOT");
        }
        else
        {
            Debug.Log("Special loot did not spawn due to probability check.");
        }

        PlaceObjects(); // Spawn objects on room floors.

        // Place the Return room (Gateway) according to the current floor conditions.
        PlaceReturnRoom();

        // All maze walls, corridors, and rooms have been generated—now apply the sprite texture.
        ApplySpriteAsTextureToMaze();

        // Ensure the Player does not spawn too close to the BossRoom.
        PlacePlayer();

    }

    // Union–find helper with path compression.
    int Find(int[] parent, int i)
    {
        if (parent[i] == i)
            return i;
        parent[i] = Find(parent, parent[i]);
        return parent[i];
    }

    // Standard Fisher–Yates shuffle.
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
    // Create the floor for a room cell.
    void CreateRoomFloor(Room room, int index)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "RoomFloor_" + index;
        floor.transform.parent = mazeParent.transform;
        floor.transform.position = new Vector3(room.center.x, floorThickness / 2, room.center.z);
        floor.transform.localScale = new Vector3(room.width, floorThickness, room.depth);
        floor.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.yellow, Random.value);
    }

    // Create walls and a roof for a room cell.
    void CreateRoomWallsAndRoof(Room room, int index)
    {
        float wallY = floorThickness + wallHeight / 2;
        // East wall.
        Vector3 posEast = new Vector3(room.center.x + room.width / 2 + wallThickness / 2, wallY, room.center.z);
        GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        eastWall.name = "Room_" + index + "_Wall_East";
        eastWall.transform.parent = mazeParent.transform;
        eastWall.transform.position = posEast;
        eastWall.transform.localScale = new Vector3(wallThickness, wallHeight, room.depth + wallThickness * 2);
        eastWall.GetComponent<Renderer>().material.color = Color.gray;

        // West wall.
        Vector3 posWest = new Vector3(room.center.x - room.width / 2 - wallThickness / 2, wallY, room.center.z);
        GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        westWall.name = "Room_" + index + "_Wall_West";
        westWall.transform.parent = mazeParent.transform;
        westWall.transform.position = posWest;
        westWall.transform.localScale = new Vector3(wallThickness, wallHeight, room.depth + wallThickness * 2);
        westWall.GetComponent<Renderer>().material.color = Color.gray;

        // North wall.
        Vector3 posNorth = new Vector3(room.center.x, wallY, room.center.z + room.depth / 2 + wallThickness / 2);
        GameObject northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        northWall.name = "Room_" + index + "_Wall_North";
        northWall.transform.parent = mazeParent.transform;
        northWall.transform.position = posNorth;
        northWall.transform.localScale = new Vector3(room.width, wallHeight, wallThickness);
        northWall.GetComponent<Renderer>().material.color = Color.gray;

        // South wall.
        Vector3 posSouth = new Vector3(room.center.x, wallY, room.center.z - room.depth / 2 - wallThickness / 2);
        GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        southWall.name = "Room_" + index + "_Wall_South";
        southWall.transform.parent = mazeParent.transform;
        southWall.transform.position = posSouth;
        southWall.transform.localScale = new Vector3(room.width, wallHeight, wallThickness);
        southWall.GetComponent<Renderer>().material.color = Color.gray;

        // Roof.
        Vector3 roofPos = new Vector3(room.center.x, floorThickness + wallHeight + roofThickness / 2, room.center.z);
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Room_" + index + "_Roof";
        roof.transform.parent = mazeParent.transform;
        roof.transform.position = roofPos;
        roof.transform.localScale = new Vector3(room.width + wallThickness * 2, roofThickness, room.depth + wallThickness * 2);
        roof.GetComponent<Renderer>().material.color = Color.gray;
    }
    // Generate a horizontal corridor connecting two adjacent cells.
    void GenerateHorizontalCorridor(Room roomA, Room roomB)
    {
        // Use the room boundaries to determine the door positions.
        Vector3 doorA = new Vector3(roomA.center.x + roomA.width / 2, 0, roomA.center.z);
        Vector3 doorB = new Vector3(roomB.center.x - roomB.width / 2, 0, roomB.center.z);
        Vector3 corridorCenter = (doorA + doorB) / 2;
        corridorCenter.y = floorThickness / 2;
        float corridorLength = Mathf.Abs(doorB.x - doorA.x);

        // Create the main corridor.
        GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridor.name = "H_Corridor_" + roomA.gridX + "_" + roomA.gridY + "_to_" + roomB.gridX + "_" + roomB.gridY;
        corridor.transform.parent = mazeParent.transform;
        corridor.transform.position = corridorCenter;
        corridor.transform.localScale = new Vector3(corridorLength, floorThickness, corridorWidth);
        corridor.GetComponent<Renderer>().material.color = Color.gray;

        // Create the north wall of the corridor.
        float corridorWallY = floorThickness + wallHeight / 2;
        Vector3 northWallPos = new Vector3(corridorCenter.x, corridorWallY, corridorCenter.z + corridorWidth / 2 + wallThickness / 2);
        GameObject corridorNorthWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorNorthWall.name = corridor.name + "_Wall_North";
        corridorNorthWall.transform.parent = mazeParent.transform;
        corridorNorthWall.transform.position = northWallPos;
        corridorNorthWall.transform.localScale = new Vector3(corridorLength, wallHeight, wallThickness);
        corridorNorthWall.GetComponent<Renderer>().material.color = Color.gray;

        // Create the south wall of the corridor.
        Vector3 southWallPos = new Vector3(corridorCenter.x, corridorWallY, corridorCenter.z - corridorWidth / 2 - wallThickness / 2);
        GameObject corridorSouthWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorSouthWall.name = corridor.name + "_Wall_South";
        corridorSouthWall.transform.parent = mazeParent.transform;
        corridorSouthWall.transform.position = southWallPos;
        corridorSouthWall.transform.localScale = new Vector3(corridorLength, wallHeight, wallThickness);
        corridorSouthWall.GetComponent<Renderer>().material.color = Color.gray;

        // Create the roof of the corridor.
        Vector3 roofPos = new Vector3(corridorCenter.x, floorThickness + wallHeight + roofThickness / 2, corridorCenter.z);
        GameObject corridorRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorRoof.name = corridor.name + "_Roof";
        corridorRoof.transform.parent = mazeParent.transform;
        corridorRoof.transform.position = roofPos;
        corridorRoof.transform.localScale = new Vector3(corridorLength + wallThickness * 2, roofThickness, corridorWidth + wallThickness * 2);
        corridorRoof.GetComponent<Renderer>().material.color = Color.gray;

        // Calculate door dimensions.
        float doorHeight = wallHeight * doorHeightRatio;
        float doorWidth = corridorWidth;
        float doorCenterY = floorThickness + doorHeight / 2;

        // Instead of instantiating door objects, only carve door holes in the room walls.
        if (roomA.hasRoom)
        {
            int indexA = roomA.gridY * gridSize + roomA.gridX;
            CarveDoorHole_East(roomA, indexA, doorWidth, doorHeight);
        }
        if (roomB.hasRoom)
        {
            int indexB = roomB.gridY * gridSize + roomB.gridX;
            CarveDoorHole_West(roomB, indexB, doorWidth, doorHeight);
        }
    }


    // Generate a vertical corridor connecting two adjacent cells.
    void GenerateVerticalCorridor(Room roomA, Room roomB)
    {
        // Compute positions for the corridor endpoints based on the rooms' door sides.
        Vector3 doorA = new Vector3(roomA.center.x, 0, roomA.center.z + roomA.depth / 2);
        Vector3 doorB = new Vector3(roomB.center.x, 0, roomB.center.z - roomB.depth / 2);
        Vector3 corridorCenter = (doorA + doorB) / 2;
        corridorCenter.y = floorThickness / 2;
        float corridorLength = Mathf.Abs(doorB.z - doorA.z);

        // Create the main corridor.
        GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridor.name = "V_Corridor_" + roomA.gridX + "_" + roomA.gridY + "_to_" + roomB.gridX + "_" + roomB.gridY;
        corridor.transform.parent = mazeParent.transform;
        corridor.transform.position = corridorCenter;
        corridor.transform.localScale = new Vector3(corridorWidth, floorThickness, corridorLength);
        corridor.GetComponent<Renderer>().material.color = Color.gray;

        // Create the east wall of the corridor.
        float corridorWallY = floorThickness + wallHeight / 2;
        Vector3 eastWallPos = new Vector3(corridorCenter.x + corridorWidth / 2 + wallThickness / 2, corridorWallY, corridorCenter.z);
        GameObject corridorEastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorEastWall.name = corridor.name + "_Wall_East";
        corridorEastWall.transform.parent = mazeParent.transform;
        corridorEastWall.transform.position = eastWallPos;
        corridorEastWall.transform.localScale = new Vector3(wallThickness, wallHeight, corridorLength);
        corridorEastWall.GetComponent<Renderer>().material.color = Color.gray;

        // Create the west wall of the corridor.
        Vector3 westWallPos = new Vector3(corridorCenter.x - corridorWidth / 2 - wallThickness / 2, corridorWallY, corridorCenter.z);
        GameObject corridorWestWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorWestWall.name = corridor.name + "_Wall_West";
        corridorWestWall.transform.parent = mazeParent.transform;
        corridorWestWall.transform.position = westWallPos;
        corridorWestWall.transform.localScale = new Vector3(wallThickness, wallHeight, corridorLength);
        corridorWestWall.GetComponent<Renderer>().material.color = Color.gray;

        // Create the corridor roof.
        Vector3 roofPos = new Vector3(corridorCenter.x, floorThickness + wallHeight + roofThickness / 2, corridorCenter.z);
        GameObject corridorRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorRoof.name = corridor.name + "_Roof";
        corridorRoof.transform.parent = mazeParent.transform;
        corridorRoof.transform.position = roofPos;
        corridorRoof.transform.localScale = new Vector3(corridorWidth + wallThickness * 2, roofThickness, corridorLength + wallThickness * 2);
        corridorRoof.GetComponent<Renderer>().material.color = Color.gray;

        // Define door dimensions.
        float doorHeight = wallHeight * doorHeightRatio;
        float doorWidth = corridorWidth;
        // The door object instantiations are removed—only the wall carvings remain.

        // For roomA (the lower room), carve a hole in its north wall.
        if (roomA.hasRoom)
        {
            int indexLower = roomA.gridY * gridSize + roomA.gridX;
            CarveDoorHole_North(roomA, indexLower, doorWidth, doorHeight);
        }
        // For roomB (the upper room), carve a hole in its south wall.
        if (roomB.hasRoom)
        {
            int indexUpper = roomB.gridY * gridSize + roomB.gridX;
            CarveDoorHole_South(roomB, indexUpper, doorWidth, doorHeight);
        }
    }


    // ----- Door Hole Carving Methods -----

    void CarveDoorHole_East(Room room, int index, float doorWidth, float doorHeight)
    {
        string wallName = "Room_" + index + "_Wall_East";
        GameObject wall = GameObject.Find(wallName);
        if (wall != null)
        {
            Vector3 pos = wall.transform.position;
            Vector3 scale = wall.transform.localScale; // (wallThickness, wallHeight, room.depth + wallThickness*2)
            float fullHeight = scale.y;
            float fullDepth = scale.z;
            Destroy(wall);

            float topPieceHeight = fullHeight - doorHeight;
            Vector3 topPos = pos;
            topPos.y = floorThickness + doorHeight + topPieceHeight / 2;
            GameObject topPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topPiece.name = "Carved_" + wallName + "_Top";
            topPiece.transform.parent = mazeParent.transform;
            topPiece.transform.position = topPos;
            topPiece.transform.localScale = new Vector3(scale.x, topPieceHeight, fullDepth);
            topPiece.GetComponent<Renderer>().material.color = Color.gray;

            float leftoverDepth = (fullDepth - doorWidth) / 2;
            Vector3 leftPos = pos;
            leftPos.y = floorThickness + doorHeight / 2;
            leftPos.z = pos.z - (doorWidth / 2 + leftoverDepth / 2);
            GameObject leftPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPiece.name = "Carved_" + wallName + "_Left";
            leftPiece.transform.parent = mazeParent.transform;
            leftPiece.transform.position = leftPos;
            leftPiece.transform.localScale = new Vector3(scale.x, doorHeight, leftoverDepth);
            leftPiece.GetComponent<Renderer>().material.color = Color.gray;

            Vector3 rightPos = pos;
            rightPos.y = floorThickness + doorHeight / 2;
            rightPos.z = pos.z + (doorWidth / 2 + leftoverDepth / 2);
            GameObject rightPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPiece.name = "Carved_" + wallName + "_Right";
            rightPiece.transform.parent = mazeParent.transform;
            rightPiece.transform.position = rightPos;
            rightPiece.transform.localScale = new Vector3(scale.x, doorHeight, leftoverDepth);
            rightPiece.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    void CarveDoorHole_West(Room room, int index, float doorWidth, float doorHeight)
    {
        string wallName = "Room_" + index + "_Wall_West";
        GameObject wall = GameObject.Find(wallName);
        if (wall != null)
        {
            Vector3 pos = wall.transform.position;
            Vector3 scale = wall.transform.localScale;
            float fullHeight = scale.y;
            float fullDepth = scale.z;
            Destroy(wall);

            float topPieceHeight = fullHeight - doorHeight;
            Vector3 topPos = pos;
            topPos.y = floorThickness + doorHeight + topPieceHeight / 2;
            GameObject topPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topPiece.name = "Carved_" + wallName + "_Top";
            topPiece.transform.parent = mazeParent.transform;
            topPiece.transform.position = topPos;
            topPiece.transform.localScale = new Vector3(scale.x, topPieceHeight, fullDepth);
            topPiece.GetComponent<Renderer>().material.color = Color.gray;

            float leftoverDepth = (fullDepth - doorWidth) / 2;
            Vector3 leftPos = pos;
            leftPos.y = floorThickness + doorHeight / 2;
            leftPos.z = pos.z - (doorWidth / 2 + leftoverDepth / 2);
            GameObject leftPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPiece.name = "Carved_" + wallName + "_Left";
            leftPiece.transform.parent = mazeParent.transform;
            leftPiece.transform.position = leftPos;
            leftPiece.transform.localScale = new Vector3(scale.x, doorHeight, leftoverDepth);
            leftPiece.GetComponent<Renderer>().material.color = Color.gray;

            Vector3 rightPos = pos;
            rightPos.y = floorThickness + doorHeight / 2;
            rightPos.z = pos.z + (doorWidth / 2 + leftoverDepth / 2);
            GameObject rightPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPiece.name = "Carved_" + wallName + "_Right";
            rightPiece.transform.parent = mazeParent.transform;
            rightPiece.transform.position = rightPos;
            rightPiece.transform.localScale = new Vector3(scale.x, doorHeight, leftoverDepth);
            rightPiece.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    void CarveDoorHole_North(Room room, int index, float doorWidth, float doorHeight)
    {
        string wallName = "Room_" + index + "_Wall_North";
        GameObject wall = GameObject.Find(wallName);
        if (wall != null)
        {
            Vector3 pos = wall.transform.position;
            Vector3 scale = wall.transform.localScale; // (room.width, wallHeight, wallThickness)
            float fullHeight = scale.y;
            float fullWidth = scale.x;
            Destroy(wall);

            float topPieceHeight = fullHeight - doorHeight;
            Vector3 topPos = pos;
            topPos.y = floorThickness + doorHeight + topPieceHeight / 2;
            GameObject topPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topPiece.name = "Carved_" + wallName + "_Top";
            topPiece.transform.parent = mazeParent.transform;
            topPiece.transform.position = topPos;
            topPiece.transform.localScale = new Vector3(fullWidth, topPieceHeight, scale.z);
            topPiece.GetComponent<Renderer>().material.color = Color.gray;

            float leftoverWidth = (fullWidth - doorWidth) / 2;
            Vector3 leftPos = pos;
            leftPos.y = floorThickness + doorHeight / 2;
            leftPos.x = pos.x - (doorWidth / 2 + leftoverWidth / 2);
            GameObject leftPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPiece.name = "Carved_" + wallName + "_Left";
            leftPiece.transform.parent = mazeParent.transform;
            leftPiece.transform.position = leftPos;
            leftPiece.transform.localScale = new Vector3(leftoverWidth, doorHeight, scale.z);
            leftPiece.GetComponent<Renderer>().material.color = Color.gray;

            Vector3 rightPos = pos;
            rightPos.y = floorThickness + doorHeight / 2;
            rightPos.x = pos.x + (doorWidth / 2 + leftoverWidth / 2);
            GameObject rightPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPiece.name = "Carved_" + wallName + "_Right";
            rightPiece.transform.parent = mazeParent.transform;
            rightPiece.transform.position = rightPos;
            rightPiece.transform.localScale = new Vector3(leftoverWidth, doorHeight, scale.z);
            rightPiece.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    void CarveDoorHole_South(Room room, int index, float doorWidth, float doorHeight)
    {
        string wallName = "Room_" + index + "_Wall_South";
        GameObject wall = GameObject.Find(wallName);
        if (wall != null)
        {
            Vector3 pos = wall.transform.position;
            Vector3 scale = wall.transform.localScale; // (room.width, wallHeight, wallThickness)
            float fullHeight = scale.y;
            float fullWidth = scale.x;
            Destroy(wall);

            float topPieceHeight = fullHeight - doorHeight;
            Vector3 topPos = pos;
            topPos.y = floorThickness + doorHeight + topPieceHeight / 2;
            GameObject topPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topPiece.name = "Carved_" + wallName + "_Top";
            topPiece.transform.parent = mazeParent.transform;
            topPiece.transform.position = topPos;
            topPiece.transform.localScale = new Vector3(fullWidth, topPieceHeight, scale.z);
            topPiece.GetComponent<Renderer>().material.color = Color.gray;

            float leftoverWidth = (fullWidth - doorWidth) / 2;
            Vector3 leftPos = pos;
            leftPos.y = floorThickness + doorHeight / 2;
            leftPos.x = pos.x - (doorWidth / 2 + leftoverWidth / 2);
            GameObject leftPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPiece.name = "Carved_" + wallName + "_Left";
            leftPiece.transform.parent = mazeParent.transform;
            leftPiece.transform.position = leftPos;
            leftPiece.transform.localScale = new Vector3(leftoverWidth, doorHeight, scale.z);
            leftPiece.GetComponent<Renderer>().material.color = Color.gray;

            Vector3 rightPos = pos;
            rightPos.y = floorThickness + doorHeight / 2;
            rightPos.x = pos.x + (doorWidth / 2 + leftoverWidth / 2);
            GameObject rightPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPiece.name = "Carved_" + wallName + "_Right";
            rightPiece.transform.parent = mazeParent.transform;
            rightPiece.transform.position = rightPos;
            rightPiece.transform.localScale = new Vector3(leftoverWidth, doorHeight, scale.z);
            rightPiece.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    void PlacePlayer(int BRDistance = 2)
    {
        // (Existing logic: build a list of valid spawn rooms that are not contiguous to the BossRoom.)
        List<Room> validRooms = new List<Room>();
        foreach (Room room in rooms)
        {
            if (RoomDistance(room, bossRoom) >= BRDistance && RoomDistance(room, passagewayRoom) >= BRDistance)
            {
                validRooms.Add(room);
            }       
        }

        if (validRooms.Count > 0)
        {

            startRoom = validRooms[Random.Range(0, validRooms.Count)];
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(startRoom.center.x, floorThickness + 1.0f, startRoom.center.z);
                player.GetComponent<PlayerController_Copilot>().enabled = true;
                // Find the index of the chosen room.
                int spawnIndex = -1;
                for (int i = 0; i < rooms.Length; i++)
                {
                    if (rooms[i] == startRoom)
                    {
                        spawnIndex = i;
                        break;
                    }
                }
                if (spawnIndex != -1)
                {
                    GameObject initialRoomFloor = GameObject.Find("RoomFloor_" + spawnIndex);
                    if (initialRoomFloor != null)
                    {
                        RoomEnemySpawner_Copilot spawner = initialRoomFloor.GetComponent<RoomEnemySpawner_Copilot>();

                        // Now: if any enemy (tagged "Enemy") is within this room's bounds, destroy it.
                        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

                        // Calculate half-extents based on room dimensions.
                        float halfWidth = startRoom.width * 0.5f;
                        float halfDepth = startRoom.depth * 0.5f;

                        foreach (GameObject enemy in enemies)
                        {
                            Vector3 enemyPos = enemy.transform.position;
                            // Check if the enemy's X and Z coordinates are within the room bounds.
                            if (enemyPos.x >= startRoom.center.x - halfWidth &&
                                enemyPos.x <= startRoom.center.x + halfWidth &&
                                enemyPos.z >= startRoom.center.z - halfDepth &&
                                enemyPos.z <= startRoom.center.z + halfDepth)
                            {
                                Destroy(enemy);
                                Debug.Log("Destroyed enemy '" + enemy.name + "' inside the player spawn room.");
                            }
                        }
                        if (spawner != null)
                        {
                            Destroy(spawner);
                            Debug.Log("Removed RoomEnemySpawner_Copilot from the initial player room: RoomFloor_" + spawnIndex);
                        }
                        else
                        {
                            Debug.LogWarning("RoomEnemySpawner_Copilot not found on " + initialRoomFloor.name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Could not find the initial room floor named \"RoomFloor_" + spawnIndex + "\"");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Player object with tag 'Player' not found!");
            }
        }
        else
        {
            PlacePlayer(BRDistance - 1);
            Debug.LogError("No valid player spawn rooms found! The grid may be too small.");
        }

        // Call PlaceInStartRoom() on the PreLevelTrigger.
        PreLevelTrigger_Copilot preLevelTrigger = FindObjectOfType<PreLevelTrigger_Copilot>();
        if (preLevelTrigger != null)
        {
            preLevelTrigger.PlaceInStartRoom();
        }
        else
        {
            Debug.LogWarning("PreLevelTrigger not found in the scene.");
        }
    }



    // This method selects a random room from the generated maze and places the Boss there.
    void PlaceBoss()
    {

        // Select a random room from your generated rooms array.
        int randomIndex = Random.Range(0, rooms.Length);
        // Store the selected room as the bossRoom.
        bossRoom = rooms[randomIndex];

        GameObject bossRoomFloor = GameObject.Find("RoomFloor_" + randomIndex);

        // Add the enemy spawner component (if not already present).
        Destroy(bossRoomFloor.GetComponent<RoomEnemySpawner_Copilot>());
        bossRoomFloor.AddComponent<BossRoomManager_Copilot>();

        // --- Collider Setup ---
        // A cube created by CreateRoomFloor() (via GameObject.CreatePrimitive) already has a BoxCollider.
        // We want to set it as a trigger and ensure its effective size matches the room.
        BoxCollider boxCol = bossRoomFloor.GetComponent<BoxCollider>();
        //if (boxCol == null)
        //{
        boxCol = bossRoomFloor.AddComponent<BoxCollider>();
        //}
        // Mark this collider as a trigger.
        boxCol.isTrigger = true;

        // When you create a primitive cube, its mesh is 1 unit in each direction.
        // In CreateRoomFloor(), you set the room floor’s transform.localScale = new Vector3(room.width, floorThickness, room.depth).
        // Because the BoxCollider’s "size" property is defined in the local space,
        // its default value (Vector3.one) already yields an effective dimension equal to (room.width, floorThickness, room.depth)
        // due to the scale.
        // Nevertheless, we explicitly re-set these values:
        boxCol.center = new Vector3(0, 3, 0);  // ensure the collider is centered
        boxCol.size = new Vector3(1, 6, 1); ;       // so that effective collider size = Vector3.one * transform.localScale.

        // Find the Boss GameObject (make sure the Boss is tagged "Boss").
        /*GameObject boss = GameObject.FindWithTag("Boss");
        if (boss != null)
        {
            // Position the boss at the center of the randomly selected room.
            boss.transform.position = new Vector3(bossRoom.center.x, floorThickness + 1.0f, bossRoom.center.z);

            // Now, remove the enemy spawner component from the BossRoom.
            // With your naming convention, the room floor should be named "RoomFloor_" + randomIndex.
            GameObject bossRoomFloor = GameObject.Find("RoomFloor_" + randomIndex);
            if (bossRoomFloor != null)
            {
                // Attempt to get the RoomEnemySpawner_Copilot component attached to the floor.
                RoomEnemySpawner_Copilot spawner = bossRoomFloor.GetComponent<RoomEnemySpawner_Copilot>();
                if (spawner != null)
                {
                    // Remove the component so that enemies will not spawn in the BossRoom.
                    Destroy(spawner);
                    Debug.Log("Removed RoomEnemySpawner_Copilot from the BossRoom (RoomFloor_" + randomIndex + ").");
                }
                else
                {
                    Debug.LogWarning("RoomEnemySpawner_Copilot component not found on " + bossRoomFloor.name);
                }
            }
            else
            {
                Debug.LogWarning("Boss room floor not found with name \"RoomFloor_" + randomIndex + "\"");
            }
        }
        else
        {
            Debug.LogWarning("Boss object with tag 'Boss' not found!");
        }*/
    }


    // This function calculates the Manhattan distance (grid distance) between two rooms.
    int RoomDistance(Room roomA, Room roomB)
    {
        int deltaX = Mathf.Abs(roomA.gridX - roomB.gridX);
        int deltaY = Mathf.Abs(roomA.gridY - roomB.gridY);

        return Mathf.Max(deltaX, deltaY); // Use the larger of the two distances to ensure a minimum separation.
    }

    // This method selects a random room to be the Passageway Room where the Portal will spawn.
    void PlacePortal()
    {
        // Select a random room from the maze.
        int randomIndex = Random.Range(0, rooms.Length);
        passagewayRoom = rooms[randomIndex];

        // Find the Portal GameObject tagged "Portal."
        GameObject portal = GameObject.FindWithTag("Portal");
        if (portal != null)
        {
            // Spawn the Portal at the center of the chosen Passageway Room.
            portal.transform.position = new Vector3(passagewayRoom.center.x, floorThickness + 3.0f, passagewayRoom.center.z);
        }
        else
        {
            Debug.LogWarning("Portal object with tag 'Portal' not found!");
        }
    }


    // This method selects a random room as the Special room where a different loot will spawn.
    void PlaceSpecialLoot()
    {
        // Randomly choose one room from the array of generated rooms.
        int randomIndex = Random.Range(0, rooms.Length);
        specialRoom = rooms[randomIndex];

        // Find the special loot GameObject by its tag ("SpecialLoot").
        GameObject specialLoot = GameObject.FindWithTag("SpecialLoot");
        if (specialLoot != null)
        {
            // Set the loot's position to the center of the chosen room (with an upward offset so it’s above the floor).
            specialLoot.transform.position = new Vector3(specialRoom.center.x, floorThickness + 1.0f, specialRoom.center.z);
        }
        else
        {
            Debug.LogWarning("Special loot object with tag 'SpecialLoot' not found!");
        }
    }

    // Spawn 5 objects (from the list set in the Inspector) in the floors of 5 different rooms.
    void PlaceObjects()
    {
        // Ensure we have sprites to spawn.
        if (spawnableSprites == null || spawnableSprites.Count == 0)
        {
            Debug.LogWarning("No sprites assigned in the spawnableSprites list!");
            return;
        }

        // Create a temporary copy of the original sprite list.
        List<Sprite> tempSpritePool = new List<Sprite>(spawnableSprites);

        // Shuffle the rooms list so that the placement is randomized.
        List<Room> roomList = new List<Room>(rooms);

        // Remove the passagewayRoom from the list, so no object is spawned there.
        if (passagewayRoom != null)
        {
            roomList.Remove(passagewayRoom);
        }

        Shuffle(roomList);

        // Determine how many sprites to spawn.
        int numToSpawn = Mathf.Min(5, roomList.Count);

        // Now spawn the remaining objects in random rooms.
        for (int i = 0; i < numToSpawn; i++)
        {
            // Use the ith room from the shuffled list.
            Room targetRoom = roomList[i];
            SpawnObjectInRoom(targetRoom, tempSpritePool);
        }
    }

    /// <summary>
    /// Spawns one object in the given room.
    /// </summary>
    /// <param name="room">The room where the object will be spawned.</param>
    void SpawnObjectInRoom(Room room, List<Sprite> spritePool)
    {
        // Randomly select a sprite from the spawnableSprites list.
        int randomSpriteIndex = Random.Range(0, spritePool.Count);
        Sprite spriteToSpawn = spritePool[randomSpriteIndex];
        spawnedSprites.Add(spriteToSpawn);

        // Remove the chosen sprite from the temporary pool.
        spritePool.RemoveAt(randomSpriteIndex);

        // Create a new GameObject to display the sprite.
        GameObject spriteObj = new GameObject("Spawned_" + spriteToSpawn.name);

        // Add a SpriteRenderer component and assign the sprite.
        SpriteRenderer spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteToSpawn;

        // Place the object at the room's center, just above the floor.
        Vector3 spawnPos = new Vector3(room.center.x, floorThickness + 0.001f, room.center.z);
        spriteObj.transform.position = spawnPos;

        spriteObj.transform.rotation = Quaternion.LookRotation(Vector3.up);
    }


    // This method designates a random room as the Return room (where the Gateway appears)
    // based on the current floor. For floor 8 and floors >=15 the spawn is 100%,
    // while for floors 9 to 14 the probability increases by 10% per floor (floor 9 = 10%, floor 10 = 20%, etc.).
    void PlaceReturnRoom()
    {
        float spawnProbability = 0f;
        if ((gridSize - 2) == 8 || (gridSize - 2) >= 15)
        {
            spawnProbability = 1f;
        }
        else if ((gridSize - 2) >= 9 && (gridSize - 2) <= 14)
        {
            spawnProbability = 0.1f * ((gridSize - 2) - 8);
        }
        else if ((gridSize - 2) >= 5 && (gridSize - 2) <= 8)
        {
            spawnProbability = 0.1f * ((gridSize - 2) - 4);
        }

        if (Random.value < spawnProbability)
        {
            // Select a random room as the Return room.
            int randomIndex = Random.Range(0, rooms.Length);
            returnRoom = rooms[randomIndex];

            // Find the Gateway object (tagged "Gateway").
            GameObject gateway = GameObject.FindWithTag("Gateway");
            if (gateway != null)
            {
                gateway.transform.position = new Vector3(returnRoom.center.x, floorThickness + 1.0f, returnRoom.center.z);

                // Remove the enemy spawner component from the Return room.
                GameObject returnRoomFloor = GameObject.Find("RoomFloor_" + randomIndex);
                if (returnRoomFloor != null)
                {
                    RoomEnemySpawner_Copilot spawner = returnRoomFloor.GetComponent<RoomEnemySpawner_Copilot>();
                    if (spawner != null)
                    {
                        Destroy(spawner);
                        Debug.Log("Removed RoomEnemySpawner_Copilot from Return room: RoomFloor_" + randomIndex);
                    }
                    else
                    {
                        Debug.LogWarning("RoomEnemySpawner_Copilot not found on " + returnRoomFloor.name);
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find the Return room floor named \"RoomFloor_" + randomIndex + "\"");
                }
            }
            else
            {
                Debug.LogWarning("Gateway object with tag 'Gateway' not found!");
            }
        }
        else
        {
            Debug.Log("Return room did not spawn on floor " + (gridSize - 2) + " (Probability was " + spawnProbability + ").");
        }
    }

    // Removes all previously spawned symbols before generating a new maze.
    void DestroyPreviousSymbols()
    {
        // Assuming symbols are tagged "Symbol" when instantiated in the maze.
        GameObject[] symbols = GameObject.FindGameObjectsWithTag("Symbol");

        foreach (GameObject symbol in symbols)
        {
            Destroy(symbol);
        }

        Debug.Log("All previous Symbols have been destroyed.");
    }

    void ApplySpriteAsTextureToMaze()
    {
        // Make sure the default sprites are assigned.
        if (defaultFloorSprite == null || defaultWallSprite == null)
        {
            Debug.LogWarning("Default floor or wall sprite not assigned!");
            return;
        }

        // Compute an identifier string for each special room (e.g., "_5")
        string bossIdStr = "";
        string portalIdStr = "";
        string returnIdStr = "";
        string specialIdStr = "";

        if (bossRoom != null)
        {
            int bossIndex = bossRoom.gridY * gridSize + bossRoom.gridX;
            bossIdStr = "_" + bossIndex.ToString();
        }
        if (passagewayRoom != null)
        {
            int portalIndex = passagewayRoom.gridY * gridSize + passagewayRoom.gridX;
            portalIdStr = "_" + portalIndex.ToString();
        }
        if (returnRoom != null)
        {
            int returnIndex = returnRoom.gridY * gridSize + returnRoom.gridX;
            returnIdStr = "_" + returnIndex.ToString();
        }
        if (specialRoom != null)
        {
            int specialIndex = specialRoom.gridY * gridSize + specialRoom.gridX;
            specialIdStr = "_" + specialIndex.ToString();
        }

        // Iterate through every maze object under mazeParent.
        foreach (Transform child in mazeParent.transform)
        {
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mr == null)
                continue;

            // Convert name to lowercase for case-insensitive checks.
            string lowerName = child.name.ToLower();
            // Decide if this element is a floor part.
            bool isFloor = lowerName.Contains("floor");

            // Determine the sprite based on room ownership and element type.
            // Check for special rooms in order.
            Sprite selectedSprite = null;

            if (!string.IsNullOrEmpty(bossIdStr) && lowerName.Contains(bossIdStr))
            {
                selectedSprite = isFloor ? bossRoomFloorSprite : bossRoomWallSprite;
            }
            else if (!string.IsNullOrEmpty(portalIdStr) && lowerName.Contains(portalIdStr))
            {
                selectedSprite = isFloor ? portalRoomFloorSprite : portalRoomWallSprite;
            }
            else if (!string.IsNullOrEmpty(returnIdStr) && lowerName.Contains(returnIdStr))
            {
                selectedSprite = isFloor ? returnRoomFloorSprite : returnRoomWallSprite;
            }
            else if (!string.IsNullOrEmpty(specialIdStr) && lowerName.Contains(specialIdStr))
            {
                selectedSprite = isFloor ? specialRoomFloorSprite : specialRoomWallSprite;
            }
            else
            {
                // Not part of a special room: use the default sprite.
                selectedSprite = isFloor ? defaultFloorSprite : defaultWallSprite;
            }

            // If no sprite was determined (e.g. a reference was missing), default to the default floor sprite.
            if (selectedSprite == null)
            {
                selectedSprite = defaultFloorSprite;
            }

            // Create a new material using a shader that supports texture tiling.
            Material matInstance = new Material(Shader.Find("Unlit/Texture"));
            // Set the material's main texture to the sprite's texture.
            matInstance.mainTexture = selectedSprite.texture;
            // Determine the tiling size based on the object's surface dimensions.
            Collider col = child.GetComponent<Collider>();
            Vector2 tilingSize;
            if (col != null)
            {
                tilingSize = new Vector2(col.bounds.size.x, col.bounds.size.z);
            }
            else
            {
                tilingSize = new Vector2(child.localScale.x, child.localScale.z);
            }
            matInstance.mainTextureScale = tilingSize;

            // Assign the new material to the object's MeshRenderer.
            mr.material = matInstance;
        }

        Debug.Log("Applied tiled sprite materials to maze objects with floor and wall/roof textures for special rooms.");
    }
}
