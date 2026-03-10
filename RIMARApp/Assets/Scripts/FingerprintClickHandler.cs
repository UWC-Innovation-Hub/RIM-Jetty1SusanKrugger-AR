using UnityEngine;

public class FingerprintClickHandler : MonoBehaviour
{
    // Called when this fingerprint is clicked
    public void OnClicked()
    {
        Debug.Log($"{gameObject.name} clicked!");
        // Add your custom behavior here, e.g., highlight, play sound, etc.
    }
}
