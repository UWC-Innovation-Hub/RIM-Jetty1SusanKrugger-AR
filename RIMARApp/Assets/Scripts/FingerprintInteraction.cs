using UnityEngine;
using UnityEngine.Video;

public class FingerprintInteraction : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private GameObject videoScreen;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Stop();
        }
    }

    // 👇 This gets called from the touch manager
    public void PlayVideo()
    {
        ShowVideoScreen();
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