using UnityEngine;
using TMPro;


public class CountdownTimer : MonoBehaviour
{
    [Tooltip("Total countdown time in seconds.")]
    public int countdownTime = 300; // 5 minutes = 300 seconds

    [Tooltip("Reference to the TMP text for the timer.")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isFlashing = false;


    void Start()
    {
        ResetTimer();
    }


    void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
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
}
