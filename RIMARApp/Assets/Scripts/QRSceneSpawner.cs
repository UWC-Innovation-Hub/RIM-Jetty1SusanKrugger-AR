using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class QRSceneSpawner : MonoBehaviour
{
    [Header("AR Tracking")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Scene Configuration")]
    [SerializeField] private List<QRSceneMapping> sceneMappings = new List<QRSceneMapping>();

    [Header("World-Locking Settings")]
    [SerializeField] private bool worldLocked = true;
    [SerializeField] private bool spawnOnlyOnce = true;
    [SerializeField] private float spawnDelay = 0.2f;

    private Dictionary<string, List<GameObject>> spawnedScenes = new Dictionary<string, List<GameObject>>();
    private HashSet<string> processedQRCodes = new HashSet<string>();

    void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            HandleImageAdded(trackedImage);
        }
    }

    void HandleImageAdded(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnOnlyOnce && processedQRCodes.Contains(imageName))
        {
            return;
        }

        QRSceneMapping sceneMapping = sceneMappings.Find(m => m.qrCodeName == imageName);
        if (sceneMapping != null && sceneMapping.sceneObjects.Count > 0)
        {
            Vector3 anchorPosition = trackedImage.transform.position;
            Quaternion anchorRotation = trackedImage.transform.rotation;

            StartCoroutine(SpawnSceneSequentially(imageName, anchorPosition, anchorRotation, sceneMapping));
            processedQRCodes.Add(imageName);

            Debug.Log($"AR Scene spawning for QR code: {imageName} with {sceneMapping.sceneObjects.Count} objects (World-Locked: {worldLocked})");
        }
    }

    System.Collections.IEnumerator SpawnSceneSequentially(string qrCodeName, Vector3 anchorPosition, Quaternion anchorRotation, QRSceneMapping sceneMapping)
    {
        List<GameObject> spawnedObjects = new List<GameObject>();

        foreach (var sceneObject in sceneMapping.sceneObjects)
        {
            if (sceneObject.prefab != null)
            {
                Vector3 worldOffset = anchorRotation * sceneObject.relativePosition;
                Vector3 spawnPosition = anchorPosition + worldOffset;

                Quaternion spawnRotation = anchorRotation * Quaternion.Euler(sceneObject.relativeRotation);

                GameObject spawnedObj = Instantiate(sceneObject.prefab, spawnPosition, spawnRotation);
                spawnedObj.transform.localScale = sceneObject.scale;
                spawnedObj.name = $"{sceneObject.prefab.name}_{sceneObject.objectName}";

                if (worldLocked)
                {
                    spawnedObj.transform.SetParent(null);
                    
                    WorldAnchor anchor = spawnedObj.AddComponent<WorldAnchor>();
                    anchor.Initialize(spawnPosition, spawnRotation);
                }

                spawnedObjects.Add(spawnedObj);

                Debug.Log($"Spawned {sceneObject.objectName} at world position {spawnPosition} (distance: {sceneObject.relativePosition.magnitude:F2}m from anchor)");
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        spawnedScenes[qrCodeName] = spawnedObjects;
        
        Debug.Log($"Scene '{sceneMapping.sceneName}' fully spawned with {spawnedObjects.Count} world-locked objects");
    }

    public void ClearScene(string qrCodeName)
    {
        if (spawnedScenes.TryGetValue(qrCodeName, out List<GameObject> objects))
        {
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            spawnedScenes.Remove(qrCodeName);
            processedQRCodes.Remove(qrCodeName);
        }
    }

    public void ClearAllScenes()
    {
        foreach (var scene in spawnedScenes.Values)
        {
            foreach (var obj in scene)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }

        spawnedScenes.Clear();
        processedQRCodes.Clear();
        Debug.Log("All AR scenes cleared");
    }

    public int GetTotalSpawnedObjects()
    {
        int count = 0;
        foreach (var scene in spawnedScenes.Values)
        {
            count += scene.Count;
        }
        return count;
    }
}

[System.Serializable]
public class QRSceneMapping
{
    public string qrCodeName;
    public string sceneName;
    public List<SceneObject> sceneObjects = new List<SceneObject>();
}

[System.Serializable]
public class SceneObject
{
    [Header("Identification")]
    public string objectName;
    
    [Header("Prefab")]
    public GameObject prefab;
    
    [Header("Position (meters from QR anchor)")]
    public Vector3 relativePosition;
    
    [Header("Rotation (degrees)")]
    public Vector3 relativeRotation = Vector3.zero;
    
    [Header("Scale")]
    public Vector3 scale = Vector3.one;
}
