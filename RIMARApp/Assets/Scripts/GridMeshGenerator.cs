using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMeshGenerator : MonoBehaviour
{
    public int columns = 6;
    public int rows = 6;
    public float cellSize = 0.025f;

    public float lineWidth = 0.0015f;

    public void GenerateGrid()
    {
        Mesh mesh = new Mesh();

        int vertCount = (columns + 1) * 2 + (rows + 1) * 2;
        Vector3[] vertices = new Vector3[vertCount];
        int[] indices = new int[vertCount];

        float width = columns * cellSize;
        float height = rows * cellSize;

        int v = 0;

        // Vertical lines
        for (int i = 0; i <= columns; i++)
        {
            float x = -width / 2 + i * cellSize;

            vertices[v] = new Vector3(x, 0, -height / 2);
            indices[v] = v;
            v++;

            vertices[v] = new Vector3(x, 0, height / 2);
            indices[v] = v;
            v++;
        }

        // Horizontal lines
        for (int j = 0; j <= rows; j++)
        {
            float z = -height / 2 + j * cellSize;

            vertices[v] = new Vector3(-width / 2, 0, z);
            indices[v] = v;
            v++;

            vertices[v] = new Vector3(width / 2, 0, z);
            indices[v] = v;
            v++;
        }

        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
