using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDebugManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private QRCodeAnchor qrCodeAnchor;
    [SerializeField] private QRSceneSpawner qrSceneSpawner;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showOnScreenInfo = true;

    private string debugInfo = "";
    private int trackedImageCount = 0;
    private int spawnedContentCount = 0;

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

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var image in args.added)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[AR Debug] QR Code Detected: {image.referenceImage.name} at {image.transform.position}");
            }
        }

        trackedImageCount = trackedImageManager.trackables.count;
        UpdateDebugInfo();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.touchCount == 3)
        {
            ClearAllContent();
        }

        if (qrSceneSpawner != null)
        {
            spawnedContentCount = qrSceneSpawner.GetTotalSpawnedObjects();
        }
        else if (qrCodeAnchor != null)
        {
            spawnedContentCount = 0;
            foreach (var mapping in qrCodeAnchor.GetType().GetField("spawnedContent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(qrCodeAnchor) as System.Collections.IDictionary ?? new System.Collections.Generic.Dictionary<string, GameObject>())
            {
                if ((mapping as System.Collections.Generic.KeyValuePair<string, GameObject>?).Value.Value != null)
                {
                    spawnedContentCount++;
                }
            }
        }

        UpdateDebugInfo();
    }

    void UpdateDebugInfo()
    {
        debugInfo = $"AR Session: {(arSession != null && arSession.enabled ? "Active" : "Inactive")}\n";
        debugInfo += $"Tracked Images: {trackedImageCount}\n";
        debugInfo += $"Spawned Objects: {spawnedContentCount}\n";
        debugInfo += $"\nTip: 3-finger tap or Space to clear";
    }

    void ClearAllContent()
    {
        if (qrSceneSpawner != null)
        {
            qrSceneSpawner.ClearAllScenes();
            if (showDebugLogs)
            {
                Debug.Log("[AR Debug] All AR scenes cleared");
            }
        }
        else if (qrCodeAnchor != null)
        {
            qrCodeAnchor.ClearAllContent();
            if (showDebugLogs)
            {
                Debug.Log("[AR Debug] All spawned content cleared");
            }
        }
    }

    void OnGUI()
    {
        if (showOnScreenInfo)
        {
            GUI.Box(new Rect(10, 10, 300, 120), "AR Debug Info");
            GUI.Label(new Rect(20, 35, 280, 100), debugInfo);
        }
    }
}
