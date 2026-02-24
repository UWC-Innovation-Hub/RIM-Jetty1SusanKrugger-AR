using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageSpawnManager : MonoBehaviour
{
    public GameObject prefabToSpawn;

    private ARTrackedImageManager trackedImageManager;

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            SpawnObject(trackedImage);
        }
    }

    void SpawnObject(ARTrackedImage trackedImage)
    {
        GameObject obj = Instantiate(prefabToSpawn, trackedImage.transform);

        // 👇 CONTROL POSITION HERE
        obj.transform.localPosition = new Vector3(0.05f, 0.02f, 0);
        obj.transform.localRotation = Quaternion.identity;
    }
}