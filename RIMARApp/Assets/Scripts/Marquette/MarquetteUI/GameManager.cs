using System.Collections.Generic;
using UnityEngine;
using TMPro;


/*
 * This script handles:
 * 1. Clue selection (random 12)
 * 2. Current clue index
 * 3. Game state (playing / finished)
 */

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Clue Settings")]
    public ClueData[] allClues; // 30 total clues (one for each location)
    private List<ClueData> selectedClues = new List<ClueData>(); // Chosen 12 clues (going to be randomly chosen

    [SerializeField] private TextMeshProUGUI clueText;

    private int currentClueIndex = 0;

    [Header("Game Settings")]
    [SerializeField] private int cluesPerGame; // 12
    [SerializeField] private int winThreshold; // 6 (50%)

    private int successfulCaptures = 0;


    private void Awake()
    {
        Instance = this;
    }


    public void StartGame()
    {
        SelectRandomClues();
        currentClueIndex = 0;
        successfulCaptures = 0;

        UpdateClueUI();
    }


    void SelectRandomClues()
    {
        selectedClues.Clear();

        List<ClueData> tempList = new List<ClueData>(allClues);

        for (int i = 0; i < cluesPerGame; i++)
        {
            int randIndex = Random.Range(0, tempList.Count);
            selectedClues.Add(tempList[randIndex]);
            tempList.RemoveAt(randIndex);
        }
    }


    public void OnSuccessfulScreenshot()
    {
        successfulCaptures++;

        UIFlowManager.Instance.SetInstruction("Intel captured! Find the next clue.");

        // Update progress bar
        ProgressTracker.Instance.AddProgress();

        // Reduce timer
        CountdownTimer.Instance.ReduceTime(10f);

        // Move to next clue
        currentClueIndex++;

        if (currentClueIndex >= selectedClues.Count)
        {
            EndGame();
            return;
        }

        UpdateClueUI();
    }


    void UpdateClueUI()
    {
        if (clueText != null && currentClueIndex < selectedClues.Count)
        {
            clueText.text = selectedClues[currentClueIndex].clueText;
        }
    }


    public void EndGame()
    {
        Debug.Log("Game Ended");

        // Show gallery BEFORE win/lose
        GalleryManager.Instance.ShowGallery();

        // Decide win/lose
        bool win = successfulCaptures >= winThreshold;

        if (win)
            Debug.Log("PLAYER WINS");
        else
            Debug.Log("PLAYER LOSES");
    }


    /*
    public int GetCurrentLocationID()
    {
        return selectedClues[currentClueIndex].locationID;
    }
    */


    public LocationData GetCurrentTargetLocation()
    {
        return selectedClues[currentClueIndex].locationData;
    }
}


[System.Serializable]
public class ClueData
{
    public string clueText;
    public LocationData locationData; // This is key
}
