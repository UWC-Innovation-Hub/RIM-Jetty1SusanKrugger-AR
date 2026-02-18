using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class QRCodeAnchor : MonoBehaviour
{
    [Header("AR Tracking")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Content Configuration")]
    [SerializeField] private List<QRContentMapping> contentMappings = new List<QRContentMapping>();

    [Header("Settings")]
    [SerializeField] private bool worldLocked = true;
    [SerializeField] private bool spawnOnlyOnce = true;
    [SerializeField] private Vector3 contentOffset = Vector3.zero;

    private Dictionary<string, GameObject> spawnedContent = new Dictionary<string, GameObject>();
    private Dictionary<string, Vector3> worldLockedPositions = new Dictionary<string, Vector3>();
    private Dictionary<string, Quaternion> worldLockedRotations = new Dictionary<string, Quaternion>();

    void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            HandleImageAdded(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            HandleImageUpdated(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            HandleImageRemoved(trackedImage);
        }
    }

    void HandleImageAdded(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnOnlyOnce && spawnedContent.ContainsKey(imageName))
        {
            return;
        }

        QRContentMapping mapping = contentMappings.Find(m => m.qrCodeName == imageName);
        if (mapping != null && mapping.contentPrefab != null)
        {
            Vector3 spawnPosition = trackedImage.transform.position + trackedImage.transform.rotation * contentOffset;
            Quaternion spawnRotation = trackedImage.transform.rotation;

            GameObject spawnedObject = Instantiate(mapping.contentPrefab, spawnPosition, spawnRotation);
            spawnedContent[imageName] = spawnedObject;

            if (worldLocked)
            {
                worldLockedPositions[imageName] = spawnPosition;
                worldLockedRotations[imageName] = spawnRotation;
                spawnedObject.transform.SetParent(null);
                
                WorldAnchor anchor = spawnedObject.AddComponent<WorldAnchor>();
                anchor.Initialize(spawnPosition, spawnRotation);
            }
            else
            {
                spawnedObject.transform.SetParent(trackedImage.transform);
            }

            Debug.Log($"AR Content spawned for QR code: {imageName} at world position {spawnPosition} (World-Locked: {worldLocked})");
        }
    }

    void HandleImageUpdated(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (!worldLocked && spawnedContent.TryGetValue(imageName, out GameObject content))
        {
            bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
            content.SetActive(isTracking);

            if (isTracking)
            {
                content.transform.position = trackedImage.transform.position + trackedImage.transform.rotation * contentOffset;
                content.transform.rotation = trackedImage.transform.rotation;
            }
        }
    }

    void HandleImageRemoved(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (!worldLocked && spawnedContent.TryGetValue(imageName, out GameObject content))
        {
            content.SetActive(false);
        }
    }

    public void ClearAllContent()
    {
        foreach (var content in spawnedContent.Values)
        {
            if (content != null)
            {
                Destroy(content);
            }
        }

        spawnedContent.Clear();
        worldLockedPositions.Clear();
        worldLockedRotations.Clear();
    }

    public GameObject GetSpawnedContent(string qrCodeName)
    {
        spawnedContent.TryGetValue(qrCodeName, out GameObject content);
        return content;
    }
}

[System.Serializable]
public class QRContentMapping
{
    public string qrCodeName;
    public GameObject contentPrefab;
}
