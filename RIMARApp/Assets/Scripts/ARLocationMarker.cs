using UnityEngine;

public class ARLocationMarker : MonoBehaviour
{
    public string locationTitle;
    [TextArea]
    public string locationDescription;

    void OnMouseDown()
    {
        InfoPanelController.Instance.ShowPanel(locationTitle, locationDescription);
    }
}