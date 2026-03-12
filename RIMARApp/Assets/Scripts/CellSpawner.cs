using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CellSpawner : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager imageManager;

    [Header("Grid Settings")]
    public float gridWidth = 2f;
    public float gridHeight = 1f;
    public float cellSize = 0.05f;
    public float gridYOffset = 0.01f;

    [Header("Grid Appearance")]
    [Range(0f, 1f)]
    public float alpha = 0.05f;

    [Header("Sphere Placeholders")]
    public int spheresCount = 4;
    public float minDistanceBetweenSpheres = 0.05f;
    public float sphereSmoothTime = 0.1f;

    private GameObject currentGrid = null;
    private string currentQRCode = null;

    private Dictionary<GameObject, Vector3> sphereTargets = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Vector3> sphereVelocities = new Dictionary<GameObject, Vector3>();

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
        // Smoothly move spheres to their target positions
        foreach (var kvp in sphereTargets)
        {
            GameObject sphere = kvp.Key;
            Vector3 targetPos = kvp.Value;

            Vector3 velocity = sphereVelocities[sphere];
            sphere.transform.localPosition = Vector3.SmoothDamp(
                sphere.transform.localPosition, targetPos, ref velocity, sphereSmoothTime);
            sphereVelocities[sphere] = velocity;
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

        // If same QR code, do nothing
        if (imageName == currentQRCode)
            return;

        // Remove old grid if exists
        if (currentGrid != null)
            Destroy(currentGrid);

        // Spawn new grid
        Color gridColor = GetColorForImage(imageName);
        currentGrid = SpawnGrid(trackedImage, gridColor);
        SpawnSpheres(currentGrid);

        currentQRCode = imageName; // Update currently active QR code
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

        // Anchor for stability
        ARAnchor anchor = trackedImage.gameObject.GetComponent<ARAnchor>();
        if (anchor == null)
            anchor = trackedImage.gameObject.AddComponent<ARAnchor>();
        gridParent.transform.SetParent(anchor.transform);

        // Calculate total grid height for positioning
        float cubeHeight = 0.02f; // cube Y scale
        float totalGridHeightY = cubeHeight; // top of grid cubes
        float buffer = 0.005f; // small offset above QR code

        // Position the grid at the **top of the QR code**
        gridParent.transform.localPosition = new Vector3(0f, totalGridHeightY / 2f + buffer, 1.2f);
        gridParent.transform.localRotation = Quaternion.identity;

        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;
        Vector3 startOffset = new Vector3(-totalWidth / 2f + cellSize / 2f, 0f, -totalHeight / 2f + cellSize / 2f);

        float extraScale = 0.02f; // make cubes slightly bigger

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

    private void SpawnSpheres(GameObject gridParent)
    {
        sphereTargets.Clear();
        sphereVelocities.Clear();

        List<Vector3> placedPositions = new List<Vector3>();

        for (int i = 0; i < spheresCount; i++)
        {
            Vector3 randomPos;
            int attempts = 0;

            do
            {
                float randomX = Random.Range(-gridWidth / 2f, gridWidth / 2f);
                float randomZ = Random.Range(-gridHeight / 2f, gridHeight / 2f);
                randomPos = new Vector3(randomX, 0.03f, randomZ);
                attempts++;
            }
            while (!IsPositionValid(randomPos, placedPositions) && attempts < 50);

            placedPositions.Add(randomPos);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(gridParent.transform);
            sphere.transform.localPosition = randomPos;
            sphere.transform.localRotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
            sphere.transform.localScale = Vector3.one * (cellSize * 0.01f);
            sphere.tag = "Interactable";

            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material.color = Color.gray;

            Destroy(sphere.GetComponent<Collider>());

            sphereTargets.Add(sphere, randomPos);
            sphereVelocities.Add(sphere, Vector3.zero);
        }
    }

    private bool IsPositionValid(Vector3 pos, List<Vector3> placedPositions)
    {
        foreach (var p in placedPositions)
        {
            if (Vector3.Distance(pos, p) < minDistanceBetweenSpheres)
                return false;
        }
        return true;
    }
}