using UnityEngine;

public static class MeshBuilder
{
    public static GameObject CreateQuadRoom(Vector3 position, float width, float height, Quaternion rotation = default)
    {
        GameObject room = new GameObject("Room");
        MeshFilter mf = room.AddComponent<MeshFilter>();
        MeshRenderer mr = room.AddComponent<MeshRenderer>();
        MeshCollider mc = room.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-width/2, 0, -height/2),
            new Vector3(-width/2, 0, height/2),
            new Vector3(width/2, 0, height/2),
            new Vector3(width/2, 0, -height/2),
        };

        int[] tris = { 0, 1, 2, 0, 2, 3 };
        Vector2[] uvs = {
            new Vector2(0, 0), new Vector2(0, 1),
            new Vector2(1, 1), new Vector2(1, 0)
        };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        mf.mesh = mesh;
        mr.material = new Material(Shader.Find("Standard"));
        mc.sharedMesh = mesh;

        room.transform.position = position;
        room.transform.rotation = rotation;

        return room;
    }
}
