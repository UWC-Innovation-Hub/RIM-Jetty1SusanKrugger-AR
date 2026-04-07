using UnityEngine;
using TMPro;
using System.Collections;


/*
 * This script handles the countdown timer behaviours
 */

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;
    
    [Header("Timer Settings")]
    public int countdownTime = 300; // 5 minutes = 300 seconds
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isRunning = false;
    private bool isFlashing = false;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        currentTime = countdownTime;
        UpdateTimerDisplay();
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

        // Start flashing red when <= 30 seconds
        if (currentTime <= 30f && !isFlashing)
        {
            StartCoroutine(FlashRed());
        }
    }


    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }


    // Reduce time on successful screenshot
    public void ReduceTime(float seconds)
    {
        currentTime -= seconds;

        if (currentTime < 0)
            currentTime = 0;

        UpdateTimerDisplay();

        // If reduction pushes it into danger zone, start flashing
        if (currentTime <= 30f && !isFlashing)
        {
            StartCoroutine(FlashRed());
        }
    }


    IEnumerator FlashRed()
    {
        isFlashing = true;

        while (currentTime > 0 && currentTime <= 30f)
        {
            timerText.color = Color.red;
            yield return new WaitForSeconds(0.5f);

            timerText.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }

        timerText.color = Color.white;
        isFlashing = false;
    }
}
