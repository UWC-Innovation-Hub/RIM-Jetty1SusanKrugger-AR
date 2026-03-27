using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


/*
 * This script handles the behaviour at the end of the experience when the screenshots are displayed
 */

public class GalleryManager : MonoBehaviour
{
    public static GalleryManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject galleryPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject imageItemPrefab;



    private void Awake()
    {
        Instance = this;
    }


    public void ShowGallery()
    {
        galleryPanel.SetActive(true);

        List<Texture2D> screenshots = ScreenshotManager.Instance.GetScreenshots();

        foreach (Texture2D tex in screenshots)
        {
            GameObject item = Instantiate(imageItemPrefab, contentParent);
            RawImage img = item.GetComponent<RawImage>();
            img.texture = tex;
        }
    }


    public void CloseGallery()
    {
        // Clear UI items
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Clear screenshots
        ScreenshotManager.Instance.GetScreenshots().Clear();

        galleryPanel.SetActive(false);

        // Restart experience
        GameManager.Instance.StartGame();
        ProgressTracker.Instance.ResetProgress();
    }
}
