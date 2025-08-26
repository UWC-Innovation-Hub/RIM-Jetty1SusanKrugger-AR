using UnityEngine;
using TMPro;


public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public int countdownTime = 300; // 5 minutes = 300 seconds
    public TextMeshProUGUI timerText;

    [Header("Clue Settings")]
    public TextMeshProUGUI clueText;         // reference to the clue TMP
    public TextMeshProUGUI instructionText;  // reference to the instruction TMP
    [TextArea] public string[] clues;        // list of clues to rotate through

    private float currentTime;
    private bool isFlashing = false;
    private int clueIndex = 0;


    void Start()
    {
        ResetTimer();

        // Set default instruction
        if (instructionText != null)
            instructionText.text = "Tap on a checkpoint to interact with the clue.";

        // Show first clue if available
        if (clues != null && clues.Length > 0)
            clueText.text = clues[clueIndex];
    }


    void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            NextClue();
            ResetTimer();
            return;
        }

        UpdateTimerDisplay();
    }


    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Flash red in the last 10 seconds
        if (currentTime <= 10f)
        {
            if (!isFlashing)
                StartCoroutine(FlashRed());
        }
    }


    System.Collections.IEnumerator FlashRed()
    {
        isFlashing = true;
        while (currentTime > 0 && currentTime <= 10f)
        {
            timerText.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            timerText.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
        isFlashing = false;
    }


    void ResetTimer()
    {
        currentTime = countdownTime;
        timerText.color = Color.white;
    }


    void NextClue()
    {
        if (clues == null || clues.Length == 0) return;

        clueIndex = (clueIndex + 1) % clues.Length; // loop back to start
        clueText.text = clues[clueIndex];
    }
}
