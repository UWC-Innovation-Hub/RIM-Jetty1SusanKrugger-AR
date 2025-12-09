using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingHandler : MonoBehaviour
{
    [Header("Trackable Content")]
    [SerializeField] private GameObject trackableContent;
    
    [Header("Tracking State Feedback")]
    [SerializeField] private bool showTrackingStateDebug = true;
    
    private ARTrackedImage trackedImage;
    private TrackingState lastTrackingState;
    
    void Start()
    {
        // Get the ARTrackedImage component
        trackedImage = GetComponent<ARTrackedImage>();
        
        if (trackedImage == null)
        {
            Debug.LogError("ImageTrackingHandler requires an ARTrackedImage component!");
            return;
        }
        
        // Initially hide the content
        if (trackableContent != null)
        {
            trackableContent.SetActive(false);
        }
    }
    
    void Update()
    {
        if (trackedImage == null) return;
        
        // Check if tracking state has changed
        if (trackedImage.trackingState != lastTrackingState)
        {
            OnTrackingStateChanged(trackedImage.trackingState);
            lastTrackingState = trackedImage.trackingState;
        }
        
        // Update content based on tracking state
        UpdateContentVisibility();
    }
    
    private void OnTrackingStateChanged(TrackingState newState)
    {
        if (showTrackingStateDebug)
        {
            Debug.Log($"Image '{trackedImage.referenceImage.name}' tracking state: {newState}");
        }
        
        switch (newState)
        {
            case TrackingState.None:
                OnImageLost();
                break;
            case TrackingState.Limited:
                OnImageLimited();
                break;
            case TrackingState.Tracking:
                OnImageFound();
                break;
        }
    }
    
    private void OnImageFound()
    {
        if (showTrackingStateDebug)
        {
            Debug.Log($"Image '{trackedImage.referenceImage.name}' found and being tracked!");
        }
    }
    
    private void OnImageLimited()
    {
        if (showTrackingStateDebug)
        {
            Debug.Log($"Image '{trackedImage.referenceImage.name}' tracking is limited");
        }
    }
    
    private void OnImageLost()
    {
        if (showTrackingStateDebug)
        {
            Debug.Log($"Image '{trackedImage.referenceImage.name}' lost tracking");
        }
    }
    
    private void UpdateContentVisibility()
    {
        if (trackableContent == null) return;
        
        // Show content only when image is being tracked
        bool shouldShowContent = trackedImage.trackingState == TrackingState.Tracking;
        
        if (trackableContent.activeSelf != shouldShowContent)
        {
            trackableContent.SetActive(shouldShowContent);
        }
    }
    
    public void SetTrackableContent(GameObject content)
    {
        trackableContent = content;
    }
    
    public TrackingState GetTrackingState()
    {
        return trackedImage != null ? trackedImage.trackingState : TrackingState.None;
    }
    
    public ARTrackedImage GetTrackedImage()
    {
        return trackedImage;
    }
}