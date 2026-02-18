using UnityEngine;

public class WorldAnchor : MonoBehaviour
{
    [Header("World-Lock Settings")]
    [SerializeField] private bool lockPosition = true;
    [SerializeField] private bool lockRotation = true;

    private Vector3 worldPosition;
    private Quaternion worldRotation;
    private bool isInitialized = false;

    public void Initialize(Vector3 position, Quaternion rotation)
    {
        worldPosition = position;
        worldRotation = rotation;
        isInitialized = true;

        transform.SetParent(null);

        Debug.Log($"WorldAnchor initialized for {gameObject.name} at position {worldPosition}");
    }

    void Start()
    {
        if (!isInitialized)
        {
            worldPosition = transform.position;
            worldRotation = transform.rotation;
            isInitialized = true;
        }

        transform.SetParent(null);
    }

    void LateUpdate()
    {
        if (!isInitialized)
            return;

        if (lockPosition && transform.position != worldPosition)
        {
            transform.position = worldPosition;
        }

        if (lockRotation && transform.rotation != worldRotation)
        {
            transform.rotation = worldRotation;
        }
    }

    public void UpdateWorldPosition(Vector3 newPosition)
    {
        worldPosition = newPosition;
        transform.position = newPosition;
    }

    public void UpdateWorldRotation(Quaternion newRotation)
    {
        worldRotation = newRotation;
        transform.rotation = newRotation;
    }

    public Vector3 GetWorldPosition()
    {
        return worldPosition;
    }

    public Quaternion GetWorldRotation()
    {
        return worldRotation;
    }
}
