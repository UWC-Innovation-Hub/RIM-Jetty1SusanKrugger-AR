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

    [Header("Interactable Prefab")]
    public GameObject interactablePrefab;

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

        if (imageName == currentImageName)
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

        gridParent.transform.position =
            trackedImage.transform.position + trackedImage.transform.up * 0.01f;

        gridParent.transform.rotation = trackedImage.transform.rotation;

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

                // Create grid cube
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

                Destroy(cube.GetComponent<Collider>());
                cube.tag = "GridCell";

                // Spawn interactable prefab
                if (interactablePrefab != null)
                {
                    GameObject obj = Instantiate(interactablePrefab);
                    obj.transform.SetParent(gridParent.transform);
                    obj.transform.localPosition = localPos + new Vector3(0, 0.03f, 0f);
                    obj.transform.localRotation = Quaternion.Euler(90,0,9);
                    obj.transform.localScale = Vector3.one * (cellSize * 0.02f);
                    obj.tag = "Interactable";

                    //Add collider if it doesn't exist
                    if (obj.GetComponent<Collider>() == null)
                    {
                        obj.AddComponent<BoxCollider>();
                    }
                }
            }
        }

        Debug.Log($"Grid spawned for {trackedImage.referenceImage.name}: {columns} x {rows}");
        Debug.Log("Grid world position: " + gridParent.transform.position);

        return gridParent;
    }
}