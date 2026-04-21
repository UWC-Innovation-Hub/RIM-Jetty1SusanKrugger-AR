using System.Collections;
using UnityEngine;
using TMPro;


/*
 * This script handles the win/lose popup shown after the gallery closes.
 * It fades in, shows the result, counts down, fades out,
 * and then resets the whole experience back to the start screen.
 */

public class WinLosePopupManager : MonoBehaviour
{
    public static WinLosePopupManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject winLosePopupGroup;
    [SerializeField] private CanvasGroup winLoseCanvasGroup;
    [SerializeField] private TextMeshProUGUI resultMessageText;
    [SerializeField] private TextMeshProUGUI resetCountdownText;

    [Header("Countdown Settings")]
    [SerializeField] private int resetCountdownSeconds = 3;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Scene References")]
    [SerializeField] private GridSpawner gridSpawner;

    private Coroutine popupRoutine;


    private void Awake()
    {
        Instance = this;
    }


    public void ShowResultPopup(bool didWin)
    {
        if (popupRoutine != null) 
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(ShowPopupRoutine(didWin));
    }


    private IEnumerator ShowPopupRoutine(bool didWin)
    {
        if (winLosePopupGroup != null)
            winLosePopupGroup.SetActive(true);

        if (winLoseCanvasGroup != null)
        {
            winLoseCanvasGroup.alpha = 0f;
            winLoseCanvasGroup.interactable = false;
            winLoseCanvasGroup.blocksRaycasts = false;
        }

        if (resultMessageText  != null)
        {
            if (didWin)
                resultMessageText.text = "Success! You gathered enough intel.";
            else
                resultMessageText.text = "Mission failed. You did not gather enough intel.";
        }

        yield return StartCoroutine(FadeCanvasGroup(winLoseCanvasGroup, 0f, 1f, fadeInDuration));

        if (winLoseCanvasGroup != null)
        {
            winLoseCanvasGroup.interactable = true;
            winLoseCanvasGroup.blocksRaycasts = true;
        }

        int remaining = resetCountdownSeconds;

        while (remaining > 0)
        {
            if (resetCountdownText != null)
                resetCountdownText.text = $"Experience resets in... {remaining}";

            yield return new WaitForSeconds(1f);
            remaining--;
        }

        if (resetCountdownText != null)
            resetCountdownText.text = "Experience resets in... 0";

        if (winLoseCanvasGroup != null)
        {
            winLoseCanvasGroup.interactable = false;
            winLoseCanvasGroup.blocksRaycasts = false;
        }

        yield return StartCoroutine(FadeCanvasGroup(winLoseCanvasGroup, 1f, 0f, fadeOutDuration));

        ResetExperience();
    }


    private void ResetExperience()
    {
        if (winLosePopupGroup != null)
            winLosePopupGroup.SetActive(false);

        // Stop any open panel
        if (InfoPanelSpawner.Instance != null)
            InfoPanelSpawner.Instance.CloseCurrentPanel();

        // Clear screenshots just in case
        if (ScreenshotManager.Instance != null)
            ScreenshotManager.Instance.ClearScreenshots();

        // Reset progress
        if (ProgressTracker.Instance != null)
            ProgressTracker.Instance.ResetProgress();

        // Reset timer
        if (CountdownTimer.Instance != null)
            CountdownTimer.Instance.ResetTimer();

        // Reset grid
        if (gridSpawner != null)
            gridSpawner.ResetGridSpawnState();

        // Reset game state 
        if (GameManager.Instance != null)
            GameManager.Instance.ResetExperienceState();

        // Return to start screen
        if (UIFlowManager.Instance != null)
            UIFlowManager.Instance.ShowWelcomeScreen();
    }


    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
