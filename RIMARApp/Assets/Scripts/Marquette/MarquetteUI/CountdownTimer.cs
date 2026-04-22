using UnityEngine;
using TMPro;
using System.Collections;
using Microsoft.Win32.SafeHandles;
using UnityEngine.XR.ARFoundation;


/*
 * This script handles the countdown timer behaviours including:
 * 1. Countdown
 * 2. Red flashing at 30 seconds remaining
 * 3. Shake feedback when time is reduced
 * 4. Quick red flash when time is reduced
 * 5. Floating -20 popup when time is reduced
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

    [Header("Quick Flash Settings")]
    [SerializeField] private float quickFlashDuration = 0.18f;
    [SerializeField] private Color quickFlashColor = Color.red;
    [SerializeField] private Color normalTimerColor = Color.white;

    [Header("Time Penalty Popup")]
    [SerializeField] private TextMeshProUGUI timePenaltyPopupText;
    [SerializeField] private float popupDuration = 0.6f;
    [SerializeField] private float popupRiseDistance = 25f;

    private float currentTime;
    private bool isRunning = false;
    private bool isFlashing = false;

    private Coroutine flashCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine quickFlashCoroutine;
    private Coroutine popupCoroutine;

    private Vector2 originalShakeAnchoredPosition;
    private Vector2 originalPopupAnchoredPosition;


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

        if (timePenaltyPopupText != null)
        {
            RectTransform popupRect = timePenaltyPopupText.rectTransform;
            originalPopupAnchoredPosition = popupRect.anchoredPosition;

            Color c = timePenaltyPopupText.color;
            c.a = 0f;
            timePenaltyPopupText.color = c;
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

        StopAllFeedbackCoroutines();

        isFlashing = false;

        if (timerText != null)
            timerText.color = Color.white;

        if (timerShakeTarget != null ) 
            timerShakeTarget.anchoredPosition = originalShakeAnchoredPosition;

        if (timePenaltyPopupText != null)
        {
            timePenaltyPopupText.rectTransform.anchoredPosition = originalPopupAnchoredPosition;
            Color c = timePenaltyPopupText.color;
            c.a = 0f;
            timePenaltyPopupText.color = c;
        }

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

        StopAllFeedbackCoroutines();

        isFlashing = false;

        if (timerText != null)
            timerText.color = Color.white;

        if (timerShakeTarget != null)
            timerShakeTarget.anchoredPosition= originalShakeAnchoredPosition;

        if (timePenaltyPopupText != null)
        {
            timePenaltyPopupText.rectTransform.anchoredPosition = originalPopupAnchoredPosition;
            Color c = timePenaltyPopupText.color;
            c.a = 0f;
            timePenaltyPopupText.color = c;
        }

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

        if (quickFlashCoroutine != null)
            StopCoroutine(quickFlashCoroutine);

        quickFlashCoroutine = StartCoroutine(QuickFlashTimer());

        if (timePenaltyPopupText != null)
        {
            if (popupCoroutine != null)
                StopCoroutine(popupCoroutine);

            popupCoroutine = StartCoroutine(ShowTimePenaltyPopup("-20"));
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


    private IEnumerator QuickFlashTimer()
    {
        if (timerText == null)
            yield break;

        Color originalColor = timerText.color;

        timerText.color = quickFlashColor;
        yield return new WaitForSeconds(quickFlashDuration);

        if (!isFlashing)
            timerText.color = normalTimerColor;

        quickFlashCoroutine = null; 
    }


    private IEnumerator ShowTimePenaltyPopup(string message)
    {
        if (timePenaltyPopupText == null)
            yield break;

        timePenaltyPopupText.text = message;

        RectTransform popupRect = timePenaltyPopupText.rectTransform;
        popupRect.anchoredPosition = originalPopupAnchoredPosition;

        Color c = timePenaltyPopupText.color;
        c.a = 1f;
        timePenaltyPopupText.color = c;

        float elapsed = 0f;
        Vector2 startPos = originalPopupAnchoredPosition;
        Vector2 endPos = originalPopupAnchoredPosition + new Vector2(0f, popupRiseDistance);

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popupDuration);

            popupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            Color color = timePenaltyPopupText.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            timePenaltyPopupText.color = color;

            yield return null;
        }

        popupRect.anchoredPosition = originalPopupAnchoredPosition;

        Color finalColor = timePenaltyPopupText.color;
        finalColor.a = 0f;
        timePenaltyPopupText.color = finalColor;

        popupCoroutine = null;
    }


    private void StopAllFeedbackCoroutines()
    {
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

        if (quickFlashCoroutine != null)
        {
            StopCoroutine(quickFlashCoroutine);
            quickFlashCoroutine = null;
        }

        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
            popupCoroutine = null;
        }
    }
}
