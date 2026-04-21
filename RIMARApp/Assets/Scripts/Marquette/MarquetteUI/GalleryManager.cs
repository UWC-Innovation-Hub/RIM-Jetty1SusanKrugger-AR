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
        ClearGalleryUI();
        
        galleryPanel.SetActive(true);

        List<Texture2D> screenshots = ScreenshotManager.Instance.GetScreenshots();

        Debug.Log("Screenshots count: " +  screenshots.Count);

        foreach (Texture2D tex in screenshots)
        {
            GameObject item = Instantiate(imageItemPrefab, contentParent);

            RawImage img = item.GetComponentInChildren<RawImage>();

            if (img != null)
            {
                img.texture = tex;
            }
            else
            {
                Debug.Log("No RawImage found in prefab!");
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
    }


    public void CloseGallery()
    {
        ClearGalleryUI();
        
        // Clear screenshots
        ScreenshotManager.Instance.ClearScreenshots();

        galleryPanel.SetActive(false);

        bool didWin = GameManager.Instance.DidPlayerWin();
        WinLosePopupManager.Instance.ShowResultPopup(didWin);
    }


    private void ClearGalleryUI()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}
