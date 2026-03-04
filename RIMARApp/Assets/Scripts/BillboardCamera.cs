using UnityEngine;

public class BillboardCamera : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Distance Scaling")]
    [SerializeField] private float scaleMultiplier;
    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;


    void Start()
    {
        mainCamera = Camera.main;
    }


    void LateUpdate()
    {
        /*if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180f, 0);
        }*/

        if (mainCamera == null) return;

        // Always face camera
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180f, 0);

        // Distance-based scaling
        float distance = Vector3.Distance(
            mainCamera.transform.position,
            transform.position
        );

        float scale = distance * scaleMultiplier;
        scale = Mathf.Clamp(scale, minScale, maxScale);

        transform.localScale = Vector3.one * scale;
    }
}
