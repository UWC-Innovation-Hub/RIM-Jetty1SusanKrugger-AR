using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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

    private string currentImageName = null;
    private GameObject currentGridParent = null;

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
            HandleImageDetected(trackedImage);
        }

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                HandleImageDetected(trackedImage);
            }
        }
    }

    private void HandleImageDetected(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        // If same QR is scanned again → do nothing
        if (imageName == currentImageName)
            return;

        Debug.Log("Switching to QR: " + imageName);

        // Destroy existing grid
        if (currentGridParent != null)
        {
            Destroy(currentGridParent);
        }

        // Spawn new grid
        Color gridColor = GetColorForImage(imageName);
        currentGridParent = SpawnGrid(trackedImage, gridColor);

        currentImageName = imageName;
    }

    private Color GetColorForImage(string imageName)
    {
        switch (imageName)
        {
            case "QRCode1":
                return new Color(0f, 1f, 1f, alpha); // Cyan
            case "QRCode2":
                return new Color(1f, 0f, 0f, alpha); // Red
            case "QRCode3":
                return new Color(0f, 1f, 0f, alpha); // Green
            default:
                return new Color(1f, 1f, 1f, alpha); // White fallback
        }
    }

    private GameObject SpawnGrid(ARTrackedImage trackedImage, Color gridColor)
    {
        int columns = Mathf.RoundToInt(gridWidth / cellSize);
        int rows = Mathf.RoundToInt(gridHeight / cellSize);

        GameObject gridParent = new GameObject("GridParent_" + trackedImage.referenceImage.name);
        gridParent.transform.SetParent(trackedImage.transform);
        gridParent.transform.localPosition = new Vector3(0, 0, 1.3f);
        gridParent.transform.localRotation = Quaternion.identity;

        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;

        Vector3 startOffset = new Vector3(
            -totalWidth / 2f + cellSize / 2f,
            0f,
            -totalHeight / 2f + cellSize / 2f
        );

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 localPos = startOffset + new Vector3(x * cellSize, 0, z * cellSize);

                // --- Create visual cube ---
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(gridParent.transform);
                cube.transform.localPosition = localPos;
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(cellSize, 0.002f, cellSize);

                Renderer cubeRend = cube.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = gridColor;
                mat.SetFloat("_SurfaceType", 1);
                cubeRend.material = mat;

                Destroy(cube.GetComponent<Collider>()); // Non-interactable
                cube.tag = "GridCell";

                // --- Spawn interactable object on top ---
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(gridParent.transform);
                sphere.transform.localPosition = localPos + new Vector3(0, 0.1f, 0.02f); // Slightly above cube
                sphere.transform.localRotation = Quaternion.identity;
                sphere.transform.localScale = Vector3.one * (cellSize * 0.2f);

                sphere.AddComponent<InteractableObject>(); // Add interactivity
                sphere.tag = "Interactable";
            }
        }

        Debug.Log($"Grid spawned for {trackedImage.referenceImage.name}: {columns} x {rows}");
        return gridParent;
    }
}