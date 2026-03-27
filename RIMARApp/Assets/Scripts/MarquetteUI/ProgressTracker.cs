using UnityEngine;
using UnityEngine.UI;


/*
 * This script handles the user's progress on the amount intel they have collected
 */

public class ProgressTracker : MonoBehaviour
{
    public static ProgressTracker Instance;

    public Slider progressBar;
    [SerializeField] private int totalCheckpoints; // 12

    private int completedCheckpoints = 0;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        progressBar.minValue = 0;
        progressBar.maxValue = totalCheckpoints;
        progressBar.value = 0;
    }


    public void AddProgress()
    {
        completedCheckpoints++;
        progressBar.value = completedCheckpoints;
    }


    public void ResetProgress()
    {
        completedCheckpoints = 0;
        progressBar.value = 0;
    }


    public int GetProgress()
    {
        return completedCheckpoints;
    }
}
