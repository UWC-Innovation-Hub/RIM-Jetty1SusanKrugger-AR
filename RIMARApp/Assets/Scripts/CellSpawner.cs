using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Main class that controls grid spawning when a QR code (tracked image) is detected
public class CellSpawner : MonoBehaviour
{
    // ================================
    // AR COMPONENT REFERENCES
    // ================================

    [Header("AR Components")]

    // Reference to the ARTrackedImageManager
    // This manager detects and tracks QR codes / images in AR
    public ARTrackedImageManager imageManager;


    // ================================
    // GRID SIZE SETTINGS
    // ================================

    [Header("Grid Settings")]

    // Total width of the grid in meters
    public float gridWidth = 2f;

    // Total height of the grid in meters
    public float gridHeight = 1f;

    // Size of each cell in the grid
    // Example: 0.05 = 5cm
    public float cellSize = 0.05f;


    // ================================
    // GRID POSITION RELATIVE TO QR
    // ================================

    [Header("Grid Offset From QR")]

    // Offset moves the grid slightly above the QR code
    // Prevents the grid from clipping into the QR surface
    public Vector3 gridOffset = new Vector3(0f, 0.01f, 0f);

    // Optional rotation of the grid relative to the QR code
    public Vector3 gridRotation = Vector3.zero;


    // ================================
    // GRID APPEARANCE
    // ================================

    [Header("Grid Appearance")]

    // Transparency level of the grid
    // 0 = invisible
    // 1 = fully solid
    [Range(0f, 1f)]
    public float alpha = 0.05f;


    // ================================
    // INTERACTABLE OBJECT SETTINGS
    // ================================

    [Header("Interactable Prefab")]

    // Prefab that will be spawned on each grid cell
    public GameObject interactablePrefab;

    // How many prefabs should spawn in each grid cell
    public int prefabsPerCell = 3;


    // ================================
    // INTERNAL VARIABLES
    // ================================

    // Stores the name of the currently tracked QR code
    private string currentImageName = null;

    // Stores the grid object currently spawned
    private GameObject currentGridParent = null;


    // ================================
    // UNITY EVENT: SCRIPT ENABLED
    // ================================

    private void OnEnable()
    {
        // Subscribe to AR tracked image events
        // This tells Unity to notify us when images are detected/updated
        if (imageManager != null)
            imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }


    // ================================
    // UNITY EVENT: SCRIPT DISABLED
    // ================================

    private void OnDisable()
    {
        // Remove event listener when script is disabled
        // Prevents memory leaks and duplicate events
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }


    // ================================
    // IMAGE TRACKING EVENT
    // ================================

    // This function is called whenever:
    // - a new image is detected
    // - an image position is updated
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        // Handle newly detected QR codes
        // We ONLY spawn grids when an image is first detected
        // This prevents duplicate grids from spawning during tracking updates
        foreach (var trackedImage in args.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                HandleImageDetected(trackedImage);
            }
        }
    }


    // ================================
    // IMAGE DETECTION HANDLER
    // ================================

    private void HandleImageDetected(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking)
            return;

        string imageName = trackedImage.referenceImage.name;

        // Prevent duplicate grids
        if (currentGridParent != null && imageName == currentImageName)
            return;

        Debug.Log("Switching to QR: " + imageName);

        if (currentGridParent != null)
        {
            Destroy(currentGridParent);
        }

        Color gridColor = GetColorForImage(imageName);

        currentGridParent = SpawnGrid(trackedImage, gridColor);

        currentImageName = imageName;
    }


    // ================================
    // GRID COLOR SELECTION
    // ================================

    private Color GetColorForImage(string imageName)
    {
        switch (imageName)
        {
            case "QRCode1":
                return new Color(0f, 1f, 1f, alpha);
            case "QRCode2":
                return new Color(1f, 0f, 0f, alpha);
            case "QRCode3":
                return new Color(0f, 1f, 0f, alpha);
            default:
                return new Color(1f, 1f, 1f, alpha);
        }
    }


    // ================================
    // GRID SPAWNING FUNCTION
    // ================================

    private GameObject SpawnGrid(ARTrackedImage trackedImage, Color gridColor)
    {
        int columns = Mathf.RoundToInt(gridWidth / cellSize);
        int rows = Mathf.RoundToInt(gridHeight / cellSize);

        GameObject gridParent = new GameObject("GridParent_" + trackedImage.referenceImage.name);

        // ================================
        // CREATE ANCHOR FOR STABILITY
        // ================================

        ARAnchor anchor = trackedImage.gameObject.GetComponent<ARAnchor>();

        if (anchor == null)
            anchor = trackedImage.gameObject.AddComponent<ARAnchor>();

        gridParent.transform.SetParent(anchor.transform);

        gridParent.transform.localPosition = gridOffset;
        gridParent.transform.localRotation = Quaternion.Euler(gridRotation);

        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;

        Vector3 startOffset = new Vector3(
            -totalWidth / 2f + cellSize / 2f,
            0f,
            -totalHeight / 2f + cellSize / 2f
        );


        // ================================
        // CREATE GRID CELLS
        // ================================

        // Instead of spawning a cube for every cell (800 objects),
        // we create long strips that form the grid rows and columns.
        // This reduces the total objects to roughly rows + columns (~60 objects).


        // ---------- CREATE ROW STRIPS ----------
        for (int z = 0; z < rows; z++)
        {
            Vector3 localPos = startOffset + new Vector3(totalWidth / 2f - cellSize / 2f, 0, z * cellSize);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            cube.transform.SetParent(gridParent.transform);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;

            cube.transform.localScale = new Vector3(totalWidth, 0.002f, cellSize);

            Renderer rend = cube.GetComponent<Renderer>();

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = gridColor;
            mat.SetFloat("_SurfaceType", 1);

            rend.material = mat;

            Destroy(cube.GetComponent<Collider>());
            cube.tag = "GridCell";
        }


        // ---------- CREATE COLUMN STRIPS ----------
        for (int x = 0; x < columns; x++)
        {
            Vector3 localPos = startOffset + new Vector3(x * cellSize, 0, totalHeight / 2f - cellSize / 2f);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            cube.transform.SetParent(gridParent.transform);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;

            cube.transform.localScale = new Vector3(cellSize, 0.002f, totalHeight);

            Renderer rend = cube.GetComponent<Renderer>();

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = gridColor;
            mat.SetFloat("_SurfaceType", 1);

            rend.material = mat;

            Destroy(cube.GetComponent<Collider>());
            cube.tag = "GridCell";
        }


        // ================================
        // SPAWN INTERACTABLE OBJECTS
        // ================================

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 localPos = startOffset + new Vector3(x * cellSize, 0, z * cellSize);

                if (interactablePrefab != null)
                {
                    for (int i = 0; i < prefabsPerCell; i++)
                    {
                        GameObject obj = Instantiate(interactablePrefab);

                        obj.transform.SetParent(gridParent.transform);

                        Vector3 randomOffset = new Vector3(
                            Random.Range(-cellSize / 4f, cellSize / 4f),
                            0.03f + i * 0.01f,
                            Random.Range(-cellSize / 4f, cellSize / 4f)
                        );

                        obj.transform.localPosition = localPos + randomOffset;
                        obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        obj.transform.localScale = Vector3.one * (cellSize * 0.02f);

                        obj.tag = "Interactable";
                    }
                }
            }
        }

        Debug.Log($"Grid spawned for {trackedImage.referenceImage.name}: {columns} x {rows}");

        return gridParent;
    }
}