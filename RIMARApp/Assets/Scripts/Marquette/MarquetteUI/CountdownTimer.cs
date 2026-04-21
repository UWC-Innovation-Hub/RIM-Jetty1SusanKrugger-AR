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
    private Coroutine flashCoroutine;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        currentTime = countdownTime;
        UpdateTimerDisplay();
        Debug.Log("CountdownTimer initialized.");
    }


    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateTimerDisplay();

            isRunning = false;
            Debug.Log("Timer reached zero.");

            GameManager.Instance.EndGame();
            return;
        }

        UpdateTimerDisplay();

        // Start flashing red when <= 30 seconds
        if (currentTime <= 30f && !isFlashing)
        {
            flashCoroutine = StartCoroutine(FlashRed());
        }
    }


    public void BeginNewRun()
    {
        currentTime = countdownTime;
        isRunning = true;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        isFlashing = false;

        if (timerText != null)
            timerText.color = Color.white;

        UpdateTimerDisplay();

        Debug.Log("Timer started. Current time: " + currentTime);
    }


    public void StopTimer()
    {
        isRunning = false;
    }


    public void ResetTimer()
    {
        isRunning = false;
        currentTime = countdownTime;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        isFlashing = false;

        if (timerText != null)
            timerText.color = Color.white;

        UpdateTimerDisplay();
    }


    // Reduce time on successful screenshot
    public void ReduceTime(float seconds)
    {
        currentTime -= seconds;

        if (currentTime < 0f)
            currentTime = 0f;

        UpdateTimerDisplay();

        Debug.Log("Timer reduced by: " + seconds + " seconds. Current time: " + currentTime);

        // If reduction pushes it into danger zone, start flashing
        if (currentTime <= 30f && !isFlashing)
        {
            flashCoroutine = StartCoroutine(FlashRed());
        }

        if (currentTime <= 0f)
        {
            isRunning = false;
            GameManager.Instance.EndGame();
        }
    }


    void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }


    IEnumerator FlashRed()
    {
        isFlashing = true;

        while (currentTime > 0f && currentTime <= 30f)
        {
            if (timerText != null)            
                timerText.color = Color.red;

            yield return new WaitForSeconds(0.5f);

            if (timerText != null)
                timerText.color = Color.white;

            yield return new WaitForSeconds(0.5f);
        }

        if (timerText != null)
            timerText.color = Color.white;

        isFlashing = false;
        flashCoroutine = null;
    }
}
