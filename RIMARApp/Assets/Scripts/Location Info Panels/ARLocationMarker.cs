using UnityEngine;


/*
 * This script is attached to the location marker prefab.
 */

public class ARLocationMarker : MonoBehaviour
{
    private LocationData locationData;


    public void Initialize(LocationData data)
    {
        locationData = data;
    }


    private void OnMouseDown()
    {
        InfoPanelSpawner.Instance.SpawnPanel(locationData, transform.position);
    }
}