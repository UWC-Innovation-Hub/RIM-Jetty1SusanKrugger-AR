using UnityEngine;

public class ARTouchInteraction : MonoBehaviour
{
    private Camera arCamera;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Ended)
        {
            Ray ray = arCamera.ScreenPointToRay(touch.position);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                FingerprintInteraction target = hit.collider.GetComponent<FingerprintInteraction>();

                if (target != null)
                {
                    // ✅ Single tap detected
                    target.PlayVideo();
                }
            }
        }
    }
}