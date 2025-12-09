using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Header("Billboard Settings")]
    [SerializeField] private bool constrainVerticalRotation = true;
    [SerializeField] private bool useMainCamera = true;
    [SerializeField] private Transform customTarget;
    
    private Camera targetCamera;
    
    void Start()
    {
        if (useMainCamera)
        {
            targetCamera = Camera.main;
        }
        else if (customTarget != null)
        {
            targetCamera = customTarget.GetComponent<Camera>();
        }
        
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera == null) return;
        
        Vector3 targetPosition = targetCamera.transform.position;
        
        if (constrainVerticalRotation)
        {
            // Keep the Y position the same to prevent tilting
            targetPosition.y = transform.position.y;
        }
        
        // Look at the camera
        transform.LookAt(targetPosition);
        
        // Flip the rotation 180 degrees so text faces the camera correctly
        transform.Rotate(0, 180, 0);
    }
}