using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;


/*
 * This script controls how the panel spawned is going to display depending on the ContentType chosen.
 */

public class InfoPanelUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public Image imageDisplay;
    public AudioSource audioSource;
    public VideoPlayer videoPlayer;
    public Transform modelAnchor;


    public void Setup(LocationData data)
    {
        titleText.text = data.locationName;
        descriptionText.text = data.description;

        if (data.image != null && imageDisplay != null)
            imageDisplay.sprite = data.image;

        if (data.audio != null && audioSource != null)
        {
            audioSource.clip = data.audio;
            audioSource.Play();
        }

        if (data.video != null && videoPlayer != null)
        {
            videoPlayer.clip = data.video;
            videoPlayer.Play();
        }

        if (data.model3D != null && modelAnchor != null)
        {
            Instantiate(data.model3D, modelAnchor);
        }
    }


    public void ClosePanel()
    {
        InfoPanelSpawner.Instance.CloseCurrentPanel();
    }
}
