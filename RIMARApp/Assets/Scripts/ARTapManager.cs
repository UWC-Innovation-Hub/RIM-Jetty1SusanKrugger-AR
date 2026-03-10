using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARTapManager : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Interactable"))
                    {
                        hit.collider.GetComponent<FingerprintClickHandler>()?.OnClicked();
                    }
                }
            }
        }
    }
}