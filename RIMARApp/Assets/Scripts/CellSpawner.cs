using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.Collections.Generic;

public class CellSpawner : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager imageManager;

    [Header("Grid Settings")]
    public float gridWidth = 2f;
    public float gridHeight = 1f;
    public float cellSize = 0.05f;

    [Header("Animation")]
    [Range(0f, 1f)] public float targetAlpha = 0.15f;
    public float fadeDuration = 0.4f;
    public float spawnScaleDuration = 0.25f;

    private string currentImageName = null;
    private GameObject currentGridParent = null;
    private ARAnchor currentAnchor = null;

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
            if (trackedImage.trackingState == TrackingState.Tracking)
                HandleImageDetected(trackedImage);
        }
    }

    private void HandleImageDetected(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (imageName == currentImageName)
            return;

        StartCoroutine(SwitchGrid(trackedImage));
    }

    private IEnumerator SwitchGrid(ARTrackedImage trackedImage)
    {
        currentImageName = trackedImage.referenceImage.name;

        // Fade out old grid
        if (currentGridParent != null)
        {
            yield return StartCoroutine(FadeGrid(currentGridParent, 0f));
            Destroy(currentGridParent);
        }

        // Remove old anchor
        if (currentAnchor != null)
        {
            Destroy(currentAnchor.gameObject);
        }

        // CREATE ANCHOR (AR Foundation 6 method)
        GameObject anchorObject = new GameObject("ImageAnchor_" + currentImageName);

        anchorObject.transform.position = trackedImage.transform.position;
        anchorObject.transform.rotation = trackedImage.transform.rotation;

        currentAnchor = anchorObject.AddComponent<ARAnchor>();

        // Spawn grid under anchor
        Color baseColor = GetColorForImage(currentImageName);
        currentGridParent = SpawnGrid(currentAnchor.transform, baseColor);

        yield return StartCoroutine(FadeGrid(currentGridParent, targetAlpha));
    }

    private Color GetColorForImage(string imageName)
    {
        switch (imageName)
        {
            case "QRCode1": return Color.cyan;
            case "QRCode2": return Color.red;
            case "QRCode3": return Color.green;
            default: return Color.white;
        }
    }

    private GameObject SpawnGrid(Transform parent, Color baseColor)
    {
        int columns = Mathf.RoundToInt(gridWidth / cellSize);
        int rows = Mathf.RoundToInt(gridHeight / cellSize);

        GameObject gridParent = new GameObject("GridParent_" + currentImageName);
        gridParent.transform.SetParent(parent);
        gridParent.transform.localPosition = Vector3.zero;
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

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                cube.transform.SetParent(gridParent.transform);
                cube.transform.localPosition = localPos;
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = Vector3.zero;

                StartCoroutine(AnimateScale(
                    cube.transform,
                    new Vector3(cellSize, 0.002f, cellSize),
                    spawnScaleDuration));

                Renderer rend = cube.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                mat.SetFloat("_Surface", 1);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

                Color c = baseColor;
                c.a = 0f;
                mat.color = c;

                rend.material = mat;

                Destroy(cube.GetComponent<Collider>());
            }
        }

        return gridParent;
    }

    private IEnumerator AnimateScale(Transform target, Vector3 targetScale, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            target.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        target.localScale = targetScale;
    }

    private IEnumerator FadeGrid(GameObject gridParent, float targetAlpha)
    {
        float time = 0f;

        List<Material> materials = new List<Material>();
        foreach (Renderer r in gridParent.GetComponentsInChildren<Renderer>())
            materials.Add(r.material);

        float startAlpha = materials[0].color.a;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            foreach (Material m in materials)
            {
                Color c = m.color;
                c.a = alpha;
                m.color = c;
            }

            time += Time.deltaTime;
            yield return null;
        }

        foreach (Material m in materials)
        {
            Color c = m.color;
            c.a = targetAlpha;
            m.color = c;
        }
    }
}