using System.Collections.Generic;
using UnityEngine;
using TMPro;


/*
 * This script handles:
 * 1. Random pathway selection
 * 2. Ordered clue progression inside the selected pathway
 * 3. Game state (playing / finished)
 * 4. Final win/lose result
 */

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Pathway Settings")]
    [SerializeField] private PathwayData[] pathways;

    private PathwayData selectedPathway;
    private int currentStepIndex = 0;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI clueText;

    [Header("Game Settings")]
    [SerializeField] private bool reduceTimeOnSuccess = true;
    [SerializeField] private float timeReductionOnSuccess = 20f;

    private int winThreshold = 0;
    private int successfulCaptures = 0;

    private bool gameEnded = false;
    private bool playerWon = false;


    private void Awake()
    {
        Instance = this;
    }


    public void StartGame()
    {
        gameEnded = false;
        playerWon = false;
        currentStepIndex = 0;
        successfulCaptures = 0;

        SelectRandomPathway();

        if (selectedPathway == null || selectedPathway.steps == null || selectedPathway.steps.Length == 0)
        {
            Debug.LogError("No valid pathway selected. Please assign pathway data in GameManager.");
            return;
        }
        
        winThreshold = Mathf.CeilToInt(selectedPathway.steps.Length / 2f);

        if (ProgressTracker.Instance != null)
        {
            ProgressTracker.Instance.SetTotalCheckpoints(selectedPathway.steps.Length);
        }

        UpdateClueUI();

        if (CountdownTimer.Instance != null)
        {
            CountdownTimer.Instance.BeginNewRun();
        }
        else
        {
            Debug.LogError("CountdownTimer.Instance is NULL in StartGame.");
        }

        Debug.Log("Game started with pathway: " + selectedPathway.pathwayName);
        Debug.Log("Win threshold: " + winThreshold);
    }


    void SelectRandomPathway()
    {
        if (pathways == null || pathways.Length == 0)
        {
            selectedPathway = null;
            return;
        }

        int randomIndex = Random.Range(0, pathways.Length);
        selectedPathway = pathways[randomIndex];
    }


    public void OnSuccessfulScreenshot()
    {
        if (gameEnded) return;
        
        successfulCaptures++;

        if (UIFlowManager.Instance != null)
            UIFlowManager.Instance.SetInstruction("Intel captured! Find the next clue.");

        if (ProgressTracker.Instance != null)
            // Update progress bar
            ProgressTracker.Instance.AddProgress();

        if (reduceTimeOnSuccess && CountdownTimer.Instance != null)
            // Reduce timer
            CountdownTimer.Instance.ReduceTime(timeReductionOnSuccess);

        // Move to next clue
        currentStepIndex++;

        if (currentStepIndex >= selectedPathway.steps.Length)
        {
            EndGame();
            return;
        }

        UpdateClueUI();
    }


    void UpdateClueUI()
    {
        if (clueText == null) return;
        if (selectedPathway == null) return;
        if (currentStepIndex < 0 || currentStepIndex >= selectedPathway.steps.Length) return;

        clueText.text = selectedPathway.steps[currentStepIndex].clueText;
    }


    public void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        
        Debug.Log("Game Ended");

        if (CountdownTimer.Instance != null)
        {
            CountdownTimer.Instance.StopTimer();
        }

        playerWon = successfulCaptures >= winThreshold;

        if (GalleryManager.Instance != null)
            // Show gallery BEFORE win/lose
            GalleryManager.Instance.ShowGallery();

        if (playerWon)
            Debug.Log("PLAYER WINS");
        else
            Debug.Log("PLAYER LOSES");
    }


    public bool DidPlayerWin()
    {
        return playerWon;
    }


    public LocationData GetCurrentTargetLocation()
    {
        if (gameEnded) return null;
        if (selectedPathway == null) return null;
        if (selectedPathway.steps == null) return null;
        if (currentStepIndex < 0 || currentStepIndex >= selectedPathway.steps.Length) return null;

        return selectedPathway.steps[currentStepIndex].locationData;
    }


    public void ResetExperienceState()
    {
        gameEnded = false;
        playerWon = false;
        currentStepIndex = 0;
        successfulCaptures = 0;
        selectedPathway = null;

        if (clueText != null)
            clueText.text = "";
    }
}
