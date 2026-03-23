using UnityEngine;

public class ARTouchInteraction : MonoBehaviour
{
    private Camera arCamera;

    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;

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
                    float currentTime = Time.time;

                    if (currentTime - lastTapTime <= doubleTapThreshold)
                    {
                        // ✅ Double tap detected
                        target.PlayVideo();
                    }

                    lastTapTime = currentTime;
                }
            }
        }
    }
}