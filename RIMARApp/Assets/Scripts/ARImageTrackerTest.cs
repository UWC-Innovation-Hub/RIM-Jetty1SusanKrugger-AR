using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTrackerTest : MonoBehaviour
{
    public ARTrackedImageManager imageManager;

    private void Start()
    {
        Debug.Log("=== AR IMAGE TRACKER TEST STARTED ===");
        
        if (imageManager == null)
        {
            Debug.LogError("❌ ARTrackedImageManager is NULL! Assign it in Inspector!");
            return;
        }

        if (imageManager.referenceLibrary == null)
        {
            Debug.LogError("❌ Reference Image Library is NULL!");
            return;
        }

        Debug.Log($"✅ Reference Library found");
        Debug.Log($"✅ Number of images in library: {imageManager.referenceLibrary.count}");

        for (int i = 0; i < imageManager.referenceLibrary.count; i++)
        {
            var image = imageManager.referenceLibrary[i];
            Debug.Log($"  Image {i}: {image.name} | Size: {image.size}");
        }
    }

    private void OnEnable()
    {
        if (imageManager != null)
        {
            imageManager.trackedImagesChanged += OnTrackedImagesChanged;
            Debug.Log("✅ Subscribed to trackedImagesChanged event");
        }
    }

    private void OnDisable()
    {
        if (imageManager != null)
        {
            imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            Debug.Log($"🎯 IMAGE DETECTED! Name: {trackedImage.referenceImage.name} | Position: {trackedImage.transform.position}");
        }

        foreach (var trackedImage in args.updated)
        {
            Debug.Log($"🔄 IMAGE UPDATED: {trackedImage.referenceImage.name} | Tracking State: {trackedImage.trackingState}");
        }

        foreach (var trackedImage in args.removed)
        {
            Debug.Log($"❌ IMAGE LOST: {trackedImage.referenceImage.name}");
        }
    }
}
