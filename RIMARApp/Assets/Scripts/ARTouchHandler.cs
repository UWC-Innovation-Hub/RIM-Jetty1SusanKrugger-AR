using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARTouchHandler : MonoBehaviour
{
    [Header("AR References")]
    [SerializeField] private Camera arCamera;
    
    [Header("Touch Settings")]
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private float maxRaycastDistance = 10f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;

    private void Start()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }

        if (arCamera == null)
        {
            Debug.LogError("ARTouchHandler: No AR Camera found!");
        }
    }

    private void Update()
    {
        HandleTouch();
    }

    private void HandleTouch()
    {
        if (arCamera == null) return;

        if (Input.touchCount > 0)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                ProcessTouchRaycast(touch.position);
            }
        }
        
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            ProcessTouchRaycast(Input.mousePosition);
        }
#endif
    }

    private void ProcessTouchRaycast(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        
        if (showDebugRays)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxRaycastDistance, Color.red, 1f);
        }

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, interactableLayers))
        {
            Debug.Log($"AR Touch hit: {hit.collider.gameObject.name}");
            
            ClickableCube clickableCube = hit.collider.GetComponent<ClickableCube>();
            if (clickableCube != null)
            {
                clickableCube.OnCubeClicked();
            }
            else
            {
                Debug.Log($"Touched {hit.collider.gameObject.name} but it doesn't have ClickableCube component");
            }
        }
        else
        {
            if (showDebugRays)
            {
                Debug.Log("AR Touch didn't hit anything");
            }
        }
    }

    public void SetARCamera(Camera camera)
    {
        arCamera = camera;
    }
}
