using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator_Grok : MonoBehaviour
{
    // Configuration variables
    public int initialGridSize = 3;      // Starting grid size (3x3)
    public float cellSize = 10f;         // Size of each grid cell in Unity units
    public float wallHeight = 5f;        // Height of walls
    public float roomChance = 0.7f;      // Probability a cell has a room (0 to 1)

    // Runtime variables
    private int currentGridSize;
    private bool[,] hasRoom;             // Tracks which cells have rooms
    private bool[,] verticalWalls;       // Walls between cells horizontally
    private bool[,] horizontalWalls;     // Walls between cells vertically
    private bool[,] visited;             // For maze generation algorithm
    private List<Wall> candidateList;    // Walls to consider for passages
    private Mesh mesh;

    // Wall struct to represent potential passages
    private struct Wall
    {
        public int i, j;
        public bool isVertical;
        public Wall(int i, int j, bool isVertical)
        {
            this.i = i;
            this.j = j;
            this.isVertical = isVertical;
        }
    }

    void Start()
    {
        currentGridSize = initialGridSize;
        GenerateMaze();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            currentGridSize++;
            GenerateMaze();
        }
    }

    void GenerateMaze()
    {
        // Initialize arrays based on current grid size
        hasRoom = new bool[currentGridSize, currentGridSize];
        verticalWalls = new bool[currentGridSize - 1, currentGridSize];
        horizontalWalls = new bool[currentGridSize, currentGridSize - 1];
        visited = new bool[currentGridSize, currentGridSize];
        candidateList = new List<Wall>();

        // Step 1: Assign rooms to cells with probability, ensure at least one room
        bool hasAtLeastOneRoom = false;
        for (int x = 0; x < currentGridSize; x++)
        {
            for (int y = 0; y < currentGridSize; y++)
            {
                hasRoom[x, y] = Random.value < roomChance;
                if (hasRoom[x, y]) hasAtLeastOneRoom = true;
            }
        }
        if (!hasAtLeastOneRoom) hasRoom[0, 0] = true; // Ensure at least one room

        // Step 2: Initialize all walls as present
        for (int i = 0; i < currentGridSize - 1; i++)
            for (int j = 0; j < currentGridSize; j++)
                verticalWalls[i, j] = true;
        for (int i = 0; i < currentGridSize; i++)
            for (int j = 0; j < currentGridSize - 1; j++)
                horizontalWalls[i, j] = true;

        // Step 3: Connect all rooms using Prim's algorithm
        // Find first room as starting point
        int startX = -1, startY = -1;
        for (int x = 0; x < currentGridSize && startX == -1; x++)
            for (int y = 0; y < currentGridSize && startY == -1; y++)
                if (hasRoom[x, y])
                {
                    startX = x;
                    startY = y;
                    break;
                }

        visited[startX, startY] = true;
        AddCandidateWalls(startX, startY);

        while (candidateList.Count > 0)
        {
            int index = Random.Range(0, candidateList.Count);
            Wall wall = candidateList[index];
            candidateList.RemoveAt(index);

            int ax, ay, bx, by;
            if (wall.isVertical)
            {
                ax = wall.i; ay = wall.j;
                bx = wall.i + 1; by = wall.j;
            }
            else
            {
                ax = wall.i; ay = wall.j;
                bx = wall.i; by = wall.j + 1;
            }

            // Check if this wall connects a visited room to an unvisited room
            bool connectsRooms = (hasRoom[ax, ay] || hasRoom[bx, by]) &&
                               (visited[ax, ay] != visited[bx, by]);
            if (connectsRooms)
            {
                // Remove wall to create passage
                if (wall.isVertical)
                    verticalWalls[wall.i, wall.j] = false;
                else
                    horizontalWalls[wall.i, wall.j] = false;

                // Mark unvisited cell as visited and add its walls
                if (!visited[ax, ay])
                {
                    visited[ax, ay] = true;
                    AddCandidateWalls(ax, ay);
                }
                else if (!visited[bx, by])
                {
                    visited[bx, by] = true;
                    AddCandidateWalls(bx, by);
                }
            }
        }

        // Step 4: Generate the 3D mesh
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Add floors for all cells (rooms and passages)
        for (int x = 0; x < currentGridSize; x++)
        {
            for (int y = 0; y < currentGridSize; y++)
            {
                float cx = x * cellSize;
                float cz = y * cellSize;
                Vector3 v0 = new Vector3(cx, 0, cz);
                Vector3 v1 = new Vector3(cx + cellSize, 0, cz);
                Vector3 v2 = new Vector3(cx + cellSize, 0, cz + cellSize);
                Vector3 v3 = new Vector3(cx, 0, cz + cellSize);
                AddQuad(vertices, normals, triangles, v0, v1, v2, v3, Vector3.up);
            }
        }

        // Add vertical walls (boundary and inner)
        for (int j = 0; j < currentGridSize; j++)
        {
            float z0 = j * cellSize;
            float z1 = (j + 1) * cellSize;
            // Left boundary
            AddWallVertical(vertices, normals, triangles, 0, z0, z1, wallHeight);
            // Right boundary
            AddWallVertical(vertices, normals, triangles, currentGridSize * cellSize, z0, z1, wallHeight);
        }
        for (int i = 0; i < currentGridSize - 1; i++)
        {
            for (int j = 0; j < currentGridSize; j++)
            {
                if (verticalWalls[i, j])
                {
                    float posX = (i + 1) * cellSize;
                    float z0 = j * cellSize;
                    float z1 = (j + 1) * cellSize;
                    AddWallVertical(vertices, normals, triangles, posX, z0, z1, wallHeight);
                }
            }
        }

        // Add horizontal walls (boundary and inner)
        for (int i = 0; i < currentGridSize; i++)
        {
            float x0 = i * cellSize;
            float x1 = (i + 1) * cellSize;
            // Bottom boundary
            AddWallHorizontal(vertices, normals, triangles, x0, x1, 0, wallHeight);
            // Top boundary
            AddWallHorizontal(vertices, normals, triangles, x0, x1, currentGridSize * cellSize, wallHeight);
        }
        for (int i = 0; i < currentGridSize; i++)
        {
            for (int j = 0; j < currentGridSize - 1; j++)
            {
                if (horizontalWalls[i, j])
                {
                    float x0 = i * cellSize;
                    float x1 = (i + 1) * cellSize;
                    float posZ = (j + 1) * cellSize;
                    AddWallHorizontal(vertices, normals, triangles, x0, x1, posZ, wallHeight);
                }
            }
        }

        // Step 5: Create and assign the mesh
        if (mesh == null) mesh = new Mesh();
        else mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();

        // Add and configure components programmatically
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (meshRenderer.material == null)
            meshRenderer.material = new Material(Shader.Find("Standard"));

        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    void AddCandidateWalls(int x, int y)
    {
        if (x > 0 && (hasRoom[x - 1, y] || hasRoom[x, y]))
            candidateList.Add(new Wall(x - 1, y, true));
        if (x < currentGridSize - 1 && (hasRoom[x + 1, y] || hasRoom[x, y]))
            candidateList.Add(new Wall(x, y, true));
        if (y > 0 && (hasRoom[x, y - 1] || hasRoom[x, y]))
            candidateList.Add(new Wall(x, y - 1, false));
        if (y < currentGridSize - 1 && (hasRoom[x, y + 1] || hasRoom[x, y]))
            candidateList.Add(new Wall(x, y, false));
    }

    void AddQuad(List<Vector3> vertices, List<Vector3> normals, List<int> triangles,
                 Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
    {
        int start = vertices.Count;
        vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
        normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);
        triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
        triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 3);
    }

    void AddWallVertical(List<Vector3> vertices, List<Vector3> normals, List<int> triangles,
                         float posX, float startZ, float endZ, float height)
    {
        Vector3 v0 = new Vector3(posX, 0, startZ);
        Vector3 v1 = new Vector3(posX, 0, endZ);
        Vector3 v2 = new Vector3(posX, height, endZ);
        Vector3 v3 = new Vector3(posX, height, startZ);
        AddQuad(vertices, normals, triangles, v0, v1, v2, v3, Vector3.left);  // Faces left
        AddQuad(vertices, normals, triangles, v3, v2, v1, v0, Vector3.right); // Faces right
    }

    void AddWallHorizontal(List<Vector3> vertices, List<Vector3> normals, List<int> triangles,
                           float startX, float endX, float posZ, float height)
    {
        Vector3 v0 = new Vector3(startX, 0, posZ);
        Vector3 v1 = new Vector3(endX, 0, posZ);
        Vector3 v2 = new Vector3(endX, height, posZ);
        Vector3 v3 = new Vector3(startX, height, posZ);
        AddQuad(vertices, normals, triangles, v0, v1, v2, v3, Vector3.back);  // Faces down
        AddQuad(vertices, normals, triangles, v3, v2, v1, v0, Vector3.forward); // Faces up
    }
}