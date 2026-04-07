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

    private GameObject currentPanel;

    private LocationData currentLocationData;


    private void Awake()
    {
        Instance = this;
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
            Destroy(currentPanel);
            currentPanel = null;
            currentLocationData = null; // important
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
}
