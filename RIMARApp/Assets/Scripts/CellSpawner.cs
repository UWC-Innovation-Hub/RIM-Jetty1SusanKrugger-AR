using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CellSpawner : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager imageManager;

    [Header("Grid Settings")]
    public float gridWidth = 2f;   // Width of grid in meters
    public float gridHeight = 1f;  // Height of grid in meters
    public float cellSize = 0.05f; // Size of each cube

    [Header("Grid Appearance")]
    [Range(0f, 1f)] public float alpha = 0.05f; // cube transparency

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

        Vector3 startPos = Vector3.zero; // World origin
        Vector3 xDir = Vector3.right;
        Vector3 zDir = Vector3.forward;

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 spawnPos =
                    startPos +
                    (xDir * x * cellSize) +
                    (zDir * z * cellSize);

                // Create a thin cube
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = spawnPos;
                cube.transform.localScale = new Vector3(cellSize, 0.002f, cellSize);
                cube.transform.rotation = Quaternion.identity;

                // Transparent material
                Renderer rend = cube.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(1f, 1f, 1f, alpha); // white with low alpha
                mat.SetFloat("_SurfaceType", 1); // transparent
                rend.material = mat;

                // Remove collider for performance
                Destroy(cube.GetComponent<Collider>());

                // Add a script or tag to identify this cube as an anchor
                cube.tag = "GridCell";
            }
        }

        Debug.Log($"Grid spawned at world origin: {columns}x{rows} cubes");
    }
}