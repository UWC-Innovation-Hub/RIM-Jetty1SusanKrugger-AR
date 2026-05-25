using UnityEngine;
using UnityEngine.UI;


/*
 * This script handles the user's progress on the amount intel they have collected
 */

public class ProgressTracker : MonoBehaviour
{
    public static ProgressTracker Instance;

    [SerializeField] private Slider progressBar;

    private int totalCheckpoints = 0;
    private int completedCheckpoints = 0;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        ResetProgress();
    }


    public void SetTotalCheckpoints(int total)
    {
        totalCheckpoints = Mathf.Max(1, total);
        completedCheckpoints = 0;

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


    public int GetProgress()
    {
        return completedCheckpoints;
    }


    public int GetTotalCheckpoints()
    {
        return totalCheckpoints;
    }
}
