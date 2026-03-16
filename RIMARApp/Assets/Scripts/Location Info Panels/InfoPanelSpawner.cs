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


    private void Awake()
    {
        Instance = this;
    }


    public void SpawnPanel(LocationData data, Vector3 markerPos)
    {
        GameObject prefab = null;

        switch (data.contentType)
        {
            case ContentType.TextOnly:
                prefab = textPanelPrefab;
                break;
            case ContentType.Image:
                prefab = imagePanelPrefab;
                break;
            case ContentType.Audio:
                prefab = audioPanelPrefab;
                break;
            case ContentType.Video:
                prefab = videoPanelPrefab;
                break;
            case ContentType.Model3D:
                prefab = modelPanelPrefab;
                break;
        }

        Vector3 spawnPos = markerPos + Vector3.up * 0.2f;

        GameObject panel = Instantiate(prefab, spawnPos, Quaternion.identity);

        panel.GetComponent<InfoPanelUI>().Setup(data);
    }
}
