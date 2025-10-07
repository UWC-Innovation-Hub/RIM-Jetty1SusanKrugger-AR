using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectTrackingManager : MonoBehaviour
{
    [Header("3D Object Tracking Setup")]
    [SerializeField] private ARTrackedObjectManager trackedObjectManager;
    [SerializeField] private XRReferenceObjectLibrary referenceObjectLibrary;
    [SerializeField] private GameObject defaultTrackedObjectPrefab;
    
    [Header("Content Mapping")]
    [SerializeField] private List<ObjectContentMapping> objectContentMappings = new List<ObjectContentMapping>();
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private Dictionary<string, GameObject> objectToContentMap = new Dictionary<string, GameObject>();
    private Dictionary<ARTrackedObject, GameObject> spawnedContent = new Dictionary<ARTrackedObject, GameObject>();
    
    void Start()
    {
        // Get ARTrackedObjectManager if not assigned
        if (trackedObjectManager == null)
        {
            trackedObjectManager = FindFirstObjectByType<ARTrackedObjectManager>();
        }
        
        if (trackedObjectManager == null)
        {
            Debug.LogError("ARTrackedObjectManager not found! Please add it to your XR Origin.");
            if (debugMode)
            {
                Debug.Log("3D Object Tracking requires ARKit on iOS or ARCore on supported Android devices.");
            }
            enabled = false;
            return;
        }
        
        // Set up the reference object library
        if (referenceObjectLibrary != null)
        {
            trackedObjectManager.referenceLibrary = referenceObjectLibrary;
        }
        else
        {
            Debug.LogWarning("No Reference Object Library assigned! Create one via Assets > Create > XR > Reference Object Library");
        }
        
        // Build content mapping
        BuildContentMapping();
        
        // Subscribe to object tracking events only if enabled
        if (enabled && trackedObjectManager.enabled)
        {
            trackedObjectManager.trackablesChanged.AddListener(OnTrackedObjectsChanged);
        }
        
        if (debugMode)
        {
            Debug.Log($"ObjectTrackingManager initialized with {objectContentMappings.Count} content mappings");
            LogPlatformSupport();
        }
    }
    
    void OnDestroy()
    {
        if (trackedObjectManager != null)
        {
            trackedObjectManager.trackablesChanged.RemoveListener(OnTrackedObjectsChanged);
        }
    }
    
    void OnEnable()
    {
        if (trackedObjectManager != null && trackedObjectManager.enabled)
        {
            trackedObjectManager.trackablesChanged.AddListener(OnTrackedObjectsChanged);
            
            if (debugMode)
            {
                Debug.Log("Object Tracking enabled");
            }
        }
    }
    
    void OnDisable()
    {
        if (trackedObjectManager != null)
        {
            trackedObjectManager.trackablesChanged.RemoveListener(OnTrackedObjectsChanged);
            
            if (debugMode)
            {
                Debug.Log("Object Tracking disabled");
            }
        }
    }
    
    private void BuildContentMapping()
    {
        objectToContentMap.Clear();
        
        foreach (var mapping in objectContentMappings)
        {
            if (!string.IsNullOrEmpty(mapping.objectName) && mapping.contentPrefab != null)
            {
                objectToContentMap[mapping.objectName] = mapping.contentPrefab;
            }
        }
    }
    
    private void OnTrackedObjectsChanged(ARTrackablesChangedEventArgs<ARTrackedObject> eventArgs)
    {
        // Handle newly detected objects
        foreach (ARTrackedObject trackedObject in eventArgs.added)
        {
            OnObjectAdded(trackedObject);
        }
        
        // Handle updated objects
        foreach (ARTrackedObject trackedObject in eventArgs.updated)
        {
            OnObjectUpdated(trackedObject);
        }
        
        // Handle removed objects
        foreach (var removedKvp in eventArgs.removed)
        {
            ARTrackedObject trackedObject = removedKvp.Value;
            OnObjectRemoved(trackedObject);
        }
    }
    
    private void OnObjectAdded(ARTrackedObject trackedObject)
    {
        string objectName = trackedObject.referenceObject.name;
        
        if (debugMode)
        {
            Debug.Log($"New 3D object detected: {objectName}");
        }
        
        // Get the appropriate content prefab
        GameObject contentPrefab = GetContentPrefabForObject(objectName);
        
        if (contentPrefab != null)
        {
            // Instantiate content at the tracked object position
            GameObject spawnedObject = Instantiate(contentPrefab, trackedObject.transform);
            spawnedContent[trackedObject] = spawnedObject;
            
            // Add ObjectTrackingHandler if it doesn't exist
            ObjectTrackingHandler handler = spawnedObject.GetComponent<ObjectTrackingHandler>();
            if (handler == null)
            {
                handler = spawnedObject.AddComponent<ObjectTrackingHandler>();
            }
            
            if (debugMode)
            {
                Debug.Log($"Spawned content for 3D object: {objectName}");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning($"No content prefab found for 3D object: {objectName}");
            }
        }
    }
    
    private void OnObjectUpdated(ARTrackedObject trackedObject)
    {
        // Content position and rotation are automatically updated by parenting to trackedObject.transform
        
        if (debugMode && trackedObject.trackingState == TrackingState.Tracking)
        {
            Debug.Log($"3D Object updated: {trackedObject.referenceObject.name} - State: {trackedObject.trackingState}");
        }
    }
    
    private void OnObjectRemoved(ARTrackedObject trackedObject)
    {
        if (spawnedContent.TryGetValue(trackedObject, out GameObject spawnedObject))
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }
            spawnedContent.Remove(trackedObject);
            
            if (debugMode)
            {
                Debug.Log($"Removed content for 3D object: {trackedObject.referenceObject.name}");
            }
        }
    }
    
    private GameObject GetContentPrefabForObject(string objectName)
    {
        // First check if we have a specific mapping for this object
        if (objectToContentMap.TryGetValue(objectName, out GameObject specificPrefab))
        {
            return specificPrefab;
        }
        
        // Fall back to default prefab
        return defaultTrackedObjectPrefab;
    }
    
    private void LogPlatformSupport()
    {
        string platform = Application.platform.ToString();
        bool isSupported = false;
        
        #if UNITY_IOS
        isSupported = true;
        Debug.Log("3D Object Tracking: iOS platform detected - ARKit support available");
        #elif UNITY_ANDROID
        Debug.Log("3D Object Tracking: Android platform detected - Limited ARCore support (check device compatibility)");
        #else
        Debug.Log("3D Object Tracking: Platform not fully supported - iOS (ARKit) recommended");
        #endif
        
        if (!isSupported && debugMode)
        {
            Debug.LogWarning("For best 3D object tracking results, use iOS devices with ARKit support.");
        }
    }
    
    public void AddObjectContentMapping(string objectName, GameObject contentPrefab)
    {
        var existingMapping = objectContentMappings.Find(m => m.objectName == objectName);
        if (existingMapping != null)
        {
            existingMapping.contentPrefab = contentPrefab;
        }
        else
        {
            objectContentMappings.Add(new ObjectContentMapping { objectName = objectName, contentPrefab = contentPrefab });
        }
        
        BuildContentMapping();
    }
    
    public List<ARTrackedObject> GetCurrentlyTrackedObjects()
    {
        List<ARTrackedObject> trackedObjects = new List<ARTrackedObject>();
        
        if (trackedObjectManager != null && trackedObjectManager.trackables.count > 0)
        {
            foreach (var trackable in trackedObjectManager.trackables)
            {
                if (trackable.trackingState == TrackingState.Tracking)
                {
                    trackedObjects.Add(trackable);
                }
            }
        }
        
        return trackedObjects;
    }
    
    public void SetReferenceObjectLibrary(XRReferenceObjectLibrary library)
    {
        referenceObjectLibrary = library;
        if (trackedObjectManager != null)
        {
            trackedObjectManager.referenceLibrary = library;
        }
    }
}

[System.Serializable]
public class ObjectContentMapping
{
    public string objectName;
    public GameObject contentPrefab;
}