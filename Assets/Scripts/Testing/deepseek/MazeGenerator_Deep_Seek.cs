// MazeGenerator.cs
using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator_Deep_Seek : MonoBehaviour
{
    public static MazeGenerator_Deep_Seek Instance;

    private int gridSize = 3;
    public float cellSize = 10f;
    public Dictionary<Vector2Int, Cell> cells = new Dictionary<Vector2Int, Cell>();

    private void Awake() => Instance = this;

    private void Start() => RegenerateMaze();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) { gridSize += 2; RegenerateMaze(); }
        if (Input.GetKeyDown(KeyCode.P)) { gridSize = Mathf.Max(3, gridSize - 2); RegenerateMaze(); }
    }

    public void RegenerateMaze()
    {
        ClearMaze();
        CreateGrid();
        GenerateMaze();
        GenerateMeshes();
    }

    private void ClearMaze()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        cells.Clear();
    }

    private void CreateGrid()
    {
        int halfSize = gridSize / 2;
        for (int x = -halfSize; x <= halfSize; x++)
        {
            for (int y = -halfSize; y <= halfSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                cells[pos] = new Cell(pos);
            }
        }
    }

    private void GenerateMaze()
    {
        Stack<Cell> stack = new Stack<Cell>();
        Cell current = cells[Vector2Int.zero];
        current.visited = true;
        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Cell> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                stack.Push(current);
                Cell neighbor = neighbors[Random.Range(0, neighbors.Count)];
                ConnectCells(current, neighbor);
                neighbor.visited = true;
                stack.Push(neighbor);
            }
        }
    }

    private List<Cell> GetUnvisitedNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();
        foreach (var dir in Cell.directions)
        {
            Vector2Int neighborPos = cell.position + dir.Value;
            if (cells.TryGetValue(neighborPos, out Cell neighbor) && !neighbor.visited)
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    private void ConnectCells(Cell a, Cell b)
    {
        Vector2Int offset = b.position - a.position;
        foreach (var dir in Cell.directions)
        {
            if (dir.Value == offset)
            {
                a.connections.Add(dir.Key);
                b.connections.Add(Cell.GetOppositeDirection(dir.Key));
            }
        }
    }

    private void GenerateMeshes()
    {
        foreach (var cell in cells.Values)
            cell.GenerateMesh();
    }
}