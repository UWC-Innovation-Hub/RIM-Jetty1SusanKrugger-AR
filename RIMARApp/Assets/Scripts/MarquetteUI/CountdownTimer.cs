using UnityEngine;
using TMPro;


/*
 * This script handles the countdown timer behaviours
 */

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public int countdownTime = 300; // 5 minutes = 300 seconds
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isRunning = false;


    private void Start()
    {
        currentTime = countdownTime;
    }


    public void StartTimer()
    {
        isRunning = true;
    }


    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;

            GameManager.Instance.EndGame();
        }

        UpdateTimerDisplay();
    }


    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
