using UnityEngine;
using UnityEngine.InputSystem;

public class MarkerTap : MonoBehaviour
{
    public GameObject arContentRoot;

    void Update()
    {
        if (Touchscreen.current == null) return;

        if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            Vector2 pos = Touchscreen.current.primaryTouch.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    // user tapped the marker
                    arContentRoot.SetActive(true);
                    gameObject.SetActive(false); // hide the marker button
                    FindObjectOfType<GestureManager>().gesturesEnabled = true;
                }
            }
        }
    }
}
