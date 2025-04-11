using UnityEngine;

public static class CorridorGenerator
{
    public static void GenerateVerticalCorridor(MazeCell_Copilot cellA, MazeCell_Copilot cellB, float cellSize,
                                                float floorThickness, float corridorWidth,
                                                float wallThickness, float wallHeight,
                                                float roofThickness, float doorHeightRatio,
                                                float doorThickness)
    {
        // If both cells are empty, nothing extra is needed.
        if (!cellA.hasRoom && !cellB.hasRoom)
            return;

        // Compute door positions.
        Vector3 doorPosA = cellA.GetDoorPositionNorth();
        Vector3 doorPosB = cellB.GetDoorPositionSouth();

        if (cellA.hasRoom && cellB.hasRoom)
        {
            Vector3 corridorCenter = (doorPosA + doorPosB) / 2f;
            corridorCenter.y = floorThickness / 2f;
            float corridorLength = Mathf.Abs(doorPosB.z - doorPosA.z);

            // Corridor floor.
            GameObject corridorFloor = MazeUtils.CreatePrimitive(
                "V_Corridor_Floor_" + cellA.gridX + "_" + cellA.gridY + "_to_" +
                cellB.gridX + "_" + cellB.gridY,
                corridorCenter,
                new Vector3(corridorWidth, floorThickness, corridorLength),
                cellA.transform.parent, Color.gray);

            // Corridor walls.
            float wallY = floorThickness + wallHeight / 2f;
            Vector3 eastWallPos = corridorCenter + new Vector3(corridorWidth / 2f + wallThickness / 2f, 0, 0);
            MazeUtils.CreatePrimitive("V_Corridor_Wall_East", eastWallPos,
                new Vector3(wallThickness, wallHeight, corridorLength),
                cellA.transform.parent, Color.gray);

            Vector3 westWallPos = corridorCenter - new Vector3(corridorWidth / 2f + wallThickness / 2f, 0, 0);
            MazeUtils.CreatePrimitive("V_Corridor_Wall_West", westWallPos,
                new Vector3(wallThickness, wallHeight, corridorLength),
                cellA.transform.parent, Color.gray);

            // Corridor roof.
            Vector3 roofPos = corridorCenter + new Vector3(0, floorThickness + wallHeight + roofThickness / 2f, 0);
            MazeUtils.CreatePrimitive("V_Corridor_Roof", roofPos,
                new Vector3(corridorWidth + 2 * wallThickness, roofThickness, corridorLength + 2 * wallThickness),
                cellA.transform.parent, Color.gray);

            // Door creation on both sides.
            float doorHeight = wallHeight * doorHeightRatio;
            float doorWidth = corridorWidth;
            float doorCenterY = floorThickness + doorHeight / 2f;

            // For the lower cell (cellA).
            Vector3 doorObjPosA = new Vector3(cellA.center.x, doorCenterY,
                cellA.center.z + cellA.roomDepth / 2f + wallThickness / 2f);
            MazeUtils.CreatePrimitive("Door_North_" + cellA.gridX + "_" + cellA.gridY,
                doorObjPosA, new Vector3(doorWidth, doorHeight, doorThickness),
                cellA.transform.parent, Color.blue, true);
            cellA.CarveDoorHoleInWall("North", doorWidth, doorHeight);

            // For the upper cell (cellB).
            Vector3 doorObjPosB = new Vector3(cellB.center.x, doorCenterY,
                cellB.center.z - cellB.roomDepth / 2f - wallThickness / 2f);
            MazeUtils.CreatePrimitive("Door_South_" + cellB.gridX + "_" + cellB.gridY,
                doorObjPosB, new Vector3(doorWidth, doorHeight, doorThickness),
                cellB.transform.parent, Color.blue, true);
            cellB.CarveDoorHoleInWall("South", doorWidth, doorHeight);
        }
        else if (cellA.hasRoom && !cellB.hasRoom)
        {
            // Only cellA is a room: generate door on its north side.
            float doorHeight = wallHeight * doorHeightRatio;
            float doorWidth = corridorWidth;
            float doorCenterY = floorThickness + doorHeight / 2f;
            Vector3 doorObjPosA = new Vector3(cellA.center.x, doorCenterY,
                cellA.center.z + cellA.roomDepth / 2f + wallThickness / 2f);
            MazeUtils.CreatePrimitive("Door_North_" + cellA.gridX + "_" + cellA.gridY,
                doorObjPosA, new Vector3(doorWidth, doorHeight, doorThickness),
                cellA.transform.parent, Color.blue, true);
            cellA.CarveDoorHoleInWall("North", doorWidth, doorHeight);
        }
        else if (!cellA.hasRoom && cellB.hasRoom)
        {
            // Only cellB is a room: generate door on its south side.
            float doorHeight = wallHeight * doorHeightRatio;
            float doorWidth = corridorWidth;
            float doorCenterY = floorThickness + doorHeight / 2f;
            Vector3 doorObjPosB = new Vector3(cellB.center.x, doorCenterY,
                cellB.center.z - cellB.roomDepth / 2f - wallThickness / 2f);
            MazeUtils.CreatePrimitive("Door_South_" + cellB.gridX + "_" + cellB.gridY,
                doorObjPosB, new Vector3(doorWidth, doorHeight, doorThickness),
                cellB.transform.parent, Color.blue, true);
            cellB.CarveDoorHoleInWall("South", doorWidth, doorHeight);
        }
    }


