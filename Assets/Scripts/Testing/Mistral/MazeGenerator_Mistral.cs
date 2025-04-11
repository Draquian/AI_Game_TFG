using UnityEngine;

public class MazeGenerator_Mistral : MonoBehaviour
{
    public int gridSize = 3;
    private GameObject[,] rooms;

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
            gridSize = Mathf.Max(1, gridSize - 1);
            GenerateMaze();
        }

        Debug.Log(gridSize);
    }

    void GenerateMaze()
    {
        if (rooms != null)
        {
            for (int i = 0; i < rooms.GetLength(0); i++)
            {
                for (int j = 0; j < rooms.GetLength(1); j++)
                {
                    if (rooms[i, j] != null)
                    {
                        Destroy(rooms[i, j]);
                    }
                }
            }
        }

        rooms = new GameObject[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                rooms[x, y] = CreateRoom(x, y);
            }
        }

        ConnectRooms();
    }

    GameObject CreateRoom(int x, int y)
    {
        GameObject room = GameObject.CreatePrimitive(PrimitiveType.Cube);
        room.transform.position = new Vector3(x * 10, 0, y * 10);
        room.transform.localScale = new Vector3(Random.Range(5, 10), 1, Random.Range(5, 10));
        room.AddComponent<BoxCollider>();
        return room;
    }

    void ConnectRooms()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (x > 0)
                {
                    CreatePassage(rooms[x, y], rooms[x - 1, y]);
                }
                if (y > 0)
                {
                    CreatePassage(rooms[x, y], rooms[x, y - 1]);
                }
            }
        }
    }

    void CreatePassage(GameObject room1, GameObject room2)
    {
        Vector3 room1Position = room1.transform.position;
        Vector3 room2Position = room2.transform.position;
        Vector3 passagePosition = (room1Position + room2Position) / 2;

        GameObject passage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        passage.transform.position = passagePosition;
        passage.transform.localScale = new Vector3(1, 1, 1);
        passage.AddComponent<BoxCollider>();
    }
}
