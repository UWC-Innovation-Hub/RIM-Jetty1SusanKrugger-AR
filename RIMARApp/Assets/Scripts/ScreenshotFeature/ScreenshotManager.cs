using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * This script manages the behavior and implementation of the screenshotting feature that the users would trigger 
 * once they double tap on an empty space on the screen.
 */

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager Instance;

    public GameObject screenshotPreviewPrefab;

    private List<Texture2D> screenshots = new List<Texture2D>();

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
        yield return new WaitForEndOfFrame();

        Texture2D screen = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screen.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screen.Apply();

        screenshots.Add(screen);

        ShowPreview(screen);
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
