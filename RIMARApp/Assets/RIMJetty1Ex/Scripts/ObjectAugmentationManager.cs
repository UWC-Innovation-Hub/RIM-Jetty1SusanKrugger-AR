using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectAugmentationManager : MonoBehaviour
{
    [Header("Reference Object Library")]
    [SerializeField] private XRReferenceObjectLibrary referenceObjectLibrary;
    
    [Header("Augmentation Content")]
    [SerializeField] private List<ObjectAugmentation> augmentations = new List<ObjectAugmentation>();
    
    [Header("AR Components")]
    [SerializeField] private ARTrackedObjectManager trackedObjectManager;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private Dictionary<string, GameObject> augmentationMapping = new Dictionary<string, GameObject>();
    private Dictionary<TrackableId, GameObject> spawnedContent = new Dictionary<TrackableId, GameObject>();
    
    [System.Serializable]
    public class ObjectAugmentation
    {
        [Header("Object Identification")]
        public string referenceObjectName;
        
        [Header("Augmentation Type")]
        public AugmentationType type = AugmentationType.DigitalContent;
        
        [Header("Content")]
        public GameObject augmentationPrefab;
        
        [Header("Positioning")]
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;
        public Vector3 scaleMultiplier = Vector3.one;
        
        [Header("Behavior")]
        public bool hideRealObject = false;
        public bool followObjectMovement = true;
        public bool scaleWithDistance = false;
    }
    
    public enum AugmentationType
    {
        DigitalContent,      // Add digital objects around the real object
        Information,         // Show UI/text information
        Effects,            // Add particle effects, lighting
        Occlusion,          // Invisible collision for physics
        Replacement         // Replace real object (your current setup)
    }
    
    void Start()
    {
        if (trackedObjectManager == null)
        {
            trackedObjectManager = FindFirstObjectByType<ARTrackedObjectManager>();
        }
        
        if (trackedObjectManager == null)
        {
            Debug.LogError("ARTrackedObjectManager not found!");
            enabled = false;
            return;
        }
        
        if (referenceObjectLibrary != null)
        {
            trackedObjectManager.referenceLibrary = referenceObjectLibrary;
        }
        
        BuildAugmentationMapping();
        
        if (enabled && trackedObjectManager.enabled)
        {
            trackedObjectManager.trackablesChanged.AddListener(OnTrackedObjectsChanged);
        }
        
        if (debugMode)
        {
            Debug.Log($"ObjectAugmentationManager initialized with {augmentations.Count} augmentations");
        }
    }
    
    void BuildAugmentationMapping()
    {
        augmentationMapping.Clear();
        
        foreach (var augmentation in augmentations)
        {
            if (!string.IsNullOrEmpty(augmentation.referenceObjectName) && augmentation.augmentationPrefab != null)
            {
                augmentationMapping[augmentation.referenceObjectName] = augmentation.augmentationPrefab;
            }
        }
    }
    
    void OnTrackedObjectsChanged(ARTrackablesChangedEventArgs<ARTrackedObject> eventArgs)
    {
        foreach (ARTrackedObject trackedObject in eventArgs.added)
        {
            OnObjectAdded(trackedObject);
        }
        
        foreach (ARTrackedObject trackedObject in eventArgs.updated)
        {
            OnObjectUpdated(trackedObject);
        }
        
        foreach (var removedKvp in eventArgs.removed)
        {
            ARTrackedObject trackedObject = removedKvp.Value;
            OnObjectRemoved(trackedObject);
        }
    }
    
    void OnObjectAdded(ARTrackedObject trackedObject)
    {
        if (trackedObject.trackingState != TrackingState.Tracking)
            return;
            
        string objectName = GetObjectName(trackedObject);
        var augmentation = GetAugmentationForObject(objectName);
        
        if (augmentation != null && augmentation.augmentationPrefab != null)
        {
            GameObject spawnedContent = CreateAugmentationContent(trackedObject, augmentation);
            spawnedContent.SetActive(true);
            
            this.spawnedContent[trackedObject.trackableId] = spawnedContent;
            
            if (debugMode)
            {
                Debug.Log($"Added augmentation for object: {objectName}");
            }
        }
    }
    
    void OnObjectUpdated(ARTrackedObject trackedObject)
    {
        if (spawnedContent.TryGetValue(trackedObject.trackableId, out GameObject content))
        {
            if (trackedObject.trackingState == TrackingState.Tracking)
            {
                content.SetActive(true);
                UpdateAugmentationPosition(trackedObject, content);
            }
            else
            {
                content.SetActive(false);
            }
        }
    }
    
    void OnObjectRemoved(ARTrackedObject trackedObject)
    {
        if (spawnedContent.TryGetValue(trackedObject.trackableId, out GameObject content))
        {
            if (debugMode)
            {
                Debug.Log($"Removing augmentation for object: {GetObjectName(trackedObject)}");
            }
            
            Destroy(content);
            spawnedContent.Remove(trackedObject.trackableId);
        }
    }
    
    GameObject CreateAugmentationContent(ARTrackedObject trackedObject, ObjectAugmentation augmentation)
    {
        GameObject content = Instantiate(augmentation.augmentationPrefab);
        content.transform.SetParent(trackedObject.transform, false);
        
        // Apply positioning offsets
        content.transform.localPosition = augmentation.positionOffset;
        content.transform.localRotation = Quaternion.Euler(augmentation.rotationOffset);
        content.transform.localScale = Vector3.Scale(content.transform.localScale, augmentation.scaleMultiplier);
        
        // Apply augmentation-specific behavior
        ApplyAugmentationBehavior(content, augmentation, trackedObject);
        
        return content;
    }
    
    void ApplyAugmentationBehavior(GameObject content, ObjectAugmentation augmentation, ARTrackedObject trackedObject)
    {
        switch (augmentation.type)
        {
            case AugmentationType.DigitalContent:
                // Normal digital content, fully visible
                break;
                
            case AugmentationType.Information:
                // Ensure UI elements face the camera
                var billboard = content.GetComponent<Billboard>();
                if (billboard == null)
                {
                    billboard = content.AddComponent<Billboard>();
                }
                break;
                
            case AugmentationType.Effects:
                // Start particle systems or effects
                var particleSystems = content.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    ps.Play();
                }
                break;
                
            case AugmentationType.Occlusion:
                // Make invisible but keep colliders for physics
                var renderers = content.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.enabled = false;
                }
                break;
                
            case AugmentationType.Replacement:
                // Your current setup - replace the real object
                break;
        }
        
        if (augmentation.hideRealObject)
        {
            // This would require advanced occlusion techniques
            // For now, just a placeholder
            Debug.Log("Real object occlusion not implemented yet");
        }
    }
    
    void UpdateAugmentationPosition(ARTrackedObject trackedObject, GameObject content)
    {
        var augmentation = GetAugmentationForObject(GetObjectName(trackedObject));
        if (augmentation != null && augmentation.followObjectMovement)
        {
            // Content automatically follows because it's parented to trackedObject
            // Additional scaling or positioning logic can go here
        }
    }
    
    string GetObjectName(ARTrackedObject trackedObject)
    {
        return trackedObject.referenceObject.name;
    }
    
    ObjectAugmentation GetAugmentationForObject(string objectName)
    {
        return augmentations.Find(aug => aug.referenceObjectName == objectName);
    }
    
    public List<ARTrackedObject> GetCurrentlyTrackedObjects()
    {
        List<ARTrackedObject> currentObjects = new List<ARTrackedObject>();
        if (trackedObjectManager != null)
        {
            foreach (var trackable in trackedObjectManager.trackables)
            {
                if (trackable.trackingState == TrackingState.Tracking)
                {
                    currentObjects.Add(trackable);
                }
            }
        }
        return currentObjects;
    }
    
    void OnEnable()
    {
        if (trackedObjectManager != null && trackedObjectManager.enabled)
        {
            trackedObjectManager.trackablesChanged.AddListener(OnTrackedObjectsChanged);
        }
    }
    
    void OnDisable()
    {
        if (trackedObjectManager != null)
        {
            trackedObjectManager.trackablesChanged.RemoveListener(OnTrackedObjectsChanged);
        }
    }
    
    void OnDestroy()
    {
        if (trackedObjectManager != null)
        {
            trackedObjectManager.trackablesChanged.RemoveListener(OnTrackedObjectsChanged);
        }
    }
}