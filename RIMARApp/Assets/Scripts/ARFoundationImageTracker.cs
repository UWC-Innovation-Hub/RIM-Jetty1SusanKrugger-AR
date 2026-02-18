using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARFoundationImageTracker : MonoBehaviour
{
    [Header("AR Foundation Setup")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Content Prefabs")]
    [SerializeField] private List<TrackedImagePrefab> imagePrefabs = new List<TrackedImagePrefab>();

    [Header("Tracking Behavior")]
    [SerializeField] private bool updatePoseOnTracking = true;
    [SerializeField] private bool disableWhenNotTracking = false;

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

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
            OnImageAdded(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            OnImageUpdated(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            OnImageRemoved(trackedImage);
        }
    }

    void OnImageAdded(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedPrefabs.ContainsKey(imageName))
        {
            return;
        }

        TrackedImagePrefab config = imagePrefabs.Find(x => x.imageName == imageName);
        if (config != null && config.prefabToSpawn != null)
        {
            GameObject spawnedObject = Instantiate(config.prefabToSpawn);
            spawnedObject.name = $"{config.prefabToSpawn.name}_{imageName}";
            
            spawnedObject.transform.SetParent(trackedImage.transform);
            spawnedObject.transform.localPosition = Vector3.zero;
            spawnedObject.transform.localRotation = Quaternion.identity;
            spawnedObject.transform.localScale = Vector3.one;

            spawnedPrefabs[imageName] = spawnedObject;

            Debug.Log($"Spawned AR content for tracked image: {imageName}");
        }
    }

    void OnImageUpdated(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedPrefabs.TryGetValue(imageName, out GameObject spawnedObject))
        {
            if (disableWhenNotTracking)
            {
                spawnedObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            }

            if (updatePoseOnTracking && trackedImage.trackingState == TrackingState.Tracking)
            {
                spawnedObject.transform.SetParent(trackedImage.transform);
                spawnedObject.transform.localPosition = Vector3.zero;
                spawnedObject.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void OnImageRemoved(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedPrefabs.TryGetValue(imageName, out GameObject spawnedObject))
        {
            if (disableWhenNotTracking)
            {
                spawnedObject.SetActive(false);
            }
        }
    }

    public GameObject GetSpawnedContent(string imageName)
    {
        spawnedPrefabs.TryGetValue(imageName, out GameObject content);
        return content;
    }

    public void ClearAllContent()
    {
        foreach (var obj in spawnedPrefabs.Values)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedPrefabs.Clear();
    }
}

[System.Serializable]
public class TrackedImagePrefab
{
    [Tooltip("Name must match the reference image name in the XRReferenceImageLibrary")]
    public string imageName;
    
    [Tooltip("Prefab to spawn when this image is detected (should contain all scene objects as children)")]
    public GameObject prefabToSpawn;
}
