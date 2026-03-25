using UnityEngine;


/*
 * This script incorporates a distance-based scaling for the panels which would make the panels further away still legible.
 */

public class DistanceScaler : MonoBehaviour
{
    [SerializeField] private float minScale; //0.0015f
    [SerializeField] private float maxScale; //0.003f

    [SerializeField] private float minDistance; //0.2f
    [SerializeField] private float maxDistance; //1.5f

    private Transform cam;


    private void Start()
    {
        cam = Camera.main.transform;
    }


    private void Update()
    {
        float distance = Vector3.Distance(cam.position, transform.position);

        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float scale = Mathf.Lerp(minScale, maxScale, t);

        transform.localScale = Vector3.one * scale;
    }
}
