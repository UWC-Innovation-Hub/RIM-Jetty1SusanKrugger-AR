using UnityEngine;

public class BillboardCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }
}
