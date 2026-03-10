using UnityEngine;

public class ARHoldManager : MonoBehaviour
{
    private FingerprintHoldHandler currentHeld = null;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (hit.collider.CompareTag("Interactable"))
                        {
                            currentHeld = hit.collider.GetComponent<FingerprintHoldHandler>();
                            currentHeld?.StartHold();
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (currentHeld != null)
                    {
                        currentHeld.EndHold();
                        currentHeld = null;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    // Optional: continue holding
                    break;
            }
        }

        // Editor testing with mouse
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    currentHeld = hit.collider.GetComponent<FingerprintHoldHandler>();
                    currentHeld?.StartHold();
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (currentHeld != null)
            {
                currentHeld.EndHold();
                currentHeld = null;
            }
        }
    }
}