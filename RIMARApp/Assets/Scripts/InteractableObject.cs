using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    // Called when the object is tapped
    public void OnTapped()
    {
        // Example interaction: toggle color between yellow and original
        if (rend.material.color != Color.yellow)
            rend.material.color = Color.yellow;
        else
            rend.material.color = Color.white;

        Debug.Log($"{name} was tapped!");
    }
}
