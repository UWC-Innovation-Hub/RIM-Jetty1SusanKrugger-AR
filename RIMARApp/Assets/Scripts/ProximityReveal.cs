using UnityEngine;

public class ProximityReveal : MonoBehaviour
{
    public float revealDistance = 0.5f;
    private Transform arCamera;
    private Renderer objectRenderer;

    void Start()
    {
        arCamera = Camera.main.transform;
        objectRenderer = GetComponent<Renderer>();
        objectRenderer.enabled = false; // Hidden initially
    }

    void Update()
    {
        float distance = Vector3.Distance(arCamera.position, transform.position);

        if (distance < revealDistance)
        {
            objectRenderer.enabled = true;
        }
    }
}