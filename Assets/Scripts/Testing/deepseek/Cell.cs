// Cell.cs
using UnityEngine;
using System.Collections.Generic;

public class Cell
{
    public static readonly Dictionary<Direction, Vector2Int> directions = new Dictionary<Direction, Vector2Int>
    {
        { Direction.North, Vector2Int.up },
        { Direction.East, Vector2Int.right },
        { Direction.South, Vector2Int.down },
        { Direction.West, Vector2Int.left }
    };

    public Vector2Int position;
    public HashSet<Direction> connections = new HashSet<Direction>();
    public bool visited;

    private GameObject parent;
    private float floorHeight = 0.1f;
    private float wallHeight = 3f;
    private float wallThickness = 0.3f;

    public Cell(Vector2Int pos)
    {
        position = pos;
        parent = new GameObject($"Cell {pos.x},{pos.y}");
        parent.transform.parent = MazeGenerator_Deep_Seek.Instance.transform;
    }

    public void GenerateMesh()
    {
        CreateFloor();
        CreateWalls();
    }

    private void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.parent = parent.transform;
        floor.transform.localPosition = new Vector3(0, -floorHeight / 2, 0);
        floor.transform.localScale = new Vector3(
            Random.Range(4f, 8f),
            floorHeight,
            Random.Range(4f, 8f)
        );
        floor.name = "Floor";
        floor.GetComponent<MeshRenderer>().material.color = Color.gray;
    }

    private void CreateWalls()
    {
        foreach (var dir in directions)
        {
            if (!connections.Contains(dir.Key) && ShouldCreateWall(dir.Value))
                CreateWall(dir.Key);
        }
    }

    private bool ShouldCreateWall(Vector2Int dir)
    {
        Vector2Int neighborPos = position + dir;
        return MazeGenerator_Deep_Seek.Instance.cells.ContainsKey(neighborPos);
    }

    private void CreateWall(Direction direction)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.parent = parent.transform;
        wall.GetComponent<MeshRenderer>().material.color = Color.red;

        Vector3 pos = Vector3.zero;
        Vector3 scale = Vector3.zero;
        float cellSize = MazeGenerator_Deep_Seek.Instance.cellSize;

        switch (direction)
        {
            case Direction.North:
                pos = new Vector3(0, wallHeight / 2, cellSize / 2);
                scale = new Vector3(cellSize, wallHeight, wallThickness);
                break;
            case Direction.South:
                pos = new Vector3(0, wallHeight / 2, -cellSize / 2);
                scale = new Vector3(cellSize, wallHeight, wallThickness);
                break;
            case Direction.East:
                pos = new Vector3(cellSize / 2, wallHeight / 2, 0);
                scale = new Vector3(wallThickness, wallHeight, cellSize);
                break;
            case Direction.West:
                pos = new Vector3(-cellSize / 2, wallHeight / 2, 0);
                scale = new Vector3(wallThickness, wallHeight, cellSize);
                break;
        }

        wall.transform.localPosition = pos;
        wall.transform.localScale = scale;
        wall.name = $"Wall {direction}";
    }

    public static Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => Direction.North
        };
    }
}

public enum Direction { North, East, South, West }