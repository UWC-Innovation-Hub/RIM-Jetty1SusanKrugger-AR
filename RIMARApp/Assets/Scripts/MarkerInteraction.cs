using UnityEngine;
using TMPro;

public class MarkerInteraction : MonoBehaviour
{
    public GameObject infoPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    private string locationTitle;
    private string locationDescription;

    public void SetData(string title, string description)
    {
        locationTitle = title;
        locationDescription = description;
    }

    void OnMouseDown()
    {
        infoPanel.SetActive(true);
        titleText.text = locationTitle;
        descriptionText.text = locationDescription;
    }
}