using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class AndroidTrackingAlternative : MonoBehaviour
{
    [Header("Android-Optimized Tracking")]
    [SerializeField] private TrackingMode preferredMode = TrackingMode.ImageTracking;
    
    [Header("Image Tracking (Recommended for Android)")]
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    [SerializeField] private ARTrackedImageManager imageManager;
    
    [Header("Object Tracking (Limited Android Support)")]
    [SerializeField] private XRReferenceObjectLibrary objectLibrary;
    [SerializeField] private ARTrackedObjectManager objectManager;
    
    [Header("Content")]
    [SerializeField] private GameObject trackableObjectPrefab; // Your 3D scanned model
    [SerializeField] private List<TrackingContent> contentMappings = new List<TrackingContent>();
    
    [Header("Android Optimization")]
    [SerializeField] private bool checkDeviceCompatibility = true;
    [SerializeField] private bool fallbackToImageTracking = true;
    
    private Dictionary<TrackableId, GameObject> spawnedContent = new Dictionary<TrackableId, GameObject>();
    private bool deviceSupportsObjectTracking = false;
    
    public enum TrackingMode
    {
        ImageTracking,      // Recommended for Android
        ObjectTracking,     // Limited Android support
        Hybrid              // Both (if device supports)
    }
    
    [System.Serializable]
    public class TrackingContent
    {
        public string trackableName;
        public GameObject contentPrefab;
        public TrackingMode supportedModes;
    }
    
    void Start()
    {
        CheckDeviceCompatibility();
        SetupTrackingBasedOnPlatform();
        InitializeTrackingManagers();
    }
    
    void CheckDeviceCompatibility()
    {
        #if UNITY_ANDROID
        // ARCore object tracking is only supported on specific high-end devices
        deviceSupportsObjectTracking = CheckARCoreObjectTrackingSupport();
        
        if (!deviceSupportsObjectTracking)
        {
            Debug.LogWarning("⚠️ Device may not support ARCore object tracking");
            Debug.LogWarning("📱 Falling back to Image Tracking (more reliable on Android)");
            
            if (fallbackToImageTracking && preferredMode == TrackingMode.ObjectTracking)
            {
                preferredMode = TrackingMode.ImageTracking;
            }
        }
        #elif UNITY_IOS
        deviceSupportsObjectTracking = true; // ARKit has good object tracking support
        #else
        deviceSupportsObjectTracking = false; // Editor/other platforms
        #endif
        
        Debug.Log($"🔍 Device object tracking support: {deviceSupportsObjectTracking}");
        Debug.Log($"🎯 Selected tracking mode: {preferredMode}");
    }
    
    bool CheckARCoreObjectTrackingSupport()
    {
        // ARCore object tracking requires:
        // 1. High-end Android device
        // 2. ARCore 1.16+ 
        // 3. Specific chipsets (Snapdragon 8xx series, etc.)
        
        // For most Android devices, return false
        // Only enable for known compatible devices
        
        string deviceModel = SystemInfo.deviceModel.ToLower();
        string[] compatibleModels = {
            "pixel 4", "pixel 5", "pixel 6", "pixel 7", "pixel 8",
            "galaxy s20", "galaxy s21", "galaxy s22", "galaxy s23",
            "galaxy note20", "galaxy note21"
        };
        
        foreach (string model in compatibleModels)
        {
            if (deviceModel.Contains(model))
            {
                return true;
            }
        }
        
        return false; // Most Android devices don't support it well
    }
    
    void SetupTrackingBasedOnPlatform()
    {
        switch (preferredMode)
        {
            case TrackingMode.ImageTracking:
                EnableImageTracking();
                DisableObjectTracking();
                break;
                
            case TrackingMode.ObjectTracking:
                if (deviceSupportsObjectTracking)
                {
                    DisableImageTracking();
                    EnableObjectTracking();
                }
                else
                {
                    Debug.LogWarning("🔄 Object tracking not supported, switching to Image tracking");
                    EnableImageTracking();
                    DisableObjectTracking();
                }
                break;
                
            case TrackingMode.Hybrid:
                EnableImageTracking();
                if (deviceSupportsObjectTracking)
                {
                    EnableObjectTracking();
                }
                break;
        }
    }
    
    void EnableImageTracking()
    {
        if (imageManager == null)
            imageManager = FindFirstObjectByType<ARTrackedImageManager>();
            
        if (imageManager != null)
        {
            imageManager.enabled = true;
            imageManager.referenceLibrary = imageLibrary;
            imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
            Debug.Log("✅ Image tracking enabled");
        }
    }
    
    void DisableImageTracking()
    {
        if (imageManager != null)
        {
            imageManager.enabled = false;
            imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
            Debug.Log("⏸️ Image tracking disabled");
        }
    }
    
    void EnableObjectTracking()
    {
        if (objectManager == null)
            objectManager = FindFirstObjectByType<ARTrackedObjectManager>();
            
        if (objectManager != null)
        {
            objectManager.enabled = true;
            objectManager.referenceLibrary = objectLibrary;
            objectManager.trackablesChanged.AddListener(OnTrackedObjectsChanged);
            Debug.Log("✅ Object tracking enabled");
        }
    }
    
    void DisableObjectTracking()
    {
        if (objectManager != null)
        {
            objectManager.enabled = false;
            objectManager.trackablesChanged.RemoveListener(OnTrackedObjectsChanged);
            Debug.Log("⏸️ Object tracking disabled");
        }
    }
    
    void InitializeTrackingManagers()
    {
        // Auto-find managers if not assigned
        if (imageManager == null)
            imageManager = FindFirstObjectByType<ARTrackedImageManager>();
            
        if (objectManager == null)
            objectManager = FindFirstObjectByType<ARTrackedObjectManager>();
    }
    
    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            OnImageDetected(trackedImage);
        }
        
        foreach (var trackedImage in eventArgs.updated)
        {
            OnImageUpdated(trackedImage);
        }
        
        foreach (var removedKvp in eventArgs.removed)
        {
            OnImageLost(removedKvp.Value);
        }
    }
    
    void OnTrackedObjectsChanged(ARTrackablesChangedEventArgs<ARTrackedObject> eventArgs)
    {
        foreach (var trackedObject in eventArgs.added)
        {
            OnObjectDetected(trackedObject);
        }
        
        foreach (var trackedObject in eventArgs.updated)
        {
            OnObjectUpdated(trackedObject);
        }
        
        foreach (var removedKvp in eventArgs.removed)
        {
            OnObjectLost(removedKvp.Value);
        }
    }
    
    void OnImageDetected(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking) return;
        
        string imageName = trackedImage.referenceImage.name;
        GameObject content = GetContentForTrackable(imageName);
        
        if (content != null)
        {
            GameObject spawnedObject = Instantiate(content, trackedImage.transform);
            spawnedContent[trackedImage.trackableId] = spawnedObject;
            
            Debug.Log($"📱 Image detected: {imageName}");
        }
    }
    
    void OnObjectDetected(ARTrackedObject trackedObject)
    {
        if (trackedObject.trackingState != TrackingState.Tracking) return;
        
        string objectName = trackedObject.referenceObject.name;
        GameObject content = GetContentForTrackable(objectName);
        
        if (content != null)
        {
            GameObject spawnedObject = Instantiate(content, trackedObject.transform);
            spawnedContent[trackedObject.trackableId] = spawnedObject;
            
            Debug.Log($"📦 Object detected: {objectName}");
        }
    }
    
    void OnImageUpdated(ARTrackedImage trackedImage)
    {
        if (spawnedContent.TryGetValue(trackedImage.trackableId, out GameObject content))
        {
            content.SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }
    }
    
    void OnObjectUpdated(ARTrackedObject trackedObject)
    {
        if (spawnedContent.TryGetValue(trackedObject.trackableId, out GameObject content))
        {
            content.SetActive(trackedObject.trackingState == TrackingState.Tracking);
        }
    }
    
    void OnImageLost(ARTrackedImage trackedImage)
    {
        if (spawnedContent.TryGetValue(trackedImage.trackableId, out GameObject content))
        {
            Destroy(content);
            spawnedContent.Remove(trackedImage.trackableId);
        }
    }
    
    void OnObjectLost(ARTrackedObject trackedObject)
    {
        if (spawnedContent.TryGetValue(trackedObject.trackableId, out GameObject content))
        {
            Destroy(content);
            spawnedContent.Remove(trackedObject.trackableId);
        }
    }
    
    GameObject GetContentForTrackable(string trackableName)
    {
        var mapping = contentMappings.Find(m => m.trackableName == trackableName);
        if (mapping != null && mapping.contentPrefab != null)
        {
            return mapping.contentPrefab;
        }
        
        // Fallback to default prefab
        return trackableObjectPrefab;
    }
    
    public void SwitchTrackingMode(TrackingMode newMode)
    {
        preferredMode = newMode;
        SetupTrackingBasedOnPlatform();
    }
    
    void OnDestroy()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
            
        if (objectManager != null)
            objectManager.trackablesChanged.RemoveListener(OnTrackedObjectsChanged);
    }
}