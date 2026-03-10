using UnityEngine;

public class FingerprintClickHandler : MonoBehaviour
{
    public void OnClicked()
    {
        Debug.Log($"{gameObject.name} clicked!");
        // Optional: highlight the fingerprint when clicked
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.yellow;
        }
    }
}