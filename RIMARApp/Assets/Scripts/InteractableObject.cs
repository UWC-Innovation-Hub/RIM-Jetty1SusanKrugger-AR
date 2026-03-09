using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    public void OnTapped()
    {
        Debug.Log("Cell tapped: " + gameObject.name);

        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.yellow;
        }
    }
}