    public static void GenerateHorizontalCorridor(MazeCell_Copilot cellA, MazeCell_Copilot cellB, float cellSize,
                                                float floorThickness, float corridorWidth,
                                                float wallThickness, float wallHeight,
                                                float roofThickness, float doorHeightRatio,
                                                float doorThickness)
    {
        // Case 1: Both cells are empty – no extra corridor geometry needed.
        if (!cellA.hasRoom && !cellB.hasRoom)
            return;

        // Compute door positions. The MazeCell getters already return the cell boundary if empty.
        Vector3 doorPosA = cellA.GetDoorPositionEast();
        Vector3 doorPosB = cellB.GetDoorPositionWest();

        // Case 2: Both cells are rooms. Generate the full corridor.
        if (cellA.hasRoom && cellB.hasRoom)
        {
            Vector3 corridorCenter = (doorPosA + doorPosB) / 2f;
            corridorCenter.y = floorThickness / 2f;
            float corridorLength = Mathf.Abs(doorPosB.x - doorPosA.x);

            // Corridor floor.
            GameObject corridorFloor = MazeUtils.CreatePrimitive(
                "H_Corridor_Floor_" + cellA.gridX + "_" + cellA.gridY + "_to_" +
                cellB.gridX + "_" + cellB.gridY,
                corridorCenter,
                new Vector3(corridorLength, floorThickness, corridorWidth),
                cellA.transform.parent, Color.gray);

            // Corridor walls.
            float wallY = floorThickness + wallHeight / 2f;
            Vector3 northWallPos = corridorCenter + new Vector3(0, 0, corridorWidth / 2f + wallThickness / 2f);
            MazeUtils.CreatePrimitive("H_Corridor_Wall_North", northWallPos,
                new Vector3(corridorLength, wallHeight, wallThickness),
                cellA.transform.parent, Color.gray);

            Vector3 southWallPos = corridorCenter - new Vector3(0, 0, corridorWidth / 2f + wallThickness / 2f);
            MazeUtils.CreatePrimitive("H_Corridor_Wall_South", southWallPos,
                new Vector3(corridorLength, wallHeight, wallThickness),
                cellA.transform.parent, Color.gray);

            // Corridor roof.
            Vector3 roofPos = corridorCenter + new Vector3(0, floorThickness + wallHeight + roofThickness / 2f, 0);
            MazeUtils.CreatePrimitive("H_Corridor_Roof", roofPos,
                new Vector3(corridorLength + 2 * wallThickness, roofThickness, corridorWidth + 2 * wallThickness),
                cellA.transform.parent, Color.gray);

            // Door creation on both sides.
            float doorHeight = wallHeight * doorHeightRatio;
            float doorWidth = corridorWidth;
            float doorCenterY = floorThickness + doorHeight / 2f;

            // Cell A has a room: generate door and carve.
            Vector3 doorObjPosA = new Vector3(cellA.center.x + cellA.roomWidth / 2f + wallThickness / 2f,
                                              doorCenterY, cellA.center.z);
            MazeUtils.CreatePrimitive("Door_East_" + cellA.gridX + "_" + cellA.gridY,
                doorObjPosA, new Vector3(doorThickness, doorHeight, doorWidth),
                cellA.transform.parent, Color.blue, true);
            cellA.CarveDoorHoleInWall("East", doorWidth, doorHeight);

            // Cell B has a room: generate door and carve.
            Vector3 doorObjPosB = new Vector3(cellB.center.x - cellB.roomWidth / 2f - wallThickness / 2f,
                                              doorCenterY, cellB.center.z);
            MazeUtils.CreatePrimitive("Door_West_" + cellB.gridX + "_" + cellB.gridY,
                doorObjPosB, new Vector3(doorThickness, doorHeight, doorWidth),
                cellB.transform.parent, Color.blue, true);
            cellB.CarveDoorHoleInWall("West", doorWidth, doorHeight);
        }
        // Case 3: One cell is a room and the other is empty.
        else if (cellA.hasRoom && !cellB.hasRoom)
        {
            // Only generate door (and door carving) for the room side (cellA).
            float doorHeight = wallHeight * doorHeightRatio;
            float doorWidth = corridorWidth;
            float doorCenterY = floorThickness + doorHeight / 2f;
            Vector3 doorObjPosA = new Vector3(cellA.center.x + cellA.roomWidth / 2f + wallThickness / 2f,
                                              doorCenterY, cellA.center.z);
            MazeUtils.CreatePrimitive("Door_East_" + cellA.gridX + "_" + cellA.gridY,
                doorObjPosA, new Vector3(doorThickness, doorHeight, doorWidth),
                cellA.transform.parent, Color.blue, true);
            cellA.CarveDoorHoleInWall("East", doorWidth, doorHeight);
        }
        else if (!cellA.hasRoom && cellB.hasRoom)
        {
            // Only generate door (and door carving) for the room side (cellB).
            float doorHeight = wallHeight * doorHeightRatio;
            float doorWidth = corridorWidth;
            float doorCenterY = floorThickness + doorHeight / 2f;
            Vector3 doorObjPosB = new Vector3(cellB.center.x - cellB.roomWidth / 2f - wallThickness / 2f,
                                              doorCenterY, cellB.center.z);
            MazeUtils.CreatePrimitive("Door_West_" + cellB.gridX + "_" + cellB.gridY,
                doorObjPosB, new Vector3(doorThickness, doorHeight, doorWidth),
                cellB.transform.parent, Color.blue, true);
            cellB.CarveDoorHoleInWall("West", doorWidth, doorHeight);
        }
    }

}

