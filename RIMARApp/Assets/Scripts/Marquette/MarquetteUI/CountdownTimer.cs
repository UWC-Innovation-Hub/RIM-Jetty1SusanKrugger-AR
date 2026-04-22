using UnityEngine;
using TMPro;
using System.Collections;
using Microsoft.Win32.SafeHandles;


/*
 * This script handles the countdown timer behaviours including:
 * 1. Countdown
 * 2. Red flashing at 30 seconds remaining
 * 3. Shake feedback when time is reduced
 */

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance;
    
    [Header("Timer Settings")]
    public int countdownTime = 300; // 5 minutes = 300 seconds
    public TextMeshProUGUI timerText;

    [Header("Shake Settings")]
    [SerializeField] private RectTransform timerShakeTarget;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 12f;
    [SerializeField] private int shakeVibrato = 12;

    private float currentTime;
    private bool isRunning = false;
    private bool isFlashing = false;
    private Coroutine flashCoroutine;
    private Coroutine shakeCoroutine;

    private Vector2 originalShakeAnchoredPosition;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        currentTime = countdownTime;
        UpdateTimerDisplay();

        if (timerShakeTarget != null )
        {
            originalShakeAnchoredPosition = timerShakeTarget.anchoredPosition;
        }

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

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        isFlashing = false;

        if (timerText != null)
            timerText.color = Color.white;

        if (timerShakeTarget != null ) 
            timerShakeTarget.anchoredPosition = originalShakeAnchoredPosition;

        UpdateTimerDisplay();

        Debug.Log("Timer started. Current time: " + currentTime);
    }


    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("Timer stopeed.");
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

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        isFlashing = false;

        if (timerText != null)
            timerText.color = Color.white;

        if (timerShakeTarget != null)
            timerShakeTarget.anchoredPosition= originalShakeAnchoredPosition;

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

        // Trigger shake every time time is reduced
        if (timerShakeTarget != null)
        {
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            shakeCoroutine = StartCoroutine(ShakeTimer());
        }

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


    private IEnumerator ShakeTimer()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float damper = 1f - Mathf.Clamp01(elapsed / shakeDuration);
            float x = Random.Range(-1f, 1f) * shakeMagnitude * damper;
            float y = Random.Range(-0.25f, 0.25f) * shakeMagnitude * 0.25f * damper;

            if (timerShakeTarget != null)
                timerShakeTarget.anchoredPosition = originalShakeAnchoredPosition + new Vector2(x, y);

            yield return null;
        }

        if (timerShakeTarget != null)
            timerShakeTarget.anchoredPosition = originalShakeAnchoredPosition;

        shakeCoroutine = null;
    }
}
