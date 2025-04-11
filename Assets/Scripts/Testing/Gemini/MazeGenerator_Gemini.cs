using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for Linq operations like Except

// MazeCell definition remains the same as before
public class MazeCell
{
    public int X, Y;
    public bool HasRoom = false;
    public Rect RoomRect; // Relative to cell bottom-left corner (0,0) to (cellSize, cellSize)
    public bool Visited = false; // For connectivity check AND pathfinding

    // Passages: True if there is a passage leading out in that direction
    public bool PassageN, PassageS, PassageE, PassageW;
    // New: Track if this cell is part of a generated passage path (even if no room)
    public bool IsPassage = false;

    // For pathfinding (used in secondary connection logic if needed)
    public MazeCell ParentCell = null;


    public MazeCell(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector2Int GridPosition => new Vector2Int(X, Y);
    public Vector2 WorldCenter(float cellSize) => new Vector2((X + 0.5f) * cellSize, (Y + 0.5f) * cellSize);

    // Helper to reset flags for regeneration
    public void ResetFlags()
    {
        Visited = false;
        PassageN = PassageS = PassageE = PassageW = false;
        IsPassage = false;
        ParentCell = null;
    }
}

public class MazeGenerator_Gemini : MonoBehaviour
{
    // --- Configuration (Set via script, not Inspector) ---
    private int gridRows = 3;
    private int gridCols = 3;
    private float cellSize = 10f;
    private float wallHeight = 3f;
    private float passageWidth = 2f;
    private float roomProbability = 0.7f;
    private float minRoomSizeFactor = 0.4f;
    private float maxRoomSizeFactor = 0.8f;
    private string materialPath = "Materials/DefaultMazeMaterial";

    // --- New Door Configuration ---
    private float doorHeight = 2.2f; // Height of the door opening
    private float doorWidth = 1.5f; // Width of the door opening (should be <= passageWidth)

    // --- State ---
    private MazeCell[,] grid;
    private List<MazeCell> roomCells = new List<MazeCell>();
    private GameObject mazeMeshObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Material mazeMaterial;

    void Start()
    {
        mazeMaterial = Resources.Load<Material>(materialPath);
        if (mazeMaterial == null)
        {
            Debug.LogError($"Failed to load material at path: Resources/{materialPath}. Please create it.");
            mazeMaterial = new Material(Shader.Find("Standard"));
            mazeMaterial.color = Color.gray;
            Debug.LogWarning("Using fallback gray material.");
        }

        // Ensure door width isn't wider than the passage
        doorWidth = Mathf.Min(doorWidth, passageWidth);

        GenerateNewMaze();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            gridRows++;
            gridCols++;
            Debug.Log($"Increasing maze size to {gridRows}x{gridCols}");
            GenerateNewMaze();
        }
    }

    void GenerateNewMaze()
    {
        if (mazeMeshObject != null) Destroy(mazeMeshObject);

        mazeMeshObject = new GameObject("MazeMesh");
        mazeMeshObject.transform.SetParent(this.transform);
        meshFilter = mazeMeshObject.AddComponent<MeshFilter>();
        meshRenderer = mazeMeshObject.AddComponent<MeshRenderer>();
        meshCollider = mazeMeshObject.AddComponent<MeshCollider>();
        meshRenderer.material = mazeMaterial;

        InitializeGrid();
        PlaceRooms();

        if (roomCells.Count == 0)
        {
            Debug.LogWarning("No rooms generated. Maze will be empty.");
            meshFilter.mesh = new Mesh(); // Assign empty mesh
            return;
        }

        EnsureConnectivityBFS(); // Use BFS based connectivity

        // --- Generate Mesh ---
        // Pass door dimensions to the builder
        Mesh mazeMesh = MazeMeshBuilder.BuildMesh(grid, cellSize, wallHeight, passageWidth, doorHeight, doorWidth);
        meshFilter.mesh = mazeMesh;
        meshCollider.sharedMesh = mazeMesh;

        PlacePlayerStartPosition();
        Debug.Log("Maze Generation Complete.");
    }

    void InitializeGrid()
    {
        grid = new MazeCell[gridCols, gridRows];
        roomCells.Clear();
        for (int x = 0; x < gridCols; x++)
        {
            for (int y = 0; y < gridRows; y++)
            {
                grid[x, y] = new MazeCell(x, y);
            }
        }
    }


