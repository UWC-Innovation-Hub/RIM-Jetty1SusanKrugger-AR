using UnityEngine;


/*
 * This script makes the info panel look at the camera constantly.
 */

public class BillboardUI : MonoBehaviour
{
    Camera cam;


    private void Start()
    {
        cam = Camera.main;
    }


    private void LateUpdate()
    {
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180, 0);
    }
}
