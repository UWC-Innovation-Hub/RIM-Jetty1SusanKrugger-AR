using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections;

public class SimpleObjectRecognition : MonoBehaviour
{
    [Header("Recognition Method")]
    [SerializeField] private RecognitionMode mode = RecognitionMode.MultiViewImageTracking;
    
    [Header("Multi-View Image Tracking (Recommended)")]
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private float imageTrackingConfidence = 0.8f;
    
    [Header("Template Matching (Advanced)")]
    [SerializeField] private Texture2D[] templateImages;
    [SerializeField] private float matchingThreshold = 0.7f;
    [SerializeField] private bool useTemplateMatching = false;
    
    [Header("Content to Anchor")]
    [SerializeField] private GameObject contentPrefab; // Your TrackableObject or augmentation content
    [SerializeField] private Vector3 contentOffset = Vector3.zero;
    [SerializeField] private Vector3 contentRotation = Vector3.zero;
    [SerializeField] private float contentScale = 1f;
    
    [Header("Tracking Stability")]
    [SerializeField] private float trackingLostTimeout = 2f;
    [SerializeField] private bool smoothTracking = true;
    [SerializeField] private float smoothingSpeed = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private UnityEngine.UI.Text debugText;
    
    private Dictionary<TrackableId, TrackedObjectData> trackedObjects = new Dictionary<TrackableId, TrackedObjectData>();
    private Camera arCamera;
    private WebCamTexture webCamTexture;
    
    public enum RecognitionMode
    {
        MultiViewImageTracking,     // Multiple images of the same object (RECOMMENDED)
        TemplateMatching,          // Simple computer vision approach
        HybridApproach             // Combines both methods
    }
    
    [System.Serializable]
    public class TrackedObjectData
    {
        public GameObject spawnedContent;
        public Transform targetTransform;
        public float lastSeenTime;
        public Vector3 lastKnownPosition;
        public Quaternion lastKnownRotation;
        public bool isTracking;
        public float confidence;
    }
    
    void Start()
    {
        SetupRecognitionSystem();
        InitializeComponents();
    }
    
    void SetupRecognitionSystem()
    {
        // Get AR camera
        arCamera = Camera.main;
        if (arCamera == null)
            arCamera = FindFirstObjectByType<Camera>();
        
        // Setup based on selected mode
        switch (mode)
        {
            case RecognitionMode.MultiViewImageTracking:
                SetupImageTracking();
                break;
                
            case RecognitionMode.TemplateMatching:
                SetupTemplateMatching();
                break;
                
            case RecognitionMode.HybridApproach:
                SetupImageTracking();
                SetupTemplateMatching();
                break;
        }
        
        Debug.Log($"🎯 Simple Object Recognition initialized with mode: {mode}");
    }
    
    void SetupImageTracking()
    {
        // Find or setup ARTrackedImageManager
        if (imageManager == null)
            imageManager = FindFirstObjectByType<ARTrackedImageManager>();
        
        if (imageManager != null)
        {
            imageManager.enabled = true;
            if (imageLibrary != null)
            {
                imageManager.referenceLibrary = imageLibrary;
                imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
                Debug.Log($"✅ Image tracking setup complete. Reference images: {imageLibrary.count}");
            }
            else
            {
                Debug.LogWarning("⚠️ No reference image library assigned!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ ARTrackedImageManager not found!");
        }
    }
    
    void SetupTemplateMatching()
    {
        if (useTemplateMatching && templateImages.Length > 0)
        {
            // Initialize webcam for template matching
            if (WebCamTexture.devices.Length > 0)
            {
                webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name);
                webCamTexture.Play();
                Debug.Log("✅ Template matching webcam initialized");
            }
            
            // Start template matching coroutine
            StartCoroutine(TemplateMatchingLoop());
        }
    }
    
    void InitializeComponents()
    {
        // Setup debug UI
        if (debugText == null)
        {
            var debugUI = GameObject.Find("Debug Text");
            if (debugUI != null)
                debugText = debugUI.GetComponent<UnityEngine.UI.Text>();
        }
    }
    
    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Handle newly detected images
        foreach (var trackedImage in eventArgs.added)
        {
            OnObjectDetected(trackedImage);
        }
        
