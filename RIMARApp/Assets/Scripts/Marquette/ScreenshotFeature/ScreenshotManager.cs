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
                if (!IsCorrectScreenshot()) return;

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
        // Wait for frame to finish rendering (clean frame, no flash yet)
        yield return new WaitForEndOfFrame();

        // Capture screeshot FIRST
        Texture2D screen = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screen.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screen.Apply();

        screenshots.Add(screen);

        // NOW play feedback effects
        StartCoroutine(FlashEffect());

        if (shutterAudio != null)
            shutterAudio.Play();

        // Wait slightly so flash effect finishes and feels natural
        yield return new WaitForSeconds(flashDuration * 0.5f);

        // Notify game system that a screenshot has been successfully taken
        GameManager.Instance.OnSuccessfulScreenshot();
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


    public List<Texture2D> GetScreenshots()
    {
        return new List<Texture2D>(screenshots); // Return copy (safe)
    }


    public void ClearScreenshots()
    {
        foreach (Texture2D tex in screenshots)
        {
            Destroy(tex); // Prevent memory leaks
        }

        screenshots.Clear();
    }


    bool IsCorrectScreenshot()
    {
        // Must have a panel open
        if (!InfoPanelSpawner.Instance.HasActivePanel())
            return false;

        LocationData activeLocation = InfoPanelSpawner.Instance.GetCurrentLocation();
        LocationData targetLocation = GameManager.Instance.GetCurrentTargetLocation();

        return activeLocation == targetLocation;
    }


    /*
    bool IsCorrectLocation(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            ARLocationMarker marker = hit.collider.GetComponent<ARLocationMarker>();

            if (marker != null)
            {
                int correctID = GameManager.Instance.GetCurrentLocationID();

                return marker.locationID == correctID;
            }
        }

        return false;
    }
    */
}
