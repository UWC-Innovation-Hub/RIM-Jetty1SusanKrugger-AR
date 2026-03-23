using UnityEngine;
using UnityEngine.Video;

public class FingerprintInteraction : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private GameObject videoScreen;

    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f; // Max time between taps for double tap

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Stop();
        }
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                // Check if this touch hit this object
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform == transform)
                    {
                        float currentTime = Time.time;

                        if (currentTime - lastTapTime <= doubleTapThreshold)
                        {
                            // Double tap detected
                            ShowVideoScreen();
                        }

                        lastTapTime = currentTime;
                    }
                }
            }
        }
    }

    void ShowVideoScreen()
    {
        if (videoPlayer == null) return;

        if (videoScreen == null)
        {
            videoScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            videoScreen.transform.position = transform.position + Vector3.up * 0.1f;
            videoScreen.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            videoScreen.transform.LookAt(Camera.main.transform);

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            videoScreen.GetComponent<Renderer>().material = mat;

            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            videoPlayer.targetMaterialRenderer = videoScreen.GetComponent<Renderer>();
        }

        VideoManager.Instance.PlayVideo(videoPlayer);
    }
}