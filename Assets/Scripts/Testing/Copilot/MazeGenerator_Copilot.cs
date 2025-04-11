using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator_Copilot : MonoBehaviour
{
    // Maze grid dimensions and cell size.
    private int gridSize = 3;
    private float cellSize = 10f;

    // Floor thickness for room and corridor floors.
    private float floorThickness = 0.5f;

    // Width of corridors connecting cells.
    private float corridorWidth = 2f;

    // Wall and roof parameters.
    private float wallThickness = 0.3f;
    private float wallHeight = 3f;
    private float roofThickness = 0.3f;

    // Door parameters.
    private float doorHeightRatio = 0.7f;
    private float doorThickness = 0.2f; // Used for door object scale.

    // Probability (0 to 1) that a cell contains a room.
    // If false, the cell will only get a passage floor.
    private float roomProbability = 0.7f;

    // Parent container holding the entire maze.
    private GameObject mazeParent;

    // The Room class represents a cell on the grid.
    // If hasRoom is true, the cell gets a room (with a floor, walls, and a roof);
    // Otherwise, it only gets a full-cell passage floor.
    public class Room
    {
        public Vector3 center;   // Center of the cell.
        public float width;      // For room cells, the floor width.
        public float depth;      // For room cells, the floor depth.
        public int gridX;        // Horizontal cell index.
        public int gridY;        // Vertical cell index.
        public bool hasRoom;     // True if this cell contains a room.
        public bool hasPlatform; // True if an empty cell gets a passage floor.
    }

    // A helper class representing an edge between adjacent cells.
    // Edges are horizontal or vertical.
    private class Edge
    {
        public int roomAIndex;
        public int roomBIndex;
        public bool horizontal;  // True if the edge is horizontal.
    }

    // Array to store all cells.
    private Room[] rooms;

    void Start()
    {
        GenerateMaze();
    }

    void Update()
    {
        // Use "N" to increase grid size and regenerate the maze.
        if (Input.GetKeyDown(KeyCode.N))
        {
            gridSize++;
            GenerateMaze();
        }
        // Use "P" to decrease grid size (minimum gridSize = 3) and regenerate.
        else if (Input.GetKeyDown(KeyCode.P))
        {
            if (gridSize > 3)
            {
                gridSize--;
                GenerateMaze();
            }
        }
    }

    void GenerateMaze()
    {
        // Immediately destroy any previous maze.
        if (mazeParent != null)
        {
            DestroyImmediate(mazeParent);
        }
        mazeParent = new GameObject("MazeContainer");

        // Create each cell in the grid.
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
                // Compute the center of the cell.
                room.center = new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2);
                // Randomly decide if the cell gets a room.
                room.hasRoom = (Random.value < roomProbability);
                room.hasPlatform = !room.hasRoom;

                if (room.hasRoom)
                {
                    room.width = Random.Range(minRoomSize, maxRoomSize);
                    room.depth = Random.Range(minRoomSize, maxRoomSize);
                    CreateRoomFloor(room, index);
                    CreateRoomWallsAndRoof(room, index);
                }
                else if (room.hasPlatform)
                {
                    // For empty cells, simply create a passage floor spanning the entire cell.
                    CreateEmptyCellPassage(room, index);
                }
                rooms[index] = room;
            }
        }

        // Build a list of all possible adjacent edges (horizontal and vertical) between cells.
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

        // Shuffle the edge list.
        Shuffle(possibleEdges);

        // Use a union–find (Kruskal) algorithm to form a minimum spanning tree (MST) connecting all cells.
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
    }

    // Union–find helper with path compression.
    int Find(int[] parent, int i)
    {
        if (parent[i] == i)
            return i;
        parent[i] = Find(parent, parent[i]);
        return parent[i];
    }

    // Fisher–Yates shuffle.
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

    // Create a full-cell passage floor for empty cells.
    void CreateEmptyCellPassage(Room room, int index)
    {
        GameObject passage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        passage.name = "EmptyCell_Passage_" + index;
        passage.transform.parent = mazeParent.transform;
        passage.transform.position = new Vector3(room.center.x, floorThickness / 2, room.center.z);
        passage.transform.localScale = new Vector3(cellSize, floorThickness, cellSize);
        passage.GetComponent<Renderer>().material.color = Color.gray;
    }

    // Generate a horizontal corridor connecting two adjacent cells.
    // Door positions are computed based on whether the cell contains a room or not.
    void GenerateHorizontalCorridor(Room roomA, Room roomB)
    {
        Vector3 doorA = new Vector3(roomA.center.x + (roomA.hasRoom ? roomA.width / 2 : cellSize / 2), 0, roomA.center.z);
        Vector3 doorB = new Vector3(roomB.center.x - (roomB.hasRoom ? roomB.width / 2 : cellSize / 2), 0, roomB.center.z);
        Vector3 corridorCenter = (doorA + doorB) / 2;
        corridorCenter.y = floorThickness / 2;
        float corridorLength = Mathf.Abs(doorB.x - doorA.x);

        GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridor.name = "H_Corridor_" + roomA.gridX + "_" + roomA.gridY + "_to_" + roomB.gridX + "_" + roomB.gridY;
        corridor.transform.parent = mazeParent.transform;
        corridor.transform.position = corridorCenter;
        corridor.transform.localScale = new Vector3(corridorLength, floorThickness, corridorWidth);
        corridor.GetComponent<Renderer>().material.color = Color.gray;

        float corridorWallY = floorThickness + wallHeight / 2;
        Vector3 northWallPos = new Vector3(corridorCenter.x, corridorWallY, corridorCenter.z + corridorWidth / 2 + wallThickness / 2);
        GameObject corridorNorthWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorNorthWall.name = corridor.name + "_Wall_North";
        corridorNorthWall.transform.parent = mazeParent.transform;
        corridorNorthWall.transform.position = northWallPos;
        corridorNorthWall.transform.localScale = new Vector3(corridorLength, wallHeight, wallThickness);
        corridorNorthWall.GetComponent<Renderer>().material.color = Color.gray;

        Vector3 southWallPos = new Vector3(corridorCenter.x, corridorWallY, corridorCenter.z - corridorWidth / 2 - wallThickness / 2);
        GameObject corridorSouthWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorSouthWall.name = corridor.name + "_Wall_South";
        corridorSouthWall.transform.parent = mazeParent.transform;
        corridorSouthWall.transform.position = southWallPos;
        corridorSouthWall.transform.localScale = new Vector3(corridorLength, wallHeight, wallThickness);
        corridorSouthWall.GetComponent<Renderer>().material.color = Color.gray;

        Vector3 roofPos = new Vector3(corridorCenter.x, floorThickness + wallHeight + roofThickness / 2, corridorCenter.z);
        GameObject corridorRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorRoof.name = corridor.name + "_Roof";
        corridorRoof.transform.parent = mazeParent.transform;
        corridorRoof.transform.position = roofPos;
        corridorRoof.transform.localScale = new Vector3(corridorLength + wallThickness * 2, roofThickness, corridorWidth + wallThickness * 2);
        corridorRoof.GetComponent<Renderer>().material.color = Color.gray;

        float doorHeight = wallHeight * doorHeightRatio;
        float doorWidth = corridorWidth;
        float doorCenterY = floorThickness + doorHeight / 2;

        if (roomA.hasRoom)
        {
            Vector3 doorPosA = new Vector3(roomA.center.x + roomA.width / 2 + wallThickness / 2, doorCenterY, roomA.center.z);
            GameObject doorObjA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorObjA.name = "Door_East_Room_" + roomA.gridX + "_" + roomA.gridY;
            doorObjA.transform.parent = mazeParent.transform;
            doorObjA.transform.position = doorPosA;
            doorObjA.transform.localScale = new Vector3(doorThickness, doorHeight, doorWidth);
            doorObjA.GetComponent<Renderer>().material.color = Color.blue;
            doorObjA.GetComponent<BoxCollider>().isTrigger = true;
            int indexA = roomA.gridY * gridSize + roomA.gridX;
            CarveDoorHole_East(roomA, indexA, doorWidth, doorHeight);
        }
        if (roomB.hasRoom)
        {
            Vector3 doorPosB = new Vector3(roomB.center.x - roomB.width / 2 - wallThickness / 2, doorCenterY, roomB.center.z);
            GameObject doorObjB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorObjB.name = "Door_West_Room_" + roomB.gridX + "_" + roomB.gridY;
            doorObjB.transform.parent = mazeParent.transform;
            doorObjB.transform.position = doorPosB;
            doorObjB.transform.localScale = new Vector3(doorThickness, doorHeight, doorWidth);
            doorObjB.GetComponent<Renderer>().material.color = Color.blue;
            doorObjB.GetComponent<BoxCollider>().isTrigger = true;
            int indexB = roomB.gridY * gridSize + roomB.gridX;
            CarveDoorHole_West(roomB, indexB, doorWidth, doorHeight);
        }
    }

    // Generate a vertical corridor connecting two adjacent cells.
    void GenerateVerticalCorridor(Room roomA, Room roomB)
    {
        Vector3 doorA = new Vector3(roomA.center.x, 0, roomA.center.z + (roomA.hasRoom ? roomA.depth / 2 : cellSize / 2));
        Vector3 doorB = new Vector3(roomB.center.x, 0, roomB.center.z - (roomB.hasRoom ? roomB.depth / 2 : cellSize / 2));
        Vector3 corridorCenter = (doorA + doorB) / 2;
        corridorCenter.y = floorThickness / 2;
        float corridorLength = Mathf.Abs(doorB.z - doorA.z);

        GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridor.name = "V_Corridor_" + roomA.gridX + "_" + roomA.gridY + "_to_" + roomB.gridX + "_" + roomB.gridY;
        corridor.transform.parent = mazeParent.transform;
        corridor.transform.position = corridorCenter;
        corridor.transform.localScale = new Vector3(corridorWidth, floorThickness, corridorLength);
        corridor.GetComponent<Renderer>().material.color = Color.gray;

        float corridorWallY = floorThickness + wallHeight / 2;
        Vector3 eastWallPos = new Vector3(corridorCenter.x + corridorWidth / 2 + wallThickness / 2, corridorWallY, corridorCenter.z);
        GameObject corridorEastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorEastWall.name = corridor.name + "_Wall_East";
        corridorEastWall.transform.parent = mazeParent.transform;
        corridorEastWall.transform.position = eastWallPos;
        corridorEastWall.transform.localScale = new Vector3(wallThickness, wallHeight, corridorLength);
        corridorEastWall.GetComponent<Renderer>().material.color = Color.gray;

        Vector3 westWallPos = new Vector3(corridorCenter.x - corridorWidth / 2 - wallThickness / 2, corridorWallY, corridorCenter.z);
        GameObject corridorWestWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorWestWall.name = corridor.name + "_Wall_West";
        corridorWestWall.transform.parent = mazeParent.transform;
        corridorWestWall.transform.position = westWallPos;
        corridorWestWall.transform.localScale = new Vector3(wallThickness, wallHeight, corridorLength);
        corridorWestWall.GetComponent<Renderer>().material.color = Color.gray;

        Vector3 roofPos = new Vector3(corridorCenter.x, floorThickness + wallHeight + roofThickness / 2, corridorCenter.z);
        GameObject corridorRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridorRoof.name = corridor.name + "_Roof";
        corridorRoof.transform.parent = mazeParent.transform;
        corridorRoof.transform.position = roofPos;
        corridorRoof.transform.localScale = new Vector3(corridorWidth + wallThickness * 2, roofThickness, corridorLength + wallThickness * 2);
        corridorRoof.GetComponent<Renderer>().material.color = Color.gray;

        float doorHeight = wallHeight * doorHeightRatio;
        float doorWidth = corridorWidth;
        float doorCenterY = floorThickness + doorHeight / 2;

        if (roomA.hasRoom)
        {
            Vector3 doorPosLower = new Vector3(roomA.center.x, doorCenterY, roomA.center.z + roomA.depth / 2 + wallThickness / 2);
            GameObject doorObjLower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorObjLower.name = "Door_North_Room_" + roomA.gridX + "_" + roomA.gridY;
            doorObjLower.transform.parent = mazeParent.transform;
            doorObjLower.transform.position = doorPosLower;
            doorObjLower.transform.localScale = new Vector3(doorWidth, doorHeight, doorThickness);
            doorObjLower.GetComponent<Renderer>().material.color = Color.blue;
            doorObjLower.GetComponent<BoxCollider>().isTrigger = true;
            int indexLower = roomA.gridY * gridSize + roomA.gridX;
            CarveDoorHole_North(roomA, indexLower, doorWidth, doorHeight);
        }
        if (roomB.hasRoom)
        {
            Vector3 doorPosUpper = new Vector3(roomB.center.x, doorCenterY, roomB.center.z - roomB.depth / 2 - wallThickness / 2);
            GameObject doorObjUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorObjUpper.name = "Door_South_Room_" + roomB.gridX + "_" + roomB.gridY;
            doorObjUpper.transform.parent = mazeParent.transform;
            doorObjUpper.transform.position = doorPosUpper;
            doorObjUpper.transform.localScale = new Vector3(doorWidth, doorHeight, doorThickness);
            doorObjUpper.GetComponent<Renderer>().material.color = Color.blue;
            doorObjUpper.GetComponent<BoxCollider>().isTrigger = true;
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
}
