using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingManager : MonoBehaviour
{
    [Header("Image Tracking Setup")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject defaultTrackableObjectPrefab;
    
    [Header("Content Mapping")]
    [SerializeField] private List<ImageContentMapping> imageContentMappings = new List<ImageContentMapping>();
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private Dictionary<string, GameObject> imageToContentMap = new Dictionary<string, GameObject>();
    private Dictionary<ARTrackedImage, GameObject> spawnedContent = new Dictionary<ARTrackedImage, GameObject>();
    
    void Start()
    {
        // Get ARTrackedImageManager if not assigned
        if (trackedImageManager == null)
        {
            trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
        }
        
        if (trackedImageManager == null)
        {
            Debug.LogError("ARTrackedImageManager not found! Please add it to your XR Origin.");
            enabled = false;
            return;
        }
        
        // Build mapping dictionary
        BuildContentMapping();
        
        // Subscribe to image tracking events
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        
        if (debugMode)
        {
            Debug.Log($"ImageTrackingManager initialized with {imageContentMappings.Count} content mappings");
        }
    }
    
    void OnDestroy()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }
    }
    
    private void BuildContentMapping()
    {
        imageToContentMap.Clear();
        
        foreach (var mapping in imageContentMappings)
        {
            if (!string.IsNullOrEmpty(mapping.imageName) && mapping.contentPrefab != null)
            {
                imageToContentMap[mapping.imageName] = mapping.contentPrefab;
            }
        }
    }
    
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Handle newly detected images
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            OnImageAdded(trackedImage);
        }
        
        // Handle updated images
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            OnImageUpdated(trackedImage);
        }
        
        // Handle removed images
        foreach (var removedKvp in eventArgs.removed)
        {
            ARTrackedImage trackedImage = removedKvp.Value;
            OnImageRemoved(trackedImage);
        }
    }
    
    private void OnImageAdded(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        
        if (debugMode)
        {
            Debug.Log($"New image detected: {imageName}");
        }
        
        // Get the appropriate content prefab
        GameObject contentPrefab = GetContentPrefabForImage(imageName);
        
        if (contentPrefab != null)
        {
            // Instantiate content at the tracked image position
            GameObject spawnedObject = Instantiate(contentPrefab, trackedImage.transform);
            spawnedContent[trackedImage] = spawnedObject;
            
            // Add ImageTrackingHandler if it doesn't exist
            ImageTrackingHandler handler = spawnedObject.GetComponent<ImageTrackingHandler>();
            if (handler == null)
            {
                handler = spawnedObject.AddComponent<ImageTrackingHandler>();
            }
            
            if (debugMode)
            {
                Debug.Log($"Spawned content for image: {imageName}");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning($"No content prefab found for image: {imageName}");
            }
        }
    }
    
    private void OnImageUpdated(ARTrackedImage trackedImage)
    {
        // Content position and rotation are automatically updated by parenting to trackedImage.transform
        // You can add additional update logic here if needed
        
        if (debugMode && trackedImage.trackingState == TrackingState.Tracking)
        {
            Debug.Log($"Image updated: {trackedImage.referenceImage.name} - State: {trackedImage.trackingState}");
        }
    }
    
    private void OnImageRemoved(ARTrackedImage trackedImage)
    {
        if (spawnedContent.TryGetValue(trackedImage, out GameObject spawnedObject))
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }
            spawnedContent.Remove(trackedImage);
            
            if (debugMode)
            {
                Debug.Log($"Removed content for image: {trackedImage.referenceImage.name}");
            }
        }
    }
    
    private GameObject GetContentPrefabForImage(string imageName)
    {
        // First check if we have a specific mapping for this image
        if (imageToContentMap.TryGetValue(imageName, out GameObject specificPrefab))
        {
            return specificPrefab;
        }
        
        // Fall back to default prefab
        return defaultTrackableObjectPrefab;
    }
    
    public void AddImageContentMapping(string imageName, GameObject contentPrefab)
    {
        var existingMapping = imageContentMappings.Find(m => m.imageName == imageName);
        if (existingMapping != null)
        {
            existingMapping.contentPrefab = contentPrefab;
        }
        else
        {
            imageContentMappings.Add(new ImageContentMapping { imageName = imageName, contentPrefab = contentPrefab });
        }
        
        BuildContentMapping();
    }
    
    public List<ARTrackedImage> GetCurrentlyTrackedImages()
    {
        List<ARTrackedImage> trackedImages = new List<ARTrackedImage>();
        
        if (trackedImageManager != null && trackedImageManager.trackables.count > 0)
        {
            foreach (var trackable in trackedImageManager.trackables)
            {
                if (trackable.trackingState == TrackingState.Tracking)
                {
                    trackedImages.Add(trackable);
                }
            }
        }
        
        return trackedImages;
    }
}

[System.Serializable]
public class ImageContentMapping
{
    public string imageName;
    public GameObject contentPrefab;
}