    void PlaceRooms()
    {
        for (int x = 0; x < gridCols; x++)
        {
            for (int y = 0; y < gridRows; y++)
            {
                // Reset flags just in case (important if regenerating)
                grid[x, y].ResetFlags();
                if (Random.value < roomProbability)
                {
                    PlaceRoomInCell(grid[x, y]);
                }
            }
        }
        // Optional: Ensure at least one room if probability failed
        if (roomCells.Count == 0 && gridRows > 0 && gridCols > 0)
        {
            Debug.LogWarning("Forcing at least one room.");
            PlaceRoomInCell(grid[Random.Range(0, gridCols), Random.Range(0, gridRows)]);
        }
    }

    void PlaceRoomInCell(MazeCell cell)
    {
        cell.HasRoom = true;
        float minSize = cellSize * minRoomSizeFactor;
        float maxSize = cellSize * maxRoomSizeFactor;
        float roomWidth = Random.Range(minSize, maxSize);
        float roomHeight = Random.Range(minSize, maxSize);
        float roomX = (cellSize - roomWidth) / 2f;
        float roomY = (cellSize - roomHeight) / 2f;
        cell.RoomRect = new Rect(roomX, roomY, roomWidth, roomHeight);
        roomCells.Add(cell);
    }

    // --- Connectivity using Breadth-First Search (BFS) ---
    void EnsureConnectivityBFS()
    {
        if (roomCells.Count <= 1) return;

        // Reset visited status and passage flags for all cells
        foreach (var cell in grid)
        {
            cell.ResetFlags(); // Reset visited, passages, IsPassage, ParentCell
        }

        Queue<MazeCell> frontier = new Queue<MazeCell>();
        List<MazeCell> reachedRooms = new List<MazeCell>();

        // Start BFS from a random room
        MazeCell startCell = roomCells[Random.Range(0, roomCells.Count)];
        startCell.Visited = true;
        startCell.IsPassage = true; // The starting room cell is part of the maze structure
        frontier.Enqueue(startCell);
        reachedRooms.Add(startCell);

        while (frontier.Count > 0)
        {
            MazeCell currentCell = frontier.Dequeue();

            // Explore neighbours (N, S, E, W)
            List<MazeCell> neighbours = GetShuffledNeighbours(currentCell);

            foreach (MazeCell neighbourCell in neighbours)
            {
                if (!neighbourCell.Visited)
                {
                    neighbourCell.Visited = true;
                    neighbourCell.ParentCell = currentCell; // Track path for potential later use
                    CreatePassage(currentCell, neighbourCell); // Mark passage flags
                    neighbourCell.IsPassage = true; // Mark this cell as part of the passage network

                    frontier.Enqueue(neighbourCell);

                    if (neighbourCell.HasRoom)
                    {
                        reachedRooms.Add(neighbourCell);
                    }
                }
            }
        }

        // --- Check if all rooms were reached and connect orphans if necessary ---
        if (reachedRooms.Count < roomCells.Count)
        {
            ConnectOrphanRooms(reachedRooms);
        }
    }

