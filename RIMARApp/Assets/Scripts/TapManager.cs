using UnityEngine;

public class TapManager : MonoBehaviour
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
                    InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        interactable.OnTapped();
                    }
                }
            }
        }
    }
}