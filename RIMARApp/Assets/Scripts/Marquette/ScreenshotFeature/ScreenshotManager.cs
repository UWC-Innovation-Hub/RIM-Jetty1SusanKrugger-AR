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

    [Header("Panel Capture")]
    [SerializeField] private Camera panelCaptureCamera;
    [SerializeField] private int captureWidth; // 1024
    [SerializeField] private int captureHeight; // 1024

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


    void TryTakeScreenshot()
    {
        // Only allow screenshot to be taken if info panel is open
        if (!InfoPanelSpawner.Instance.HasActivePanel())
            return;

        if (panelCaptureCamera == null)
        {
            Debug.LogError("Panel Capture Camera is not assigned on ScreenshotManager.");
            return;
        }

        StartCoroutine(CapturePanelScreenshot());
    }


    IEnumerator CapturePanelScreenshot()
    {
        // Wait for frame to finish rendering (clean frame, no flash yet)
        yield return new WaitForEndOfFrame();

        GameObject panel = InfoPanelSpawner.Instance.GetCurrentPanel();

        if (panel == null)
        {
            Debug.LogWarning("No active panel found for capture.");
            yield break;
        }

        PositionCaptureCamera(panel);

        RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 24);
        panelCaptureCamera.targetTexture = rt;

        Texture2D screenshot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        panelCaptureCamera.Render();

        screenshot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        screenshot.Apply();

        panelCaptureCamera.targetTexture = null;
        RenderTexture.active = currentRT;

        rt.Release();
        Destroy(rt);

        screenshots.Add(screenshot);
        Debug.Log("Panel screenshot count: " +  screenshots.Count);

        StartCoroutine(FlashEffect());

        if (shutterAudio != null)
            shutterAudio.Play();

        yield return new WaitForSeconds(flashDuration * 0.5f);

        GameManager.Instance.OnSuccessfulScreenshot();
    }


    void PositionCaptureCamera(GameObject panel)
    {
        Renderer[] renderers = panel.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            // fallback
            panelCaptureCamera.transform.position = panel.transform.position - panel.transform.forward * 0.5f;
            panelCaptureCamera.transform.rotation = Quaternion.LookRotation(panel.transform.forward);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 panelForward = panel.transform.forward;
        Vector3 center = bounds.center;
        float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        float distance = Mathf.Max(0.5f, size * 1.5f);

        panelCaptureCamera.transform.position = center - panelForward * distance;
        panelCaptureCamera.transform.rotation = Quaternion.LookRotation(panelForward, Vector3.up);
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


    bool IsCorrectScreenshot()
    {
        // Must have a panel open
        if (!InfoPanelSpawner.Instance.HasActivePanel())
            return false;

        LocationData activeLocation = InfoPanelSpawner.Instance.GetCurrentLocation();
        LocationData targetLocation = GameManager.Instance.GetCurrentTargetLocation();

        return activeLocation == targetLocation;
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
}