using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class QRImageSpawner : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    [System.Serializable]
    public class QRPrefabPair
    {
        public string imageName;
        public GameObject prefab;
    }

    public List<QRPrefabPair> qrPrefabs;

    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            SpawnObject(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateObject(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            RemoveObject(trackedImage);
        }
    }

    void SpawnObject(ARTrackedImage trackedImage)
    {
        foreach (var pair in qrPrefabs)
        {
            if (trackedImage.referenceImage.name == pair.imageName)
            {
                GameObject obj = Instantiate(pair.prefab, trackedImage.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                spawnedObjects[trackedImage.referenceImage.name] = obj;
            }
        }
    }

    void UpdateObject(ARTrackedImage trackedImage)
    {
        if (spawnedObjects.TryGetValue(trackedImage.referenceImage.name, out GameObject obj))
        {
            obj.SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }
    }

    void RemoveObject(ARTrackedImage trackedImage)
    {
        if (spawnedObjects.TryGetValue(trackedImage.referenceImage.name, out GameObject obj))
        {
            Destroy(obj);
            spawnedObjects.Remove(trackedImage.referenceImage.name);
        }
    }
}
