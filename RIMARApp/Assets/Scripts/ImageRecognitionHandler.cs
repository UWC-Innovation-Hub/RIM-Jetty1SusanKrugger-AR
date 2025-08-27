using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using UnityEngine.Video;
using System.Collections;

public class ImageRecognitionHandler : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    [Header("Popup UI")]
    public GameObject popupPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text instructionsText;
    public VideoPlayer videoPlayer;

    private float lastTapTime;
    private int tapCount;
    private Coroutine instructionsCoroutine;

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
            ShowPopup(trackedImage.referenceImage.name);
        }

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
                ShowPopup(trackedImage.referenceImage.name);
            else
                HidePopup();
        }

        foreach (var trackedImage in args.removed)
        {
            HidePopup();
        }
    }

    void ShowPopup(string imageName)
    {
        popupPanel.SetActive(true);
        titleText.text = "Clue Found: " + imageName;
        descriptionText.text = "Here is more information.";
        instructionsText.gameObject.SetActive(true);
        instructionsText.text = "Tap once to expand, double tap to collect.";

        videoPlayer.Play();

        // Reset & start timer for hiding instructions
        if (instructionsCoroutine != null)
            StopCoroutine(instructionsCoroutine);
        instructionsCoroutine = StartCoroutine(HideInstructionsAfterDelay(5f));
    }

    void HidePopup()
    {
        popupPanel.SetActive(false);
        videoPlayer.Stop();

        if (instructionsCoroutine != null)
            StopCoroutine(instructionsCoroutine);
    }

    IEnumerator HideInstructionsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        instructionsText.gameObject.SetActive(false);
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
                    CollectClue();
                    tapCount = 0;
                }
            }
            else
            {
                tapCount = 1;
            }

            lastTapTime = timeNow;
        }
    }

    void CollectClue()
    {
        Debug.Log("Clue collected! Move to next.");
        popupPanel.SetActive(false);
        videoPlayer.Stop();
    }
}
