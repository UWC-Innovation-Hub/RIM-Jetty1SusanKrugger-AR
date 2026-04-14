using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;


public class GridSpawner : MonoBehaviour
{
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

    /*
    [System.Serializable]
    public class LocationPoint
    {
        public string locationName;
        public float x_cm;
        public float z_cm;
    }

    public List<LocationPoint> locations = new List<LocationPoint>();
    */

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
            if (!hasSpawnedGrid)
            {
                SpawnGrid(trackedImage);
                hasSpawnedGrid = true;
            }
        }
    }


    void SpawnGrid(ARTrackedImage trackedImage)
    {
        // Delete old grid if exists
        if (currentGridParent != null)
            Destroy(currentGridParent);

        int columns = Mathf.RoundToInt(marquetteWidth / cellSize);
        int rows = Mathf.RoundToInt(marquetteHeight /  cellSize);

        gridArray = new GameObject[columns, rows];

        Vector3 xDirection = trackedImage.transform.right;
        Vector3 zDirection = trackedImage.transform.forward;
        Vector3 yDirection = trackedImage.transform.up;

        float detectedQRWidth = trackedImage.size.x > 0 ? trackedImage.size.x : qrSize;
        float detectedQRHeight = trackedImage.size.y > 0 ? trackedImage.size.y : qrSize;

        MarquetteCorner scannedCorner = GetScannedCornerFromReferenceName(trackedImage.referenceImage.name);

        // Get the exact world-space corner point of the scanned QR
        Vector3 qrCornerWorld = GetQRCodeCornerWorldPosition(
            trackedImage.transform.position,
            xDirection,
            zDirection,
            detectedQRWidth,
            detectedQRHeight,
            scannedCorner
        );

        // Convert whichever scanned corner it is into the marquette's true bottom-left world point
        Vector3 bottomLeftWorld = GetMarquetteBottomLeftWorld(
            qrCornerWorld,
            xDirection,
            zDirection,
            scannedCorner
        );

        // Create one stable root object in world space
        currentGridParent = new GameObject("GridParent");
        currentGridParent.transform.position = bottomLeftWorld;
        currentGridParent.transform.rotation = trackedImage.transform.rotation;

        // Spawn cubes locally under the parent
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 localCubePos = new Vector3(
                    x * cellSize + cellSize * 0.5f,
                    cubeHeight,
                    z * cellSize + cellSize * 0.5f
                );

                GameObject cube = Instantiate(cubePrefab, currentGridParent.transform);
                cube.transform.localPosition = localCubePos;
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(cellSize, cubeHeight, cellSize);

                Renderer cubeRenderer = cube.GetComponent<Renderer>();
                if (cubeRenderer != null)
                {
                    cubeRenderer.enabled = false;
                }

                gridArray[x, z] = cube;
            }
        }
     
        MapAllLocations();

        // Trigger UI update; Start the experience ONLY ONCE after the full grid has spawned
        UIFlowManager.Instance.OnQRCodeScanned();
        GameManager.Instance.StartGame();

        Debug.Log($"Grid spawned from {scannedCorner} QR. Bottom-left world point: {bottomLeftWorld}");
    }


    private MarquetteCorner GetScannedCornerFromReferenceName(string referenceImageName)
    {
        switch (referenceImageName)
        {
            case "QR_BottomLeft":
                return MarquetteCorner.BottomLeft;

            case "QR_BottomRight":
                return MarquetteCorner.BottomRight;

            case "QR_TopLeft":
                return MarquetteCorner.TopLeft;

            case "QR_TopRight":
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

                    Vector3 markerPosition = cube.transform.position + Vector3.up * 0.001f;

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
                        Vector3 textPosition = marker.transform.position + Vector3.up * 0.05f;

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


    // Optional: Call this if you ever want to allow the QR to start a completely new grid session again
    public void ResetGridSpawnState()
    {
        hasSpawnedGrid = false;

        if (currentGridParent != null)
        {
            Destroy(currentGridParent);
            currentGridParent = null;
        }

        gridArray = null;
    }
}

