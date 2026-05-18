using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;


public class GridSpawner : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARTrackedImageManager imageManager;

    [Header("Prefabs")]
    public GameObject cubePrefab;
    public GameObject locationMarkerPrefab;
    public GameObject locationTextPrefab;

    [Header("Marquette Settings")]
    [SerializeField] private float marquetteWidth;  // 2 meters
    [SerializeField] private float marquetteHeight; // 1 meter
    [SerializeField] private float cellSize;        // 2.5cm
    [SerializeField] private float cubeHeight;      // 0.002f
    [SerializeField] private float qrSize;          // 0.025f | 0.0125f for spawning at the corner of the qr code

    [Header("Vertical Offsets")]
    [SerializeField] private float gridSurfaceOffset = 0.0001f;
    [SerializeField] private float markerHeightOffset = 0.02f;
    [SerializeField] private float textHeightOffset = 0.05f;

    [Header("Location Data")]
    public LocationDatabase locationDatabase;

    private GameObject currentGridParent; // parent all the cubes under one object so can delete them easily.
    private GameObject[,] gridArray;
    private bool hasSpawnedGrid = false;


    public enum MarquetteCorner
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight
    }

    private void OnEnable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }


    private void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }


    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        if (hasSpawnedGrid) return;

        // Do not allow QR scanning while the start screen if open
        if (UIFlowManager.Instance == null || !UIFlowManager.Instance.CanScanQRCode())
            return;
        
        // Try newly added images first
        foreach (var trackedImage in args.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                Debug.Log("Tracked image added: " + trackedImage.referenceImage.name);
                SpawnGrid(trackedImage);
                return;
            }
        }

        // If not yet spawned, also try updated images
        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                Debug.Log("Tracked image updated: " + trackedImage.referenceImage.name);
                SpawnGrid(trackedImage);
                return;
            }
        }
    }


    private void SpawnGrid(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        hasSpawnedGrid = true;
        
        ClearExistingGrid();

        int columns = Mathf.RoundToInt(marquetteWidth / cellSize);
        int rows = Mathf.RoundToInt(marquetteHeight / cellSize);
        gridArray = new GameObject[columns, rows];

        MarquetteCorner scannedCorner = GetScannedCornerFromReferenceName(trackedImage.referenceImage.name);

        Vector3 qrCenter = trackedImage.transform.position;
        Vector3 xDirection = trackedImage.transform.right.normalized;
        Vector3 zDirection = trackedImage.transform.forward.normalized;
        Vector3 upDirection = trackedImage.transform.up.normalized;

        // IMPORTANT:
        // Top QR codes are physically rotated 180 degrees compared to the bottom ones,
        // so flip both axes to keep the grid growing inward across the marquette
        if (scannedCorner == MarquetteCorner.TopLeft || scannedCorner == MarquetteCorner.TopRight)
        {
            xDirection *= -1f;
            zDirection *= -1f;
        }

        float detectedQRWidth = trackedImage.size.x > 0 ? trackedImage.size.x : qrSize;
        float detectedQRHeight = trackedImage.size.y > 0 ? trackedImage.size.y : qrSize;

        // Get the exact QR corner based on the tracked image center
        Vector3 qrCornerWorld = GetQRCodeCornerWorldPosition(
            qrCenter,
            xDirection,
            zDirection,
            detectedQRWidth,
            detectedQRHeight,
            scannedCorner
        );

        // Convert scanned corner to the marquette's true bottom-left
        Vector3 bottomLeftWorld = GetMarquetteBottomLeftWorld(
            qrCornerWorld,
            xDirection,
            zDirection,
            scannedCorner
        );

        Quaternion gridRotation = Quaternion.LookRotation(zDirection, upDirection);

        currentGridParent = new GameObject("GridParent");
        currentGridParent.transform.SetPositionAndRotation(bottomLeftWorld, gridRotation);

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 localCubePos = new Vector3(
                    x * cellSize + cellSize * 0.5f,
                    gridSurfaceOffset + cubeHeight * 0.5f,
                    z * cellSize + cellSize * 0.5f
                );

                GameObject cube = Instantiate(cubePrefab, currentGridParent.transform);
                cube.transform.localPosition = localCubePos;
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(cellSize, cubeHeight, cellSize);

                Renderer cubeRenderer = cube.GetComponent<Renderer>();
                if (cubeRenderer != null)
                    cubeRenderer.enabled = false;

                gridArray[x, z] = cube;
            }
        }

        MapAllLocations();

        UIFlowManager.Instance.OnQRCodeScanned();
        GameManager.Instance.StartGame();

        Debug.Log($"Grid spawned from {scannedCorner} at {bottomLeftWorld}.");
    }


    private MarquetteCorner GetScannedCornerFromReferenceName(string referenceImageName)
    {
        switch (referenceImageName)
        {
            case "BottomLeft":
                return MarquetteCorner.BottomLeft;
            case "BottomRight":
                return MarquetteCorner.BottomRight;
            case "TopLeft":
                return MarquetteCorner.TopLeft;
            case "TopRight":
                return MarquetteCorner.TopRight;
            default:
                Debug.LogWarning($"Unknown reference image name '{referenceImageName}'. Defaulting to BottomLeft.");
                return MarquetteCorner.BottomLeft;
        }
    }


    private Vector3 GetQRCodeCornerWorldPosition(
        Vector3 imageCenter,
        Vector3 xDirection,
        Vector3 zDirection,
        float qrWidth,
        float qrHeight,
        MarquetteCorner qrCorner
    )
    {
        float halfWidth = qrWidth * 0.5f;
        float halfHeight = qrHeight * 0.5f;

        switch (qrCorner)
        {
            case MarquetteCorner.BottomLeft:
                return imageCenter - xDirection * halfWidth - zDirection * halfWidth;

            case MarquetteCorner.BottomRight:
                return imageCenter + xDirection * halfWidth - zDirection * halfHeight;

            case MarquetteCorner.TopLeft:
                return imageCenter - xDirection * halfWidth + zDirection * halfHeight;

            case MarquetteCorner.TopRight:
                return imageCenter + xDirection * halfWidth + zDirection * halfHeight;

            default:
                return imageCenter;
        }
    }


    private Vector3 GetMarquetteBottomLeftWorld(
        Vector3 scannedQRCornerWorld,
        Vector3 xDirection,
        Vector3 zDirection,
        MarquetteCorner scannedCorner
    )
    {
        switch (scannedCorner)
        {
            case MarquetteCorner.BottomLeft:
                return scannedQRCornerWorld;

            case MarquetteCorner.BottomRight:
                return scannedQRCornerWorld - xDirection * marquetteWidth;

            case MarquetteCorner.TopLeft:
                return scannedQRCornerWorld - zDirection * marquetteHeight;

            case MarquetteCorner.TopRight:
                return scannedQRCornerWorld - xDirection * marquetteWidth - zDirection * marquetteHeight;

            default:
                return scannedQRCornerWorld;
        }
    }


    void MapAllLocations()
    {
        if (locationDatabase == null || locationDatabase.locations == null || gridArray == null)
            return;

        foreach (LocationData location in locationDatabase.locations)
        {
            int xIndex = Mathf.RoundToInt(location.x_cm / 2.5f);
            int zIndex = Mathf.RoundToInt(location.z_cm / 2.5f);

            if (xIndex >= 0 && xIndex < gridArray.GetLength(0) &&
                zIndex >= 0 && zIndex < gridArray.GetLength(1))
            {
                GameObject cube = gridArray[xIndex, zIndex];

                if (cube != null)
                {
                    // Restore the magenta colour
                    cube.GetComponent<Renderer>().material.color = Color.magenta;

                    Vector3 markerPosition = cube.transform.position + currentGridParent.transform.up * markerHeightOffset;

                    GameObject marker = Instantiate(
                        locationMarkerPrefab,
                        markerPosition,
                        Quaternion.identity,
                        currentGridParent.transform
                    );

                    ARLocationMarker markerScript = marker.GetComponent<ARLocationMarker>();
                    markerScript.Initialize(location);

                    if (locationTextPrefab != null)
                    {
                        Vector3 textPosition = marker.transform.position + currentGridParent.transform.up * textHeightOffset;

                        GameObject textObj = Instantiate(
                            locationTextPrefab,
                            textPosition,
                            Quaternion.identity,
                            currentGridParent.transform
                        );

                        textObj.transform.localScale = Vector3.one * 0.01f;

                        TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();
                        if (tmp != null)
                        {
                            tmp.text = location.locationName;
                            tmp.fontSize = 10;
                            tmp.color = Color.white;
                        }
                    }
                }
            }
        }
    }


    private void ClearExistingGrid()
    {
        if (currentGridParent != null)
        {
            Destroy(currentGridParent);
            currentGridParent = null;
        }

        gridArray = null;
    }


    // Optional: Call this if you ever want to allow the QR to start a completely new grid session again
    public void ResetGridSpawnState()
    {
        hasSpawnedGrid = false;
        ClearExistingGrid();
    }


    // Optional: Pull full list of cubes
    public List<GameObject> GetAllCubes()
    {
        List<GameObject> cubeList = new List<GameObject>();

        if (gridArray == null)
            return cubeList;

        foreach (GameObject cube in gridArray)
        {
            if (cube != null)
                cubeList.Add(cube);
        }

        return cubeList;
    }
}