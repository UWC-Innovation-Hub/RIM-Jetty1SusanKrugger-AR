using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CellSpawner : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager imageManager;

    [Header("Grid Settings")]
    public float gridWidth = 2f;
    public float gridHeight = 1f;
    public float cellSize = 0.05f;

    [Header("Grid Appearance")]
    [Range(0f, 1f)] public float alpha = 0.05f;

    private HashSet<string> spawnedImages = new HashSet<string>();

    private void OnEnable()
    {
        imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var trackedImage in args.added)
        {
            string imageName = trackedImage.referenceImage.name;

            if (!spawnedImages.Contains(imageName))
            {
                SpawnGrid(trackedImage);
                spawnedImages.Add(imageName);
            }
        }
    }

    private void SpawnGrid(ARTrackedImage trackedImage)
    {
        int columns = Mathf.RoundToInt(gridWidth / cellSize);
        int rows = Mathf.RoundToInt(gridHeight / cellSize);

        // Create parent so everything moves with QR
        GameObject gridParent = new GameObject("GridParent");
        gridParent.transform.SetParent(trackedImage.transform);
        gridParent.transform.localPosition = new Vector3(0, 0, 0.75f);
        gridParent.transform.localRotation = Quaternion.identity;

        // Center grid on QR
        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;

        Vector3 startOffset = new Vector3(
            -totalWidth / 2f + cellSize / 2f,
            0f, // slightly above QR so it doesn't clip
            -totalHeight / 2f + cellSize / 2f
        );

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 localPos = startOffset + new Vector3(x * cellSize, 0, z * cellSize);

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // Parent to grid
                cube.transform.SetParent(gridParent.transform);
                cube.transform.localPosition = localPos;
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(cellSize, 0.002f, cellSize);

                // Transparent material
                Renderer rend = cube.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.5f, 1f, 1f, alpha);
                mat.SetFloat("_SurfaceType", 1);
                rend.material = mat;

                Destroy(cube.GetComponent<Collider>());
                cube.tag = "GridCell";
            }
        }

        Debug.Log($"Grid spawned relative to QR: {columns} x {rows}");
    }
}