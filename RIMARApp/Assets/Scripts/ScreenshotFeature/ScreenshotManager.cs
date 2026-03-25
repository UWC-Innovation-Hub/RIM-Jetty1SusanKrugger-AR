using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 * This script manages the behavior and implementation of the screenshotting feature that the users would trigger 
 * once they double tap on an empty space on the screen.
 */

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager Instance;

    [Header ("Preview")]
    public GameObject screenshotPreviewPrefab;

    [Header("Flash Effect")]
    // Visual element to confirm screenshot has been taken
    public Image flashImage;
    [SerializeField] private float flashDuration; // 0.15f

    [Header("Audio")]
    // Audio element to confirm screenshot has been taken
    public AudioSource shutterAudio;

    private List<Texture2D> screenshots = new List<Texture2D>();

    [Header ("Double Tap Durations")]
    [SerializeField] private float lastTimeTap; // 0f
    [SerializeField] private float doubleTapThreshold; // 0.3f


    private void Awake()
    {
        Instance = this;
    }


    private void Update()
    {
        DetectDoubleTap();
    }


    void DetectDoubleTap()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                // Check if tap is on empty space
                if (IsTouchOnUI(touch.position)) return;
                if (IsTouchOnMarker(touch.position)) return;

                float timeSinceLastTap = Time.time - lastTimeTap;

                if (timeSinceLastTap <= doubleTapThreshold)
                {
                    // Double tap detected
                    TryTakeScreenshot();
                }

                lastTimeTap = Time.time;
            }
        }
    }


    bool IsTouchOnUI(Vector2 screenPos)
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }


    bool IsTouchOnMarker(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.GetComponent<ARLocationMarker>() != null)
                return true;
        }

        return false;
    }


    void TryTakeScreenshot()
    {
        // Only allow screenshot to be taken if info panel is open
        if (!InfoPanelSpawner.Instance.HasActivePanel())
            return;

        StartCoroutine(CaptureScreenshot());
    }


    IEnumerator CaptureScreenshot()
    {
        // Show and play the flash + sound first
        StartCoroutine(FlashEffect());

        if (shutterAudio != null)
            shutterAudio.Play();
        
        yield return new WaitForEndOfFrame();

        Texture2D screen = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screen.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screen.Apply();

        screenshots.Add(screen);

        ShowPreview(screen);
    }


    IEnumerator FlashEffect()
    {
        if (flashImage == null) yield break;

        // Fade IN (flash)
        float t = 0;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / flashDuration);
            flashImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // Fade OUT (flash)
        t = 0;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / flashDuration);
            flashImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        flashImage.color = new Color(1, 1, 1, 0);
    }


    void ShowPreview(Texture2D texture)
    {
        GameObject panel = Instantiate(screenshotPreviewPrefab);

        ScreenshotPreview preview = panel.GetComponent<ScreenshotPreview>();
        preview.SetImage(texture);
    }


    public List<Texture2D> GetScreenshots()
    {
        return screenshots;
    }
}
