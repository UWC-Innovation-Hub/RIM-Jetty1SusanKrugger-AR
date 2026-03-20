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
    public int fingerprintsCount = 4;
    public float minDistanceBetweenPrefabs = 0.05f;
    public float prefabSmoothTime = 0.1f;

    [Header("Video Settings")]
    public VideoClip[] videoClips; // Assign in Inspector

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
                prefabSmoothTime);

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
        float buffer = gridYOffset;

        gridParent.transform.localPosition = new Vector3(0f, buffer, 0f);
        gridParent.transform.localRotation = Quaternion.identity;

        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;

        Vector3 startOffset = new Vector3(
            -totalWidth / 2f + cellSize / 2f,
            0f,
            -totalHeight / 2f + cellSize / 2f
        );

        float extraScale = 0.02f;

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
                randomPos = new Vector3(randomX, 0.001f, randomZ);
                attempts++;
            }
            while (!IsPositionValid(randomPos, placedPositions) && attempts < 50);

            placedPositions.Add(randomPos);

            GameObject fingerprint = Instantiate(fingerprintPrefab, gridParent.transform);

            fingerprint.transform.localPosition = randomPos;
            fingerprint.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            fingerprint.transform.localScale = Vector3.one * (cellSize * 0.01f);
            fingerprint.tag = "Interactable";

            // Ensure collider exists
            Collider col = fingerprint.GetComponent<Collider>();
            if (col == null)
                col = fingerprint.AddComponent<BoxCollider>();

            // Make collider easier to tap
            if (col is BoxCollider box)
                box.size = new Vector3(0.1f, 0.02f, 0.1f);

            // 🎥 Assign random video
            VideoPlayer vp = fingerprint.GetComponent<VideoPlayer>();

            if (vp != null && videoClips != null && videoClips.Length > 0)
            {
                int index = Random.Range(0, videoClips.Length);
                vp.clip = videoClips[index];
            }
            else
            {
                Debug.LogWarning("Missing VideoPlayer or video clips!");
            }

            prefabTargets.Add(fingerprint, randomPos);
            prefabVelocities.Add(fingerprint, Vector3.zero);
        }
    }

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