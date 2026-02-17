using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;


public class GridSpawner : MonoBehaviour
{
    public ARTrackedImageManager imageManager;

    public GameObject cubePrefab;

    private float marquetteWidth = 2f; // 2 meters
    private float marquetteHeight = 1f; // 1 meter
    private float cellSize = 0.025f; // 2.5cm

    private bool gridSpawned = false;


    private void OnEnable()
    {
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }


    private void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }


    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            Debug.Log($"[GridSpawner] QR Code Detected: {trackedImage.referenceImage.name}");
            
            if (!gridSpawned)
            {
                SpawnGridFromCorner(trackedImage);
                gridSpawned = true;
            }
        }
    }


    void SpawnGridFromCorner(ARTrackedImage trackedImage)
    {
        string qrName = trackedImage.referenceImage.name;

        Debug.Log($"[GridSpawner] Starting grid spawn from QR: {qrName}");

        int columns = Mathf.RoundToInt(marquetteWidth / cellSize);
        int rows = Mathf.RoundToInt(marquetteHeight /  cellSize);

        Vector3 startPosition = trackedImage.transform.position;

        Color quadrantColor = Color.white;

        Vector3 xDirection = trackedImage.transform.right;
        Vector3 zDirection = trackedImage.transform.forward;

        if (qrName == "QR_TopLeft" || qrName.Contains("TopLeft") || qrName.Contains("RED"))
        {
            quadrantColor = Color.red;
            Debug.Log("[GridSpawner] Detected TopLeft (RED) corner");
        }
        else if (qrName == "QR_TopRight" || qrName.Contains("TopRight") || qrName.Contains("GREEN"))
        {
            quadrantColor = Color.green;
            xDirection = -xDirection;
            Debug.Log("[GridSpawner] Detected TopRight (GREEN) corner");
        }
        else if (qrName == "QR_BottomLeft" || qrName.Contains("BottomLeft") || qrName.Contains("BLUE"))
        {
            quadrantColor = Color.blue;
            zDirection = -zDirection;
            Debug.Log("[GridSpawner] Detected BottomLeft (BLUE) corner");
        }
        else if (qrName == "QR_BottomRight" || qrName.Contains("BottomRight") || qrName.Contains("YELLOW"))
        {
            quadrantColor = Color.yellow;
            xDirection = -xDirection;
            zDirection = -zDirection;
            Debug.Log("[GridSpawner] Detected BottomRight (YELLOW) corner");
        }
        else
        {
            Debug.LogWarning($"[GridSpawner] Unknown QR code name: {qrName}. Using default white color.");
        }

        for (int x = 0; x < columns; x++)
        {
            for (int z =0; z < rows; z++)
            {
                Vector3 spawnPosition =
                    startPosition +
                    (xDirection * x * cellSize) +
                    (zDirection * z * cellSize);

                GameObject cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

                cube.transform.localScale = new Vector3(cellSize, 0.002f, cellSize);

                if (x < columns / 2 && z < rows / 2)
                {
                    cube.GetComponent<Renderer>().material.color = quadrantColor;
                }
            }
        }
        
        Debug.Log($"[GridSpawner] Grid spawned! {columns}x{rows} = {columns * rows} cubes");
    }
}
