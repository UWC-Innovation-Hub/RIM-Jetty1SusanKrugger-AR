using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance;

    private VideoPlayer currentPlayer;

    void Awake()
    {
        Instance = this;
    }

    public void PlayVideo(VideoPlayer newPlayer)
    {
        // Stop previous video
        if (currentPlayer != null && currentPlayer != newPlayer)
        {
            currentPlayer.Stop();
        }

        currentPlayer = newPlayer;
        currentPlayer.Play();
    }
}