    // Gets valid neighbours in random order
    List<MazeCell> GetShuffledNeighbours(MazeCell cell)
    {
        List<MazeCell> neighbours = new List<MazeCell>();
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = cell.X + dx[i];
            int ny = cell.Y + dy[i];
            if (IsValidCell(nx, ny))
            {
                neighbours.Add(grid[nx, ny]);
            }
        }
        // Shuffle
        System.Random rng = new System.Random();
        return neighbours.OrderBy(a => rng.Next()).ToList();
    }


    // --- Fallback to connect rooms not reached by the initial BFS ---
    void ConnectOrphanRooms(List<MazeCell> currentlyConnectedRooms)
    {
        List<MazeCell> orphanRooms = roomCells.Except(currentlyConnectedRooms).ToList();
        Debug.LogWarning($"Initial BFS didn't connect all rooms. Found {orphanRooms.Count} orphans. Attempting connection...");

        // Get all cells that are part of the main connected component (rooms + passages)
        List<MazeCell> mainComponentCells = grid.Cast<MazeCell>().Where(c => c.IsPassage && c.Visited).ToList();

        if (mainComponentCells.Count == 0)
        {
            Debug.LogError("Cannot connect orphans: Main component is empty. This shouldn't happen.");
            return;
        }

        foreach (var orphan in orphanRooms)
        {
            if (orphan.Visited) continue; // Should not happen if logic is correct, but safety check

            MazeCell closestMainCell = null;
            float minDistance = float.MaxValue;

            // Find the closest cell in the main connected component to this orphan
            foreach (var mainCell in mainComponentCells)
            {
                float dist = Vector2Int.Distance(orphan.GridPosition, mainCell.GridPosition);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestMainCell = mainCell;
                }
            }

            if (closestMainCell != null)
            {
                Debug.Log($"Connecting orphan at ({orphan.X},{orphan.Y}) to closest main cell at ({closestMainCell.X},{closestMainCell.Y})");
                // Perform a simple pathfinding (BFS) from orphan to closestMainCell to carve the path
                PathfindAndConnect(orphan, closestMainCell);
            }
            else
            {
                Debug.LogError($"Could not find a cell in the main component to connect orphan at ({orphan.X},{orphan.Y})");
            }
        }
    }

    // Simple BFS for pathfinding between two cells on the grid
    void PathfindAndConnect(MazeCell startCell, MazeCell endCell)
    {
        Queue<MazeCell> pathFrontier = new Queue<MazeCell>();
        Dictionary<MazeCell, MazeCell> cameFrom = new Dictionary<MazeCell, MazeCell>(); // Path reconstruction

        // Reset temporary visited flags for this specific pathfind
        // We use a separate dictionary or temporary flag to not interfere with main 'Visited'
        HashSet<MazeCell> pathVisited = new HashSet<MazeCell>();

        pathFrontier.Enqueue(startCell);
        pathVisited.Add(startCell);
        cameFrom[startCell] = null;

        MazeCell current = null;
        bool foundPath = false;

        while (pathFrontier.Count > 0)
        {
            current = pathFrontier.Dequeue();

            if (current == endCell)
            {
                foundPath = true;
                break; // Found the target
            }

            // Explore neighbours (using non-shuffled might be faster for pathfinding)
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                int nx = current.X + dx[i];
                int ny = current.Y + dy[i];
                if (IsValidCell(nx, ny))
                {
                    MazeCell neighbour = grid[nx, ny];
                    if (!pathVisited.Contains(neighbour)) // Check if visited in *this* path search
                    {
                        pathVisited.Add(neighbour);
                        cameFrom[neighbour] = current; // Track path
                        pathFrontier.Enqueue(neighbour);
                    }
                }
            }
        }

        // Reconstruct and carve path if found
        if (foundPath && current != null)
        {
            MazeCell pathCell = current; // Start from the end cell
            while (pathCell != null && cameFrom.ContainsKey(pathCell) && cameFrom[pathCell] != null)
            {
                MazeCell previousCell = cameFrom[pathCell];
                CreatePassage(pathCell, previousCell); // Carve passage
                pathCell.IsPassage = true; // Mark cells on path
                pathCell.Visited = true; // Also mark as globally visited now
                previousCell.IsPassage = true;
                previousCell.Visited = true;
                pathCell = previousCell;
            }
            startCell.IsPassage = true; // Ensure start is marked too
            startCell.Visited = true;
        }
        else
        {
            Debug.LogError($"Pathfinding failed between ({startCell.X},{startCell.Y}) and ({endCell.X},{endCell.Y})");
        }
    }


    bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < gridCols && y >= 0 && y < gridRows;
    }

    void CreatePassage(MazeCell cell1, MazeCell cell2)
    {
        // Mark passage flags on both cells
        if (cell1.X == cell2.X)
        { // Vertical
            if (cell1.Y < cell2.Y) { cell1.PassageN = true; cell2.PassageS = true; }
            else { cell1.PassageS = true; cell2.PassageN = true; }
        }
        else
        { // Horizontal
            if (cell1.X < cell2.X) { cell1.PassageE = true; cell2.PassageW = true; }
            else { cell1.PassageW = true; cell2.PassageE = true; }
        }
    }

    void PlacePlayerStartPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && roomCells.Count > 0)
        {
            MazeCell startRoom = roomCells[Random.Range(0, roomCells.Count)];
            Vector3 roomWorldCenter = new Vector3(
                (startRoom.X * cellSize) + startRoom.RoomRect.center.x,
                1.0f, // Place slightly above floor level
                (startRoom.Y * cellSize) + startRoom.RoomRect.center.y // Note: Using Y grid for Z world axis
            );

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            player.transform.position = roomWorldCenter;
            if (controller != null) controller.enabled = true;

            Debug.Log($"Player start position set in room at grid ({startRoom.X},{startRoom.Y})");
        }
        else if (player == null) Debug.LogWarning("Player object with tag 'Player' not found.");
        else Debug.LogWarning("No rooms generated, cannot set player start.");
    }
}