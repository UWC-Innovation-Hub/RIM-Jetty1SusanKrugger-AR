using UnityEngine;
using UnityEngine.UI; // For optional progress bar UI

public class FingerprintHoldHandler : MonoBehaviour
{
    public float holdTime = 2f; // Seconds to hold
    private float holdTimer = 0f;
    private bool isHolding = false;

    // Optional: progress UI (can be a world-space UI or 3D object)
    public Image progressBar;

    void Update()
    {
        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            if (progressBar != null)
                progressBar.fillAmount = holdTimer / holdTime;

            if (holdTimer >= holdTime)
            {
                TriggerAction();
                ResetHold();
            }
        }
    }

    public void StartHold()
    {
        isHolding = true;
        holdTimer = 0f;
        if (progressBar != null)
            progressBar.fillAmount = 0f;
    }

    public void EndHold()
    {
        ResetHold();
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTimer = 0f;
        if (progressBar != null)
            progressBar.fillAmount = 0f;
    }

    private void TriggerAction()
    {
        Debug.Log($"{gameObject.name} hold complete!");
        // TODO: show popup or perform action
        // For example: instantiate a UI popup, open info panel, etc.
    }
}