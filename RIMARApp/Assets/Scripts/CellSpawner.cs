using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CellSpawner : MonoBehaviour
{
    public ARTrackedImageManager imageManager;

    // Assign different prefabs for each QR code in the inspector
    public GameObject prefabQR1;
    public GameObject prefabQR2;
    public GameObject prefabQR3;

    private float gridWidth = 2f;    // Width of the grid in meters
    private float gridHeight = 1f;   // Height of the grid in meters
    private float cellSize = 0.025f; // 2.5cm

    // Track which QR codes have already spawned their grid
    private HashSet<string> spawnedQRs = new HashSet<string>();

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
            string qrName = trackedImage.referenceImage.name;

            if (!spawnedQRs.Contains(qrName))
            {
                SpawnGridForQR(trackedImage);
                spawnedQRs.Add(qrName);
            }
        }
    }

    void SpawnGridForQR(ARTrackedImage trackedImage)
    {
        string qrName = trackedImage.referenceImage.name;
        Debug.Log($"[CellSpawner] Spawning grid for QR: {qrName}");

        int columns = Mathf.RoundToInt(gridWidth / cellSize);
        int rows = Mathf.RoundToInt(gridHeight / cellSize);

        Vector3 startPosition = trackedImage.transform.position;
        Vector3 xDirection = trackedImage.transform.right;
        Vector3 zDirection = trackedImage.transform.forward;

        GameObject selectedPrefab = null;

        // Select prefab/content based on QR name
        switch (qrName)
        {
            case "QR_One":
                selectedPrefab = prefabQR1;
                break;

            case "QR_Two":
                selectedPrefab = prefabQR2;
                break;

            case "QR_Three":
                selectedPrefab = prefabQR3;
                break;

            default:
                Debug.LogWarning($"[CellSpawner] Unknown QR code: {qrName}, skipping spawn.");
                return;
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning($"[CellSpawner] Prefab for {qrName} is not assigned.");
            return;
        }

        // Spawn the grid
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 spawnPos = startPosition + (xDirection * x * cellSize) + (zDirection * z * cellSize);

                // Instantiate the selected prefab
                GameObject obj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

                // Optional: scale objects to fit the grid
                obj.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
            }
        }

        Debug.Log($"[CellSpawner] Grid spawned for {qrName}: {columns}x{rows} = {columns * rows} objects");
    }
}
