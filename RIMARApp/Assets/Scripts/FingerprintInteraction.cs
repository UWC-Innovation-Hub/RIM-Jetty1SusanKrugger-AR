using UnityEngine;
using UnityEngine.Video;

public class FingerprintInteraction : MonoBehaviour
{
    private Vector3 originalScale;
    private VideoPlayer videoPlayer;

    private float holdTime = 0f;
    private float requiredHoldTime = 1.0f;
    private bool isHolding = false;

    private GameObject videoScreen;

    void Start()
    {
        originalScale = transform.localScale;
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Stop();
        }
    }

    void Update()
    {
        if (isHolding)
        {
            holdTime += Time.deltaTime;

            float progress = holdTime / requiredHoldTime;

            // ✨ Scale effect (visual feedback)
            transform.localScale = originalScale * (1f + progress * 0.3f);

            if (holdTime >= requiredHoldTime)
            {
                ShowVideoScreen();
                ResetHold();
            }
        }
    }

    void ShowVideoScreen()
    {
        if (videoPlayer == null) return;

        // Create floating screen if not exists
        if (videoScreen == null)
        {
            videoScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            videoScreen.transform.position = transform.position + Vector3.up * 0.1f;
            videoScreen.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            // Face camera
            videoScreen.transform.LookAt(Camera.main.transform);

            // Assign material
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            videoScreen.GetComponent<Renderer>().material = mat;

            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            videoPlayer.targetMaterialRenderer = videoScreen.GetComponent<Renderer>();
        }

        VideoManager.Instance.PlayVideo(videoPlayer);
    }

    void ResetHold()
    {
        holdTime = 0f;
        isHolding = false;
        transform.localScale = originalScale;
    }

    void OnMouseDown()
    {
        isHolding = true;
    }

    void OnMouseUp()
    {
        ResetHold();
    }
}