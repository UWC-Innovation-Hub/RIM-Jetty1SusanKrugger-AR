using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Video;
using System.Collections.Generic;

public class CellSpawner : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager imageManager;

    [Header("Grid Settings")]
    public float gridWidth = 2f;
    public float gridHeight = 1f;
    public float cellSize = 0.025f;
    public float gridYOffset = 0.02f;

    [Header("Grid Appearance")]
    [Range(0f, 1f)]
    public float alpha = 0.05f;

    [Header("Fingerprint Prefab")]
    public GameObject fingerprintPrefab;
    public float prefabSmoothTime = 0.1f;

    [Header("Fixed Positions (LOCAL to grid)")]
    public List<Vector3> fixedPositions = new List<Vector3>()
    {
        new Vector3(-0.5f, 0.001f, -0.2f),
        new Vector3(0.3f, 0.001f, 0.1f),
        new Vector3(0.6f, 0.001f, -0.3f),
        new Vector3(-0.2f, 0.001f, 0.4f)
    };

    [Header("Video Settings")]
    public VideoClip[] videoClips;

    private GameObject currentGrid = null;
    private string currentQRCode = null;

    private Dictionary<GameObject, Vector3> prefabTargets = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Vector3> prefabVelocities = new Dictionary<GameObject, Vector3>();

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

    private void Update()
    {
        foreach (var kvp in prefabTargets)
        {
            GameObject prefab = kvp.Key;
            Vector3 targetPos = kvp.Value;
            Vector3 velocity = prefabVelocities[prefab];

            prefab.transform.localPosition = Vector3.SmoothDamp(
                prefab.transform.localPosition,
                targetPos,
                ref velocity,
                prefabSmoothTime
            );

            prefabVelocities[prefab] = velocity;
        }
    }

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

    private void HandleImageDetected(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking) return;

        string imageName = trackedImage.referenceImage.name;

        if (imageName == currentQRCode) return;

        if (currentGrid != null)
            Destroy(currentGrid);

        Color gridColor = GetColorForImage(imageName);

        currentGrid = SpawnGrid(trackedImage, gridColor);
        SpawnFingerprints(currentGrid);

        currentQRCode = imageName;
    }

    private Color GetColorForImage(string imageName)
    {
        switch (imageName)
        {
            case "QRCode1": return new Color(0f, 1f, 1f, alpha);
            case "QRCode2": return new Color(1f, 0f, 0f, alpha);
            case "QRCode3": return new Color(0f, 1f, 0f, alpha);
            default: return new Color(1f, 1f, 1f, alpha);
        }
    }

    private GameObject SpawnGrid(ARTrackedImage trackedImage, Color gridColor)
    {
        int columns = Mathf.RoundToInt(gridWidth / cellSize);
        int rows = Mathf.RoundToInt(gridHeight / cellSize);

        GameObject gridParent = new GameObject("Grid_" + trackedImage.referenceImage.name);

        ARAnchor anchor = trackedImage.gameObject.GetComponent<ARAnchor>();
        if (anchor == null)
            anchor = trackedImage.gameObject.AddComponent<ARAnchor>();

        gridParent.transform.SetParent(anchor.transform);

        float cubeHeight = 0.001f;

        gridParent.transform.localPosition = new Vector3(0f, gridYOffset, 0f);
        gridParent.transform.localRotation = Quaternion.identity;

        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;

        Vector3 startOffset = new Vector3(
            -totalWidth / 2f + cellSize / 2f,
            0f,
            -totalHeight / 2f + cellSize / 2f
        );

        float extraScale = 0.02f;

        // Shared material (performance improvement)
        Material sharedMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        sharedMat.color = gridColor;
        sharedMat.SetFloat("_SurfaceType", 1);

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
                rend.material = sharedMat;

                Destroy(cube.GetComponent<Collider>());
                cube.tag = "GridCell";
            }
        }

        return gridParent;
    }

    private void SpawnFingerprints(GameObject gridParent)
    {
        prefabTargets.Clear();
        prefabVelocities.Clear();

        for (int i = 0; i < fixedPositions.Count; i++)
        {
            Vector3 localPos = fixedPositions[i];

            GameObject fingerprint = Instantiate(fingerprintPrefab, gridParent.transform);

            fingerprint.transform.localPosition = localPos;
            fingerprint.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            fingerprint.transform.localScale = Vector3.one * 0.02f;
            fingerprint.tag = "Interactable";

            // Ensure collider
            if (!fingerprint.TryGetComponent(out BoxCollider box))
                box = fingerprint.AddComponent<BoxCollider>();

            box.size = new Vector3(0.1f, 0.02f, 0.1f);

            // Assign video
            if (fingerprint.TryGetComponent(out VideoPlayer vp) && videoClips != null && videoClips.Length > 0)
            {
                int index = Random.Range(0, videoClips.Length);
                vp.clip = videoClips[index];
            }
            else
            {
                Debug.LogWarning("Missing VideoPlayer or video clips!");
            }

            prefabTargets.Add(fingerprint, localPos);
            prefabVelocities.Add(fingerprint, Vector3.zero);
        }
    }
}