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
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180f, 0);
        }
    }
}
