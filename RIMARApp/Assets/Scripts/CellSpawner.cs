using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

// This script spawns a grid of cubes on top of a detected QR code
// and places fingerprint prefabs on the grid that move smoothly.
public class CellSpawner : MonoBehaviour
{
    // ================================
    // AR COMPONENTS
    // ================================
    [Header("AR Components")]
    public ARTrackedImageManager imageManager; // Reference to ARTrackedImageManager for QR code tracking

    // ================================
    // GRID SETTINGS
    // ================================
    [Header("Grid Settings")]
    public float gridWidth = 2f;       // Total width of the grid
    public float gridHeight = 1f;      // Total height of the grid
    public float cellSize = 0.025f;    // Size of each cube in the grid
    public float gridYOffset = 0.02f;  // Offset above the QR code to place the grid

    // ================================
    // GRID APPEARANCE
    // ================================
    [Header("Grid Appearance")]
    [Range(0f, 1f)]
    public float alpha = 0.05f;        // Transparency of grid cubes

    // ================================
    // FINGERPRINT PREFAB SETTINGS
    // ================================
    [Header("Fingerprint Prefab")]
    public GameObject fingerprintPrefab;      // Assign your fingerprint prefab in the Inspector
    public int fingerprintsCount = 4;         // Number of fingerprints to spawn
    public float minDistanceBetweenPrefabs = 0.05f; // Minimum distance between fingerprints
    public float prefabSmoothTime = 0.1f;     // SmoothDamp time for fingerprint movement

    // ================================
    // PRIVATE VARIABLES
    // ================================
    private GameObject currentGrid = null; // Holds the currently spawned grid
    private string currentQRCode = null;   // Holds the currently detected QR code name
    private Dictionary<GameObject, Vector3> prefabTargets = new Dictionary<GameObject, Vector3>(); // Target positions for fingerprints
    private Dictionary<GameObject, Vector3> prefabVelocities = new Dictionary<GameObject, Vector3>(); // Velocities for SmoothDamp

    // Subscribe to tracked image events
    private void OnEnable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    // Unsubscribe when disabled
    private void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void Update()
    {
        // Smoothly move each fingerprint towards its target position
        foreach (var kvp in prefabTargets)
        {
            GameObject prefab = kvp.Key;
            Vector3 targetPos = kvp.Value;
            Vector3 velocity = prefabVelocities[prefab];

            prefab.transform.localPosition = Vector3.SmoothDamp(
                prefab.transform.localPosition,
                targetPos,
                ref velocity,
                prefabSmoothTime);

            prefabVelocities[prefab] = velocity;
        }
    }

    // Called when tracked images (QR codes) are added or updated
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var trackedImage in args.added)
            HandleImageDetected(trackedImage);

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
                HandleImageDetected(trackedImage);
        }
    }

    // Main handler for QR code detection
    private void HandleImageDetected(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking) return;

        string imageName = trackedImage.referenceImage.name;

        // If same QR code is already active, do nothing
        if (imageName == currentQRCode) return;

        // Remove previous grid if any
        if (currentGrid != null)
            Destroy(currentGrid);

        // Get color based on QR code
        Color gridColor = GetColorForImage(imageName);

        // Spawn new grid anchored to QR code and fingerprints
        currentGrid = SpawnGrid(trackedImage, gridColor);
        SpawnFingerprints(currentGrid);

        currentQRCode = imageName;
    }

    // Returns a color for each QR code
    private Color GetColorForImage(string imageName)
    {
        switch (imageName)
        {
            case "QRCode1": return new Color(0f, 1f, 1f, alpha); // Cyan
            case "QRCode2": return new Color(1f, 0f, 0f, alpha); // Red
            case "QRCode3": return new Color(0f, 1f, 0f, alpha); // Green
            default: return new Color(1f, 1f, 1f, alpha);        // White
        }
    }

    // Spawns a grid of cubes anchored to the QR code
    private GameObject SpawnGrid(ARTrackedImage trackedImage, Color gridColor)
    {
        int columns = Mathf.RoundToInt(gridWidth / cellSize);
        int rows = Mathf.RoundToInt(gridHeight / cellSize);

        GameObject gridParent = new GameObject("Grid_" + trackedImage.referenceImage.name);

        // Attach an ARAnchor to the QR code for stability
        ARAnchor anchor = trackedImage.gameObject.GetComponent<ARAnchor>();
        if (anchor == null)
            anchor = trackedImage.gameObject.AddComponent<ARAnchor>();

        gridParent.transform.SetParent(anchor.transform);

        float cubeHeight = 0.001f; // very thin cube for visual grid
        float buffer = gridYOffset; // Y offset above QR code
        gridParent.transform.localPosition = new Vector3(0f, buffer, 0f);
        gridParent.transform.localRotation = Quaternion.identity;

        // Calculate start offset so grid is centered
        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;
        Vector3 startOffset = new Vector3(-totalWidth / 2f + cellSize / 2f, 0f, -totalHeight / 2f + cellSize / 2f);

        float extraScale = 0.02f;

        // Spawn cubes in a grid
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 localPos = startOffset + new Vector3(x * cellSize, 0, z * cellSize);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(gridParent.transform);
                cube.transform.localPosition = localPos;
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(cellSize + extraScale, cubeHeight, cellSize + extraScale);

                Renderer rend = cube.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = gridColor;
                mat.SetFloat("_SurfaceType", 1);
                rend.material = mat;

                Destroy(cube.GetComponent<Collider>());
                cube.tag = "GridCell";
            }
        }

        return gridParent;
    }

    // Spawns fingerprint prefabs flat on the grid at random positions
    private void SpawnFingerprints(GameObject gridParent)
    {
        prefabTargets.Clear();
        prefabVelocities.Clear();
        List<Vector3> placedPositions = new List<Vector3>();

        for (int i = 0; i < fingerprintsCount; i++)
        {
            Vector3 randomPos;
            int attempts = 0;

            do
            {
                float randomX = Random.Range(-gridWidth / 2f, gridWidth / 2f);
                float randomZ = Random.Range(-gridHeight / 2f, gridHeight / 2f);
                randomPos = new Vector3(randomX, 0.001f, randomZ); // slight height to sit on grid
                attempts++;
            } while (!IsPositionValid(randomPos, placedPositions) && attempts < 50);

            placedPositions.Add(randomPos);

            GameObject fingerprint = Instantiate(fingerprintPrefab, gridParent.transform);
            fingerprint.transform.localPosition = randomPos;
            fingerprint.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0); // flat on grid
            fingerprint.transform.localScale = Vector3.one * (cellSize * 0.01f); // scale appropriately
            fingerprint.tag = "Interactable";

            if (fingerprint.GetComponent<Collider>() != null)
                Destroy(fingerprint.GetComponent<Collider>());

            prefabTargets.Add(fingerprint, randomPos);
            prefabVelocities.Add(fingerprint, Vector3.zero);
        }
    }

    // Checks if the position is far enough from already placed fingerprints
    private bool IsPositionValid(Vector3 pos, List<Vector3> placedPositions)
    {
        foreach (var p in placedPositions)
        {
            if (Vector3.Distance(pos, p) < minDistanceBetweenPrefabs)
                return false;
        }
        return true;
    }
}