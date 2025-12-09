using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingController : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    public GameObject markerButton;      // appears over the marker
    public GameObject arContentRoot;     // content that shows after tapping

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var tracked in args.added)
            PositionMarker(tracked);

        foreach (var tracked in args.updated)
            PositionMarker(tracked);
    }

    void PositionMarker(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking)
            return;

        // place the marker button at the center of the image
        markerButton.SetActive(true);
        markerButton.transform.position = trackedImage.transform.position;
        markerButton.transform.rotation = trackedImage.transform.rotation;

        // keep AR content hidden until user taps
        arContentRoot.SetActive(false);
    }
}
