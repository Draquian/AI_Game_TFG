using UnityEngine;

public static class MazeUtils
{
    /// <summary>
    /// Creates a new cube primitive with a name, position, scale, parent, and color.
    /// Optionally, you can set its collider as trigger.
    /// </summary>
    public static GameObject CreatePrimitive(string name, Vector3 position, Vector3 scale, Transform parent, Color color, bool isTrigger = false)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.parent = parent;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = color;
        }
        Collider col = obj.GetComponent<Collider>();
        if (col != null && col is BoxCollider)
        {
            ((BoxCollider)col).isTrigger = isTrigger;
        }
        return obj;
    }

    /// <summary>
    /// Carves a door hole in the given wall. The original wall is destroyed and replaced by three pieces:
    /// a top piece and two side pieces. Depending on the direction “East”, “West” (or “North”, “South”)
    /// the carving is done along the Z axis (or X axis).
    /// </summary>
    public static GameObject CarveDoorHole(GameObject wall, string direction, Vector3 center, float dimension,
                                             float floorThickness, float wallThickness, float wallHeight,
                                             float doorWidth, float doorHeight, Transform parent)
    {
        Vector3 pos = wall.transform.position;
        Vector3 scale = wall.transform.localScale;
        Object.DestroyImmediate(wall);

        // The top piece spans the full wall width (or depth) above the door.
        float topPieceHeight = scale.y - doorHeight;
        Vector3 topPos = pos;
        topPos.y = floorThickness + doorHeight + topPieceHeight / 2;
        GameObject topPiece = CreatePrimitive("Carved_" + direction + "_Top", topPos,
            new Vector3(scale.x, topPieceHeight, scale.z), parent, Color.gray);

        float leftover;
        GameObject leftPiece = null;
        GameObject rightPiece = null;

        if (direction == "East" || direction == "West")
        {
            // Carve along the Z axis. The leftover on each side gets computed.
            leftover = (scale.z - doorWidth) / 2f;
            Vector3 leftPos = pos;
            leftPos.y = floorThickness + doorHeight / 2;
            leftPos.z = pos.z - (doorWidth / 2 + leftover / 2);
            leftPiece = CreatePrimitive("Carved_" + direction + "_Left", leftPos,
                new Vector3(scale.x, doorHeight, leftover), parent, Color.gray);

            Vector3 rightPos = pos;
            rightPos.y = floorThickness + doorHeight / 2;
            rightPos.z = pos.z + (doorWidth / 2 + leftover / 2);
            rightPiece = CreatePrimitive("Carved_" + direction + "_Right", rightPos,
                new Vector3(scale.x, doorHeight, leftover), parent, Color.gray);
        }
        else if (direction == "North" || direction == "South")
        {
            // Carve along the X axis.
            leftover = (scale.x - doorWidth) / 2f;
            Vector3 leftPos = pos;
            leftPos.y = floorThickness + doorHeight / 2;
            leftPos.x = pos.x - (doorWidth / 2 + leftover / 2);
            leftPiece = CreatePrimitive("Carved_" + direction + "_Left", leftPos,
                new Vector3(leftover, doorHeight, scale.z), parent, Color.gray);

            Vector3 rightPos = pos;
            rightPos.y = floorThickness + doorHeight / 2;
            rightPos.x = pos.x + (doorWidth / 2 + leftover / 2);
            rightPiece = CreatePrimitive("Carved_" + direction + "_Right", rightPos,
                new Vector3(leftover, doorHeight, scale.z), parent, Color.gray);
        }
        // For simplicity return the top piece as the representative object.
        return topPiece;
    }

    /// <summary>
    /// Returns a random color between red and yellow.
    /// </summary>
    public static Color GetRandomColor()
    {
        return Color.Lerp(Color.red, Color.yellow, Random.value);
    }
}
