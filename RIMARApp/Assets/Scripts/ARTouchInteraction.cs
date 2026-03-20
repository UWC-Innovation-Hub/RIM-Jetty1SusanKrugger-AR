using UnityEngine;

public class ARTouchInteraction : MonoBehaviour
{
    private Camera arCamera;
    private FingerprintInteraction currentTarget;

    private float holdTime = 0f;
    private float requiredHoldTime = 1f;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0)
        {
            ResetTouch();
            return;
        }

        Touch touch = Input.GetTouch(0);

        Ray ray = arCamera.ScreenPointToRay(touch.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            FingerprintInteraction target = hit.collider.GetComponent<FingerprintInteraction>();

            if (target != null)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    currentTarget = target;
                    holdTime = 0f;
                }

                if (touch.phase == TouchPhase.Stationary && currentTarget == target)
                {
                    holdTime += Time.deltaTime;

                    if (holdTime >= requiredHoldTime)
                    {
                        currentTarget.SendMessage("PlayVideo");
                        ResetTouch();
                    }
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    ResetTouch();
                }
            }
        }
    }

    void ResetTouch()
    {
        holdTime = 0f;
        currentTarget = null;
    }
}
