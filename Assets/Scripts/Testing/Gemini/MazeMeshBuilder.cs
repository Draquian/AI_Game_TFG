using UnityEngine;
using System.Collections.Generic;

public static class MazeMeshBuilder
{
    // Mesh data lists
    private static List<Vector3> vertices = new List<Vector3>();
    private static List<int> triangles = new List<int>();
    private static List<Vector2> uvs = new List<Vector2>();

    // Parameters
    private static MazeCell[,] currentGrid;
    private static float currentCellSize;
    private static float currentWallHeight;
    private static float currentPassageWidth;
    private static float currentDoorHeight;
    private static float currentDoorWidth;

    // Tolerance
    private const float Tolerance = 0.001f;

    public static Mesh BuildMesh(MazeCell[,] grid, float cellSize, float wallHeight, float passageWidth, float doorHeight, float doorWidth)
    {
        // Store parameters
        currentGrid = grid;
        currentCellSize = cellSize;
        currentWallHeight = wallHeight;
        currentPassageWidth = passageWidth;
        currentDoorWidth = Mathf.Min(doorWidth, passageWidth - Tolerance * 2);
        currentDoorHeight = Mathf.Clamp(doorHeight, Tolerance, wallHeight - Tolerance);

        // Clear lists
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        // Get grid dimensions for boundary walls later
        int gridCols = grid.GetLength(0);
        int gridRows = grid.GetLength(1);

        // Generate mesh cell by cell
        for (int x = 0; x < gridCols; x++)
        {
            for (int y = 0; y < gridRows; y++)
            {
                GenerateCellMesh(grid[x, y]);
            }
        }

        // *** NEW: Add Outer Boundary Walls ***
        AddBoundaryWalls(gridCols, gridRows, cellSize, wallHeight);

        // Create and return mesh
        Mesh mesh = new Mesh();
        mesh.name = "GeneratedMazeMesh_V4_Enclosed"; // Version update
        if (vertices.Count > 65534)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
        if (vertices.Count > 0)
        {
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        else
        {
            Debug.LogWarning("Generated mesh has no vertices.");
        }
        currentGrid = null;
        return mesh;
    }

    // --- Main Cell Mesh Generation Logic ---
    // (Largely same as V3, focusing on fixes in helpers if needed)
    private static void GenerateCellMesh(MazeCell cell)
    {
        if (!cell.HasRoom && !cell.IsPassage) { return; } // Strict check

        float worldX = cell.X * currentCellSize;
        float worldZ = cell.Y * currentCellSize;
        Vector3 cellOrigin = new Vector3(worldX, 0, worldZ);
        Vector3 cellCenter = cellOrigin + new Vector3(currentCellSize / 2f, 0, currentCellSize / 2f);

        bool passageN = cell.PassageN; bool passageS = cell.PassageS;
        bool passageE = cell.PassageE; bool passageW = cell.PassageW;

        if (cell.HasRoom)
        {
            // --- Generate Room Geometry --- (Same as V3)
            Rect roomRect = cell.RoomRect;
            Vector3 floorBL = cellOrigin + new Vector3(roomRect.xMin, 0, roomRect.yMin);
            Vector3 floorBR = cellOrigin + new Vector3(roomRect.xMax, 0, roomRect.yMin);
            Vector3 floorTL = cellOrigin + new Vector3(roomRect.xMin, 0, roomRect.yMax);
            Vector3 floorTR = cellOrigin + new Vector3(roomRect.xMax, 0, roomRect.yMax);
            AddQuad(floorBL, floorTL, floorTR, floorBR); // Floor
            AddQuad(floorBL + Vector3.up * currentWallHeight, floorBR + Vector3.up * currentWallHeight, floorTR + Vector3.up * currentWallHeight, floorTL + Vector3.up * currentWallHeight); // Roof

            float halfPassage = currentPassageWidth / 2f;
            float roomCenterX = roomRect.center.x; float roomCenterZ = roomRect.center.y;

            Vector3 wallStartN = floorTL; Vector3 wallEndN = floorTR;
            Vector3 passagePointN = cellOrigin + new Vector3(roomCenterX, 0, roomRect.yMax);
            GenerateRoomWallSegment(wallStartN, wallEndN, passagePointN, passageN, cell.X, cell.Y + 1);

            Vector3 wallStartS = floorBR; Vector3 wallEndS = floorBL;
            Vector3 passagePointS = cellOrigin + new Vector3(roomCenterX, 0, roomRect.yMin);
            GenerateRoomWallSegment(wallStartS, wallEndS, passagePointS, passageS, cell.X, cell.Y - 1);

            Vector3 wallStartE = floorTR; Vector3 wallEndE = floorBR;
            Vector3 passagePointE = cellOrigin + new Vector3(roomRect.xMax, 0, roomCenterZ);
            GenerateRoomWallSegment(wallStartE, wallEndE, passagePointE, passageE, cell.X + 1, cell.Y);

            Vector3 wallStartW = floorBL; Vector3 wallEndW = floorTL;
            Vector3 passagePointW = cellOrigin + new Vector3(roomRect.xMin, 0, roomCenterZ);
            GenerateRoomWallSegment(wallStartW, wallEndW, passagePointW, passageW, cell.X - 1, cell.Y);

            if (passageN && currentCellSize - roomRect.yMax > Tolerance) AddPassageFloorRoof(passagePointN, Vector3.forward, halfPassage, currentCellSize - roomRect.yMax);
            if (passageS && roomRect.yMin > Tolerance) AddPassageFloorRoof(passagePointS, Vector3.back, halfPassage, roomRect.yMin);
            if (passageE && currentCellSize - roomRect.xMax > Tolerance) AddPassageFloorRoof(passagePointE, Vector3.right, halfPassage, currentCellSize - roomRect.xMax);
            if (passageW && roomRect.xMin > Tolerance) AddPassageFloorRoof(passagePointW, Vector3.left, halfPassage, roomRect.xMin);
        }
        else // --- Generate Empty Cell Passage Geometry --- (cell.IsPassage is true)
        {
            float halfSize = currentCellSize / 2f;
            float halfPassage = currentPassageWidth / 2f;

            Vector3 centerBL = cellCenter + new Vector3(-halfPassage, 0, -halfPassage);
            Vector3 centerTL = cellCenter + new Vector3(-halfPassage, 0, halfPassage);
            Vector3 centerTR = cellCenter + new Vector3(halfPassage, 0, halfPassage);
            Vector3 centerBR = cellCenter + new Vector3(halfPassage, 0, -halfPassage);

            AddQuad(centerBL, centerTL, centerTR, centerBR); // Center Floor Patch
            AddQuad(centerBL + Vector3.up * currentWallHeight, centerBR + Vector3.up * currentWallHeight, centerTR + Vector3.up * currentWallHeight, centerTL + Vector3.up * currentWallHeight); // Center Roof Patch

            // Generate passage segments from CENTER PATCH EDGE outwards
            // The GeneratePassageSegmentWithWalls ensures side walls connect to the center patch edges.
            if (passageN) GeneratePassageSegmentWithWalls(centerTL, centerTR, Vector3.forward, halfPassage, halfSize - halfPassage);
            if (passageS) GeneratePassageSegmentWithWalls(centerBR, centerBL, Vector3.back, halfPassage, halfSize - halfPassage);
            if (passageE) GeneratePassageSegmentWithWalls(centerTR, centerBR, Vector3.right, halfPassage, halfSize - halfPassage);
            if (passageW) GeneratePassageSegmentWithWalls(centerBL, centerTL, Vector3.left, halfPassage, halfSize - halfPassage);

            // Add solid boundary walls ONLY where passages DO NOT exit.
            // These prevent gaps between generated passage side walls and the cell edge.
            Vector3 northWall_Start = cellOrigin + new Vector3(0, 0, currentCellSize);
            Vector3 northWall_End = cellOrigin + new Vector3(currentCellSize, 0, currentCellSize);
            Vector3 southWall_Start = cellOrigin + new Vector3(currentCellSize, 0, 0);
            Vector3 southWall_End = cellOrigin;
            Vector3 eastWall_Start = cellOrigin + new Vector3(currentCellSize, 0, currentCellSize);
            Vector3 eastWall_End = cellOrigin + new Vector3(currentCellSize, 0, 0);
            Vector3 westWall_Start = cellOrigin;
            Vector3 westWall_End = cellOrigin + new Vector3(0, 0, currentCellSize);

            if (!passageN) AddWallSegment(northWall_Start, northWall_End, currentWallHeight);
            if (!passageS) AddWallSegment(southWall_Start, southWall_End, currentWallHeight);
            if (!passageE) AddWallSegment(eastWall_Start, eastWall_End, currentWallHeight);
            if (!passageW) AddWallSegment(westWall_Start, westWall_End, currentWallHeight);
        }
    }

    // --- NEW Helper: Adds outer boundary walls ---
    private static void AddBoundaryWalls(int gridCols, int gridRows, float cellSize, float wallHeight)
    {
        float minX = 0;
        float maxX = gridCols * cellSize;
        float minZ = 0;
        float maxZ = gridRows * cellSize;

        // Define corners
        Vector3 BL_ground = new Vector3(minX, 0, minZ);
        Vector3 BR_ground = new Vector3(maxX, 0, minZ);
        Vector3 TL_ground = new Vector3(minX, 0, maxZ);
        Vector3 TR_ground = new Vector3(maxX, 0, maxZ);

        // Add Walls (ensure winding order makes them face inwards)
        // North Wall (viewed from South: TL -> TR)
        AddWallSegment(TL_ground, TR_ground, wallHeight);
        // South Wall (viewed from North: BR -> BL)
        AddWallSegment(BR_ground, BL_ground, wallHeight);
        // East Wall (viewed from West: TR -> BR)
        AddWallSegment(TR_ground, BR_ground, wallHeight);
        // West Wall (viewed from East: BL -> TL)
        AddWallSegment(BL_ground, TL_ground, wallHeight);
    }


    // --- Other Helper Functions (Mostly Unchanged from V3) ---

    private static bool NeighbourHasRoom(int neighbourX, int neighbourY)
    {
        // Added boundary checks here for safety, although GenerateRoomWallSegment also passes valid coords.
        if (neighbourX >= 0 && neighbourX < currentGrid.GetLength(0) && neighbourY >= 0 && neighbourY < currentGrid.GetLength(1))
        {
            return currentGrid[neighbourX, neighbourY].HasRoom;
        }
        return false;
    }

    private static void GenerateRoomWallSegment(Vector3 baseStart, Vector3 baseEnd, Vector3 passageCenterPoint, bool hasPassage, int neighbourX, int neighbourY)
    {
        // (Implementation from V3 seems robust, keeping it)
        if (Vector3.Distance(baseStart, baseEnd) < Tolerance) return;
        if (!hasPassage) { AddWallSegment(baseStart, baseEnd, currentWallHeight); }
        else
        {
            bool neighbourIsRoom = NeighbourHasRoom(neighbourX, neighbourY);
            float openingHeight = neighbourIsRoom ? currentDoorHeight : currentWallHeight;
            float halfOpeningWidth = neighbourIsRoom ? currentDoorWidth / 2f : currentPassageWidth / 2f;
            Vector3 wallDir = (baseEnd - baseStart).normalized;
            float segmentLength = Vector3.Distance(baseStart, baseEnd);
            halfOpeningWidth = Mathf.Min(halfOpeningWidth, segmentLength / 2f - Tolerance);
            if (halfOpeningWidth < Tolerance) { AddWallSegment(baseStart, baseEnd, currentWallHeight); return; }
            Vector3 projectedCenter = baseStart + Vector3.Project(passageCenterPoint - baseStart, wallDir);
            Vector3 openingBottomLeft = projectedCenter - wallDir * halfOpeningWidth;
            Vector3 openingBottomRight = projectedCenter + wallDir * halfOpeningWidth;
            if (Vector3.Distance(baseStart, openingBottomLeft) > Tolerance) AddWallSegment(baseStart, openingBottomLeft, currentWallHeight);
            if (Vector3.Distance(openingBottomRight, baseEnd) > Tolerance) AddWallSegment(openingBottomRight, baseEnd, currentWallHeight);
            if (openingHeight < currentWallHeight - Tolerance)
            {
                Vector3 openingTopLeft = openingBottomLeft + Vector3.up * openingHeight;
                Vector3 openingTopRight = openingBottomRight + Vector3.up * openingHeight;
                AddWallSegment(openingTopLeft, openingTopRight, currentWallHeight - openingHeight);
            }
        }
    }

    // Generates Floor, Roof, and side Walls for a passage segment starting from specific edge points
    private static void GeneratePassageSegmentWithWalls(Vector3 startEdgeLeft, Vector3 startEdgeRight, Vector3 dir, float halfWidth, float length)
    {
        // (Implementation from V3 seems robust, keeping it, including vertex order fix)
        if (length < Tolerance) return;
        Vector3 endEdgeLeft = startEdgeLeft + dir * length;
        Vector3 endEdgeRight = startEdgeRight + dir * length;
        Vector3 floor_bl, floor_tl, floor_tr, floor_br;
        // Define vertices based on direction for consistent BL, TL, TR, BR order relative to segment
        if (dir == Vector3.forward) { floor_bl = startEdgeLeft; floor_tl = endEdgeLeft; floor_tr = endEdgeRight; floor_br = startEdgeRight; }
        else if (dir == Vector3.back) { floor_bl = endEdgeRight; floor_tl = startEdgeRight; floor_tr = startEdgeLeft; floor_br = endEdgeLeft; }
        else if (dir == Vector3.right) { floor_bl = startEdgeRight; floor_tl = startEdgeLeft; floor_tr = endEdgeLeft; floor_br = endEdgeRight; }
        else { floor_bl = endEdgeLeft; floor_tl = endEdgeRight; floor_tr = startEdgeRight; floor_br = startEdgeLeft; } // West

        AddQuad(floor_bl, floor_tl, floor_tr, floor_br); // Floor
        AddQuad(floor_bl + Vector3.up * currentWallHeight, floor_br + Vector3.up * currentWallHeight, floor_tr + Vector3.up * currentWallHeight, floor_tl + Vector3.up * currentWallHeight); // Roof (BL, BR, TR, TL)

        // Side Walls - Use the defined floor vertices to ensure alignment
        AddWallSegment(floor_bl, floor_tl, currentWallHeight); // Left wall segment
        AddWallSegment(floor_br, floor_tr, currentWallHeight); // Right wall segment (base order reversed for correct facing)
    }


    // Helper just for Floor/Roof of passage stubs
    private static void AddPassageFloorRoof(Vector3 startCenter, Vector3 dir, float halfWidth, float length)
    {
        // (Implementation from V3 seems robust, keeping it, including vertex order fix)
        if (length < Tolerance) return;
        Vector3 sideOffset = Vector3.Cross(dir, Vector3.up) * halfWidth;
        Vector3 endCenter = startCenter + dir * length;
        Vector3 v_bl, v_tl, v_tr, v_br;
        Vector3 startLeft = startCenter - sideOffset; Vector3 startRight = startCenter + sideOffset;
        Vector3 endLeft = endCenter - sideOffset; Vector3 endRight = endCenter + sideOffset;
        if (dir == Vector3.forward) { v_bl = startLeft; v_tl = endLeft; v_tr = endRight; v_br = startRight; }
        else if (dir == Vector3.back) { v_bl = endRight; v_tl = startRight; v_tr = startLeft; v_br = endLeft; }
        else if (dir == Vector3.right) { v_bl = startRight; v_tl = startLeft; v_tr = endLeft; v_br = endRight; }
        else { v_bl = endLeft; v_tl = endRight; v_tr = startRight; v_br = startLeft; } // West
        AddQuad(v_bl, v_tl, v_tr, v_br); // Floor
        AddQuad(v_bl + Vector3.up * currentWallHeight, v_br + Vector3.up * currentWallHeight, v_tr + Vector3.up * currentWallHeight, v_tl + Vector3.up * currentWallHeight); // Roof
    }


    // Helper to add a simple rectangular wall face
    private static void AddWallSegment(Vector3 baseStart, Vector3 baseEnd, float height)
    {
        if (height < Tolerance || Vector3.Distance(baseStart, baseEnd) < Tolerance) return;
        Vector3 topStart = baseStart + Vector3.up * height;
        Vector3 topEnd = baseEnd + Vector3.up * height;
        AddQuad(baseStart, topStart, topEnd, baseEnd); // CCW order: BL, TL, TR, BR relative to wall face
    }

    // Helper to add a Quad face to the mesh (CCW winding order assumed for standard normal)
    private static void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        if (Vector3.Distance(v1, v2) < Tolerance || Vector3.Distance(v2, v3) < Tolerance || Vector3.Distance(v3, v4) < Tolerance) return;
        int startIndex = vertices.Count;
        vertices.Add(v1); uvs.Add(new Vector2(0, 0));
        vertices.Add(v2); uvs.Add(new Vector2(0, 1));
        vertices.Add(v3); uvs.Add(new Vector2(1, 1));
        vertices.Add(v4); uvs.Add(new Vector2(1, 0));
        triangles.Add(startIndex); triangles.Add(startIndex + 1); triangles.Add(startIndex + 2);
        triangles.Add(startIndex); triangles.Add(startIndex + 2); triangles.Add(startIndex + 3);
    }
}