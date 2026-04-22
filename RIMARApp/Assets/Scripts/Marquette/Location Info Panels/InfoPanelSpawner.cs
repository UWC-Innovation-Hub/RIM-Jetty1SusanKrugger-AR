using UnityEngine;


/*
 * This script chooses which info panel prefab to spawn depending on the location data of that location.
 */

public class InfoPanelSpawner : MonoBehaviour
{
    public static InfoPanelSpawner Instance;

    public GameObject textPanelPrefab;
    public GameObject imagePanelPrefab;
    public GameObject audioPanelPrefab;
    public GameObject videoPanelPrefab;
    public GameObject modelPanelPrefab;

    [Header("Panel Capture")]
    [SerializeField] private string panelCaptureLayerName; // "PanelCapture"

    private GameObject currentPanel;
    private LocationData currentLocationData;
    private int panelCaptureLayer = -1;


    private void Awake()
    {
        Instance = this;
        panelCaptureLayer = LayerMask.NameToLayer(panelCaptureLayerName);

        if (panelCaptureLayer == -1)
        {
            Debug.LogError($"Layer '{panelCaptureLayerName}' does not exist. Please create it in Unity.");
        }
    }


    public void SpawnPanel(LocationData data, Vector3 markerPos)
    {
        // Destroy existing panel ONLY when tapping another marker
        if (currentPanel != null)
        {
            Destroy(currentPanel);
        }

        GameObject prefab = GetPrefab(data.contentType);
        Vector3 spawnPos = markerPos + Vector3.up * 0.2f;

        currentPanel = Instantiate(prefab, spawnPos, Quaternion.identity);
        currentPanel.GetComponent<InfoPanelUI>().Setup(data);

        // Store current location
        currentLocationData = data;

        if (panelCaptureLayer != -1)
        {
            SetLayerRecursively(currentPanel, panelCaptureLayer);
        }
    }


    GameObject GetPrefab(ContentType type)
    {
        switch (type)
        {
            case ContentType.Image: return imagePanelPrefab;
            case ContentType.Audio: return audioPanelPrefab;
            case ContentType.Video: return videoPanelPrefab;
            case ContentType.Model3D: return modelPanelPrefab;
            default: return textPanelPrefab;
        }
    }


    public void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            InfoPanelCloseAnimator animator = currentPanel.GetComponent<InfoPanelCloseAnimator>();

            if (animator != null)
            {
                animator.PlayClose(() =>
                {
                    currentPanel = null;
                });
            }
            else
            {
                Destroy(currentPanel);
                currentPanel = null;
            }
            
            //currentLocationData = null; // important
        }
    }


    // Checks if there is an info panel open for screenshot to take place
    public bool HasActivePanel()
    {
        return currentPanel != null;
    }


    public LocationData GetCurrentLocation()
    {
        return currentLocationData;
    }


    public GameObject GetCurrentPanel()
    {
        return currentPanel;
    }


    private void SetLayerRecursively(GameObject obj, int layer)
    {
        // Skip excluded objects and their children
        if (obj.GetComponent<ExcludeFromPanelCapture>() != null)
            return;
        
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
