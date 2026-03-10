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

    [Header("Grid Offset From QR")]
    public Vector3 gridOffset = new Vector3(0f, 0.01f, 0f);
    public Vector3 gridRotation = Vector3.zero;

    [Header("Grid Appearance")]
    [Range(0f, 1f)]
    public float alpha = 0.05f;

    [Header("Interactable Prefab")]
    public GameObject interactablePrefab;
    public int prefabsPerCell = 3;

    private string currentImageName = null;
    private GameObject currentGridParent = null;

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

    private GameObject SpawnGrid(ARTrackedImage trackedImage, Color gridColor)
    {
        int columns = Mathf.RoundToInt(gridWidth / cellSize);
        int rows = Mathf.RoundToInt(gridHeight / cellSize);

        GameObject gridParent = new GameObject("GridParent_" + trackedImage.referenceImage.name);

        // Anchor for stability
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

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 localPos = startOffset + new Vector3(x * cellSize, 0, z * cellSize);

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(gridParent.transform);
                cube.transform.localPosition = localPos;
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(cellSize, 0.002f, cellSize);

                Renderer rend = cube.GetComponent<Renderer>();

                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = gridColor;
                mat.SetFloat("_SurfaceType", 1);

                rend.material = mat;

                Destroy(cube.GetComponent<Collider>());
                cube.tag = "GridCell";

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
                        obj.transform.localRotation = Quaternion.Euler(90, 0, 9);
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