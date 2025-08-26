using UnityEngine;
using UnityEngine.UI;


public class ProgressTracker : MonoBehaviour
{
    [Tooltip("Reference to the progress bar slider.")]
    public Slider progressBar;

    [Tooltip("Total number of checkpoints to complete.")]
    public int totalCheckpoints = 5;

    private int completedCheckpoints = 0;


    void Start()
    {
        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = totalCheckpoints;
            progressBar.value = 0;
        }
    }


    public void AddProgress()
    {
        completedCheckpoints++;
        if (progressBar != null)
            progressBar.value = completedCheckpoints;
    }


    public void ResetProgress()
    {
        completedCheckpoints = 0;
        if (progressBar != null)
            progressBar.value = 0;
    }
}