        // Handle updated images
        foreach (var trackedImage in eventArgs.updated)
        {
            OnObjectUpdated(trackedImage);
        }
        
        // Handle lost images
        foreach (var removedKvp in eventArgs.removed)
        {
            OnObjectLost(removedKvp.Value.trackableId);
        }
    }
    
    void OnObjectDetected(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking) return;
        
        Debug.Log($"🎯 Object detected: {trackedImage.referenceImage.name}");
        
        // Create new tracked object data
        var objectData = new TrackedObjectData
        {
            targetTransform = trackedImage.transform,
            lastSeenTime = Time.time,
            lastKnownPosition = trackedImage.transform.position,
            lastKnownRotation = trackedImage.transform.rotation,
            isTracking = true,
            confidence = 1.0f
        };
        
        // Spawn content
        if (contentPrefab != null)
        {
            objectData.spawnedContent = Instantiate(contentPrefab);
            SetupSpawnedContent(objectData.spawnedContent, trackedImage.transform);
        }
        
        trackedObjects[trackedImage.trackableId] = objectData;
        
        UpdateDebugInfo($"Detected: {trackedImage.referenceImage.name}");
    }
    
    void OnObjectUpdated(ARTrackedImage trackedImage)
    {
        if (!trackedObjects.TryGetValue(trackedImage.trackableId, out TrackedObjectData objectData))
            return;
        
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // Update tracking data
            objectData.isTracking = true;
            objectData.lastSeenTime = Time.time;
            objectData.confidence = 1.0f;
            
            if (smoothTracking && objectData.spawnedContent != null)
            {
                // Smooth position and rotation updates
                Vector3 targetPos = trackedImage.transform.position + trackedImage.transform.TransformDirection(contentOffset);
                Quaternion targetRot = trackedImage.transform.rotation * Quaternion.Euler(contentRotation);
                
                objectData.spawnedContent.transform.position = Vector3.Lerp(
                    objectData.spawnedContent.transform.position, 
                    targetPos, 
                    Time.deltaTime * smoothingSpeed
                );
                
                objectData.spawnedContent.transform.rotation = Quaternion.Lerp(
                    objectData.spawnedContent.transform.rotation, 
                    targetRot, 
                    Time.deltaTime * smoothingSpeed
                );
            }
            else if (objectData.spawnedContent != null)
            {
                // Direct position update
                UpdateContentTransform(objectData.spawnedContent, trackedImage.transform);
            }
            
            objectData.lastKnownPosition = trackedImage.transform.position;
            objectData.lastKnownRotation = trackedImage.transform.rotation;
        }
        else
        {
            // Tracking lost
            objectData.isTracking = false;
            objectData.confidence = 0.5f;
        }
        
        UpdateDebugInfo($"Tracking: {trackedImage.referenceImage.name} - State: {trackedImage.trackingState}");
    }
    
    void OnObjectLost(TrackableId trackableId)
    {
        if (trackedObjects.TryGetValue(trackableId, out TrackedObjectData objectData))
        {
            Debug.Log("📍 Object tracking lost");
            
            objectData.isTracking = false;
            objectData.confidence = 0f;
            
            // Start timeout coroutine
            StartCoroutine(HandleTrackingLost(trackableId));
        }
    }
    
    IEnumerator HandleTrackingLost(TrackableId trackableId)
    {
        yield return new WaitForSeconds(trackingLostTimeout);
        
        if (trackedObjects.TryGetValue(trackableId, out TrackedObjectData objectData))
        {
            if (!objectData.isTracking)
            {
                // Remove content after timeout
                if (objectData.spawnedContent != null)
                {
                    Destroy(objectData.spawnedContent);
                }
                trackedObjects.Remove(trackableId);
                
                Debug.Log("🗑️ Object removed after tracking timeout");
                UpdateDebugInfo("Object lost");
            }
        }
    }
    
    void SetupSpawnedContent(GameObject content, Transform trackingTransform)
    {
        // Position content relative to tracked object
        UpdateContentTransform(content, trackingTransform);
        
        // Apply scale
        content.transform.localScale = Vector3.one * contentScale;
        
        // Add any additional setup
        var billboard = content.GetComponent<Billboard>();
        if (billboard == null && arCamera != null)
        {
            billboard = content.AddComponent<Billboard>();
        }
    }
    
    void UpdateContentTransform(GameObject content, Transform trackingTransform)
    {
        content.transform.position = trackingTransform.position + trackingTransform.TransformDirection(contentOffset);
        content.transform.rotation = trackingTransform.rotation * Quaternion.Euler(contentRotation);
    }
    
    IEnumerator TemplateMatchingLoop()
    {
        while (useTemplateMatching && webCamTexture != null)
        {
            if (webCamTexture.isPlaying && templateImages.Length > 0)
            {
                // Perform template matching (simplified)
                PerformTemplateMatching();
            }
            
            yield return new WaitForSeconds(0.1f); // Check 10 times per second
        }
    }
    
    void PerformTemplateMatching()
    {
        // This is a simplified template matching approach
        // In a real implementation, you'd use more sophisticated computer vision
        
        if (webCamTexture == null || !webCamTexture.isPlaying) return;
        
        // Get current camera frame
        Color32[] pixels = webCamTexture.GetPixels32();
        
        // Simple template matching logic would go here
        // For now, we'll just detect if there's significant movement or change
        
        // This is where you'd implement:
        // 1. Feature detection (corners, edges)
        // 2. Template correlation
        // 3. Object boundary detection
        
        // Placeholder for template matching results
        bool objectDetected = false; // Replace with actual detection logic
        
        if (objectDetected)
        {
            // Create a virtual tracking point
            CreateVirtualTrackingPoint();
        }
    }
    
    void CreateVirtualTrackingPoint()
    {
        // Create a virtual anchor point in front of the camera
        Vector3 cameraForward = arCamera.transform.forward;
        Vector3 anchorPosition = arCamera.transform.position + cameraForward * 0.5f;
        
        // Create temporary tracking data
        var tempId = new TrackableId(0, 1); // Temporary ID for template matching
        
        if (!trackedObjects.ContainsKey(tempId))
        {
            var objectData = new TrackedObjectData
            {
                lastSeenTime = Time.time,
                lastKnownPosition = anchorPosition,
                lastKnownRotation = Quaternion.LookRotation(cameraForward),
                isTracking = true,
                confidence = matchingThreshold
            };
            
            if (contentPrefab != null)
            {
                objectData.spawnedContent = Instantiate(contentPrefab);
                objectData.spawnedContent.transform.position = anchorPosition;
                objectData.spawnedContent.transform.rotation = Quaternion.LookRotation(cameraForward);
            }
            
            trackedObjects[tempId] = objectData;
        }
    }
    
    void UpdateDebugInfo(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"🔍 {message}");
            
            if (debugText != null)
            {
                debugText.text = $"Recognition: {mode}\n{message}\nObjects: {trackedObjects.Count}";
            }
        }
    }
    
    public void SwitchRecognitionMode(int modeIndex)
    {
        mode = (RecognitionMode)modeIndex;
        
        // Clean up current tracking
        ClearAllTrackedObjects();
        
        // Reinitialize with new mode
        SetupRecognitionSystem();
        
        Debug.Log($"🔄 Switched to recognition mode: {mode}");
    }
    
    void ClearAllTrackedObjects()
    {
        foreach (var kvp in trackedObjects)
        {
            if (kvp.Value.spawnedContent != null)
            {
                Destroy(kvp.Value.spawnedContent);
            }
        }
        trackedObjects.Clear();
    }
    
    void OnDestroy()
    {
        // Clean up
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }
        
        ClearAllTrackedObjects();
    }
    
    // Public methods for UI controls
    public void ToggleSmoothing() { smoothTracking = !smoothTracking; }
    public void SetContentScale(float scale) { contentScale = scale; }
    public void SetTrackingTimeout(float timeout) { trackingLostTimeout = timeout; }
}