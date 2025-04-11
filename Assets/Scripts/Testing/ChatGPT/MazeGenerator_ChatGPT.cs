using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator_ChatGPT : MonoBehaviour
{
    public int gridSize = 3;
    public float cellSize = 20f;
    public Vector2 roomMinSize = new Vector2(8, 8);
    public Vector2 roomMaxSize = new Vector2(16, 16);

    private List<GameObject> currentMaze = new List<GameObject>();

    void Start()
    {
        GenerateMaze();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            gridSize++;
            GenerateMaze();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GenerateMaze();
        }
    }

    void GenerateMaze()
    {
        foreach (var obj in currentMaze)
            Destroy(obj);
        currentMaze.Clear();

        RoomData[,] rooms = new RoomData[gridSize, gridSize];
        System.Random rand = new System.Random();

        // Step 1: Generate Rooms
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float width = Random.Range(roomMinSize.x, roomMaxSize.x);
                float height = Random.Range(roomMinSize.y, roomMaxSize.y);
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                GameObject room = MeshBuilder.CreateQuadRoom(position, width, height);
                currentMaze.Add(room);

                rooms[x, y] = new RoomData { center = position, size = new Vector2(width, height), obj = room };
            }
        }

        // Step 2: Connect rooms using Union-Find (ensure full connectivity)
        UnionFind uf = new UnionFind(gridSize * gridSize);
        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.right, Vector2Int.up };

        List<(Vector2Int, Vector2Int)> edges = new List<(Vector2Int, Vector2Int)>();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                foreach (var dir in directions)
                {
                    int nx = x + dir.x, ny = y + dir.y;
                    if (nx < gridSize && ny < gridSize)
                        edges.Add((new Vector2Int(x, y), new Vector2Int(nx, ny)));
                }
            }
        }

        // Randomize edges for randomized MST
        System.Random rng = new System.Random();
        edges.Sort((a, b) => rng.Next(-1, 2));

        foreach (var edge in edges)
        {
            int a = edge.Item1.x + edge.Item1.y * gridSize;
            int b = edge.Item2.x + edge.Item2.y * gridSize;

            if (uf.Find(a) != uf.Find(b))
            {
                uf.Union(a, b);
                CreatePassage(rooms[edge.Item1.x, edge.Item1.y], rooms[edge.Item2.x, edge.Item2.y]);
            }
        }
    }

    void CreatePassage(RoomData a, RoomData b)
    {
        Vector3 dir = b.center - a.center;
        Vector3 mid = (a.center + b.center) / 2;
        float width = 4f;
        float length = dir.magnitude;
        GameObject passage = MeshBuilder.CreateQuadRoom(mid, length, width, Quaternion.LookRotation(dir));
        currentMaze.Add(passage);
    }

    class RoomData
    {
        public Vector3 center;
        public Vector2 size;
        public GameObject obj;
    }

    class UnionFind
    {
        private int[] parent;
        public UnionFind(int size)
        {
            parent = new int[size];
            for (int i = 0; i < size; i++) parent[i] = i;
        }

        public int Find(int x)
        {
            if (parent[x] != x) parent[x] = Find(parent[x]);
            return parent[x];
        }

        public void Union(int x, int y)
        {
            int xr = Find(x);
            int yr = Find(y);
            if (xr != yr) parent[xr] = yr;
        }
    }
}
