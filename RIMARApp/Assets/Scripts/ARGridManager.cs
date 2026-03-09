using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARGridManager : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public GameObject gridPrefab;

    private GameObject activeGrid;

    void OnEnable()
    {
        imageManager.trackablesChanged.AddListener(OnImagesChanged);
    }

    void OnDisable()
    {
        imageManager.trackablesChanged.RemoveListener(OnImagesChanged);
    }

    void OnImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var img in args.added)
        {
            PlaceGrid(img);
        }
    }

    void PlaceGrid(ARTrackedImage image)
    {
        if (activeGrid != null)
            Destroy(activeGrid);

        activeGrid = Instantiate(gridPrefab);

        activeGrid.transform.position =
            image.transform.position + image.transform.forward * 1.3f;

        activeGrid.transform.rotation = image.transform.rotation;

        activeGrid.transform.SetParent(null);
    }
}