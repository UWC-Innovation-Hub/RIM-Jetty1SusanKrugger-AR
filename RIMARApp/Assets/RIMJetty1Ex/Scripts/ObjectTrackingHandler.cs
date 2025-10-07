using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectTrackingHandler : MonoBehaviour
{
    [Header("Trackable Content")]
    [SerializeField] private GameObject trackableContent;
    
    [Header("Tracking State Feedback")]
    [SerializeField] private bool showTrackingStateDebug = true;
    [SerializeField] private bool hideContentWhenNotTracking = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private Material trackingMaterial;
    [SerializeField] private Material notTrackingMaterial;
    [SerializeField] private Renderer[] renderers;
    
    private ARTrackedObject trackedObject;
    private TrackingState lastTrackingState;
    
    void Start()
    {
        // Get the ARTrackedObject component
        trackedObject = GetComponent<ARTrackedObject>();
        
        if (trackedObject == null)
        {
            Debug.LogError("ObjectTrackingHandler requires an ARTrackedObject component!");
            return;
        }
        
        // Find renderers if not assigned
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
        
        // Initially set up based on current tracking state
        if (trackableContent != null && hideContentWhenNotTracking)
        {
            trackableContent.SetActive(trackedObject.trackingState == TrackingState.Tracking);
        }
    }
    
    void Update()
    {
        if (trackedObject == null) return;
        
        // Check if tracking state has changed
        if (trackedObject.trackingState != lastTrackingState)
        {
            OnTrackingStateChanged(trackedObject.trackingState);
            lastTrackingState = trackedObject.trackingState;
        }
        
        // Update content and visuals based on tracking state
        UpdateContentVisibility();
        UpdateVisualFeedback();
    }
    
    private void OnTrackingStateChanged(TrackingState newState)
    {
        if (showTrackingStateDebug)
        {
            Debug.Log($"3D Object '{trackedObject.referenceObject.name}' tracking state: {newState}");
        }
        
        switch (newState)
        {
            case TrackingState.None:
                OnObjectLost();
                break;
            case TrackingState.Limited:
                OnObjectLimited();
                break;
            case TrackingState.Tracking:
                OnObjectFound();
                break;
        }
    }
    
    private void OnObjectFound()
    {
        if (showTrackingStateDebug)
        {
            Debug.Log($"3D Object '{trackedObject.referenceObject.name}' found and being tracked!");
        }
    }
    
    private void OnObjectLimited()
    {
        if (showTrackingStateDebug)
        {
            Debug.Log($"3D Object '{trackedObject.referenceObject.name}' tracking is limited");
        }
    }
    
    private void OnObjectLost()
    {
        if (showTrackingStateDebug)
        {
            Debug.Log($"3D Object '{trackedObject.referenceObject.name}' lost tracking");
        }
    }
    
    private void UpdateContentVisibility()
    {
        if (trackableContent == null || !hideContentWhenNotTracking) return;
        
        // Show content only when object is being tracked
        bool shouldShowContent = trackedObject.trackingState == TrackingState.Tracking;
        
        if (trackableContent.activeSelf != shouldShowContent)
        {
            trackableContent.SetActive(shouldShowContent);
        }
    }
    
    private void UpdateVisualFeedback()
    {
        if (renderers == null || renderers.Length == 0) return;
        
        Material materialToUse = trackedObject.trackingState == TrackingState.Tracking ? trackingMaterial : notTrackingMaterial;
        
        if (materialToUse != null)
        {
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.material = materialToUse;
                }
            }
        }
    }
    
    public void SetTrackableContent(GameObject content)
    {
        trackableContent = content;
    }
    
    public TrackingState GetTrackingState()
    {
        return trackedObject != null ? trackedObject.trackingState : TrackingState.None;
    }
    
    public ARTrackedObject GetTrackedObject()
    {
        return trackedObject;
    }
    
    public Vector3 GetObjectSize()
    {
        // Note: XRReferenceObject doesn't expose size property in Unity 6
        // Size information may be available through other means or provider-specific data
        if (trackedObject?.referenceObject != null)
        {
            // Return bounds size if available from the trackable
            if (trackedObject.transform != null)
            {
                // Try to get bounds from rendered content
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds combinedBounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        combinedBounds.Encapsulate(renderers[i].bounds);
                    }
                    return combinedBounds.size;
                }
            }
        }
        return Vector3.zero;
    }
    
    public void OnTrackingQualityChanged(TrackingState state)
    {
        // Custom logic for when tracking quality changes
        switch (state)
        {
            case TrackingState.Tracking:
                // Object is being tracked well
                break;
            case TrackingState.Limited:
                // Tracking is limited - object might be partially occluded
                break;
            case TrackingState.None:
                // Object is not being tracked
                break;
        }
    }
}