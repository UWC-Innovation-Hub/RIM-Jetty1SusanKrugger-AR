using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager raycastManager;

    [Header("Placement")]
    public GameObject placementPrefab;

    [Header("UI")]
    public GameObject uiCanvas;

    private GameObject spawnedObject;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (spawnedObject != null)
            return; // Prevent multiple placements

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            var anchor = spawnedObject.AddComponent<ARAnchor>();
            spawnedObject = Instantiate(
                placementPrefab,
                hitPose.position,
                hitPose.rotation
            );
            spawnedObject.AddComponent<ARAnchor>();

            OnObjectPlaced();
        }
    }

    void OnObjectPlaced()
    {
        Debug.Log("Object placed");

        if (uiCanvas != null)
        {
            uiCanvas.SetActive(true);
        }

        foreach (var plane in FindObjectOfType<ARPlaneManager>().trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }
}
