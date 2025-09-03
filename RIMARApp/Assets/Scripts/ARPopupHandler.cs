using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;


public class ARPopupHandler : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    [Header("Popup UI")]
    public GameObject popupPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public RawImage videoDisplay;
    public Button playButton;
    public VideoPlayer videoPlayer;

    private float lastTapTime;
    private int tapCount;
    private bool expanded = false;


    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }


    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }


    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            ShowPopup("Clue: " + trackedImage.referenceImage.name);
        }

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
                ShowPopup("Clue: " + trackedImage.referenceImage.name);
            else
                HidePopup();
        }

        foreach (var trackedImage in args.removed)
        {
            HidePopup();
        }
    }


    void ShowPopup(string clueTitle)
    {
        popupPanel.SetActive(true);

        // Title only
        titleText.text = clueTitle;
        descriptionText.gameObject.SetActive(false);
        videoDisplay.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);

        expanded = false;
    }


    void HidePopup()
    {
        popupPanel.SetActive(false);
        videoPlayer.Stop();
    }


    void Update()
    {
        if (popupPanel.activeSelf && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            float timeNow = Time.time;

            if (timeNow - lastTapTime < 0.3f)
            {
                tapCount++;
                if (tapCount == 2)
                {
                    // double tap
                    StartCoroutine(CaptureScreenshotAndClose());
                    tapCount = 0;
                    return;
                }
            }
            else
            {
                tapCount = 1;
            }

            lastTapTime = timeNow;

            // single tap: expand if not already expanded
            if (!expanded)
            {
                ExpandPanel();
            }
        }
    }


    void ExpandPanel()
    {
        expanded = true;
        descriptionText.gameObject.SetActive(true);
        videoDisplay.gameObject.SetActive(true);
        playButton.gameObject.SetActive(true);

        descriptionText.text = "This is a detailed clue description.";
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() =>
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
                videoPlayer.Play();
        });
    }


    IEnumerator CaptureScreenshotAndClose()
    {
        yield return new WaitForEndOfFrame();

        string path = System.IO.Path.Combine(Application.persistentDataPath, "AR_ClueScreenshot.png");
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Screenshot saved at: " + path);

        HidePopup();
    }
}
