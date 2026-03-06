using UnityEngine;
using TMPro;

public class InfoPanelController : MonoBehaviour
{
    public static InfoPanelController Instance;

    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowPanel(string title, string description)
    {
        titleText.text = title;
        descriptionText.text = description;

        panel.SetActive(true);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }
}