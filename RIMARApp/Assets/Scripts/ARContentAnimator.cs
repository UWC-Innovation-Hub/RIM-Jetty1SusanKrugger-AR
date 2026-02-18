using UnityEngine;

public class ARContentAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private bool rotateContent = true;
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 50, 0);

    [SerializeField] private bool scaleAnimation = true;
    [SerializeField] private float scaleSpeed = 1f;
    [SerializeField] private float scaleMin = 0.8f;
    [SerializeField] private float scaleMax = 1.2f;

    [SerializeField] private bool floatAnimation = false;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmplitude = 0.1f;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private float timeOffset;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialScale = transform.localScale;
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (rotateContent)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (scaleAnimation)
        {
            float scale = Mathf.Lerp(scaleMin, scaleMax, (Mathf.Sin(Time.time * scaleSpeed + timeOffset) + 1f) / 2f);
            transform.localScale = initialScale * scale;
        }

        if (floatAnimation)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatAmplitude;
            transform.localPosition = initialPosition + new Vector3(0, yOffset, 0);
        }
    }
}
