using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Count-up timer that displays elapsed time using TextMeshPro
/// Automatically starts when the scene loads
/// </summary>
public class ElapsedTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("TextMeshPro component to display the timer")]
    public TextMeshProUGUI timerText;
    
    [Tooltip("Auto-start timer when scene loads")]
    public bool autoStart = true;
    
    [Tooltip("Timer format options")]
    public TimerFormat displayFormat = TimerFormat.MinutesSeconds;
    
    [Header("Visual Settings")]
    [Tooltip("Color when timer is running")]
    public Color runningColor = Color.white;
    
    [Tooltip("Color when timer is paused")]
    public Color pausedColor = Color.yellow;
    
    [Tooltip("Update frequency (times per second)")]
    [Range(1, 60)]
    public int updateFrequency = 10;

    // Timer state
    private float elapsedTime = 0f;
    private bool isRunning = false;
    private bool isPaused = false;
    private float lastUpdateTime;
    
    // Update interval based on frequency
    private float updateInterval;

    public enum TimerFormat
    {
        Seconds,           // 123.45
        MinutesSeconds,    // 2:03.45
        HoursMinutesSeconds, // 1:02:03
        HoursMinutesSecondsMillis // 1:02:03.456
    }

    // Events for external systems
    public System.Action<float> OnTimerUpdate;
    public System.Action OnTimerStarted;
    public System.Action OnTimerPaused;
    public System.Action OnTimerResumed;
    public System.Action OnTimerReset;

    private void Start()
    {
        InitializeTimer();
        
        if (autoStart)
        {
            StartTimer();
        }
    }

    private void InitializeTimer()
    {
        // Get TextMeshPro component if not assigned
        if (timerText == null)
        {
            timerText = GetComponent<TextMeshProUGUI>();
            if (timerText == null)
            {
                timerText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (timerText == null)
        {
            Debug.LogError($"No TextMeshProUGUI component found on {gameObject.name}! Please assign one.");
            enabled = false;
            return;
        }

        // Calculate update interval
        updateInterval = 1f / updateFrequency;
        lastUpdateTime = Time.time;

        // Initialize display
        UpdateTimerDisplay();
        UpdateTimerColor();
    }

    private void Update()
    {
        if (!isRunning || isPaused)
            return;

        // Update elapsed time
        elapsedTime += Time.deltaTime;

        // Update display at specified frequency
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateTimerDisplay();
            OnTimerUpdate?.Invoke(elapsedTime);
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// Start or resume the timer
    /// </summary>
    public void StartTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            isPaused = false;
            OnTimerStarted?.Invoke();
            Debug.Log("Timer started");
        }
        else if (isPaused)
        {
            ResumeTimer();
        }
        
        UpdateTimerColor();
    }

    /// <summary>
    /// Pause the timer (can be resumed)
    /// </summary>
    public void PauseTimer()
    {
        if (isRunning && !isPaused)
        {
            isPaused = true;
            OnTimerPaused?.Invoke();
            UpdateTimerColor();
            Debug.Log($"Timer paused at {FormatTime(elapsedTime)}");
        }
    }

    /// <summary>
    /// Resume the timer from pause
    /// </summary>
    public void ResumeTimer()
    {
        if (isRunning && isPaused)
        {
            isPaused = false;
            OnTimerResumed?.Invoke();
            UpdateTimerColor();
            Debug.Log("Timer resumed");
        }
    }

    /// <summary>
    /// Stop and reset the timer
    /// </summary>
    public void ResetTimer()
    {
        isRunning = false;
        isPaused = false;
        elapsedTime = 0f;
        UpdateTimerDisplay();
        UpdateTimerColor();
        OnTimerReset?.Invoke();
        Debug.Log("Timer reset");
    }

    /// <summary>
    /// Stop the timer without resetting
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        isPaused = false;
        UpdateTimerColor();
        Debug.Log($"Timer stopped at {FormatTime(elapsedTime)}");
    }

    /// <summary>
    /// Set the timer to a specific value
    /// </summary>
    /// <param name="timeInSeconds">Time to set in seconds</param>
    public void SetTime(float timeInSeconds)
    {
        elapsedTime = Mathf.Max(0f, timeInSeconds);
        UpdateTimerDisplay();
        Debug.Log($"Timer set to {FormatTime(elapsedTime)}");
    }

    /// <summary>
    /// Add time to the current elapsed time
    /// </summary>
    /// <param name="timeToAdd">Time to add in seconds</param>
    public void AddTime(float timeToAdd)
    {
        elapsedTime += timeToAdd;
        elapsedTime = Mathf.Max(0f, elapsedTime);
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(elapsedTime);
        }
    }

    private void UpdateTimerColor()
    {
        if (timerText != null)
        {
            timerText.color = isPaused ? pausedColor : runningColor;
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        switch (displayFormat)
        {
            case TimerFormat.Seconds:
                return timeInSeconds.ToString("F2");

            case TimerFormat.MinutesSeconds:
                int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
                float seconds = timeInSeconds % 60f;
                return $"{minutes}:{seconds:00.00}";

            case TimerFormat.HoursMinutesSeconds:
                int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
                int mins = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
                int secs = Mathf.FloorToInt(timeInSeconds % 60f);
                return $"{hours:00}:{mins:00}:{secs:00}";

            case TimerFormat.HoursMinutesSecondsMillis:
                int h = Mathf.FloorToInt(timeInSeconds / 3600f);
                int m = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
                float s = timeInSeconds % 60f;
                return $"{h:00}:{m:00}:{s:00.000}";

            default:
                return timeInSeconds.ToString("F2");
        }
    }

    // Public getters for external access
    public float ElapsedTime => elapsedTime;
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    public string FormattedTime => FormatTime(elapsedTime);

    // Utility methods for common operations
    public void TogglePause()
    {
        if (isPaused)
            ResumeTimer();
        else
            PauseTimer();
    }

    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    private void OnDestroy()
    {
        // Clean up events
        OnTimerUpdate = null;
        OnTimerStarted = null;
        OnTimerPaused = null;
        OnTimerResumed = null;
        OnTimerReset = null;
    }

    // Debug information in inspector
    private void OnValidate()
    {
        if (updateFrequency < 1)
            updateFrequency = 1;
    }
}