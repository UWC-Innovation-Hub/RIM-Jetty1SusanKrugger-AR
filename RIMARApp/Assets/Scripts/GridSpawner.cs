using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;


public class GridSpawner : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;

    public GameObject cubePrefab;

    [SerializeField] private float marquetteWidth; // 2 meters
    [SerializeField] private float marquetteHeight; // 1 meter
    [SerializeField] private float cellSize; // 2.5cm
    [SerializeField] private float cubeHeight; // 0.002f
    [SerializeField] private float qrSize; // 0.025f | 0.0125f for spawning at the corner of the qr code

    private GameObject currentGridParent; // parent all the cubes under one object so can delete them easily.

    //private bool gridSpawned = false;


    private void OnEnable()
    {
        imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }


    private void OnDisable()
    {
        imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }


    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var trackedImage in args.added)
        {
            SpawnGridFromCorner(trackedImage);
        }
    }


    void SpawnGridFromCorner(ARTrackedImage trackedImage)
    {
        // Delete old grid if exists
        if (currentGridParent != null)
        {
            Destroy(currentGridParent);
        }

        currentGridParent = new GameObject("GridParent");

        int columns = Mathf.RoundToInt(marquetteWidth / cellSize);
        int rows = Mathf.RoundToInt(marquetteHeight /  cellSize);

        float halfQR = qrSize / 2f;

        Vector3 qrCenter = trackedImage.transform.position;

        Vector3 xDirection = trackedImage.transform.right;
        Vector3 zDirection = trackedImage.transform.forward;

        string qrName = trackedImage.referenceImage.name;

        // Determine correct directions and corner offset
        Vector3 startCorner = qrCenter;

        if (qrName == "QR_TopLeft")
        {
            startCorner = qrCenter
                - (xDirection * halfQR)
                + (zDirection * halfQR);

            //xDirection = xDirection;
            zDirection = -zDirection;
        }
        else if (qrName == "QR_TopRight")
        {
            startCorner = qrCenter
                + (xDirection * halfQR)
                + (zDirection * halfQR);

            xDirection = -xDirection;
            zDirection = -zDirection;
        }
        else if (qrName == "QR_BottomLeft")
        {
            startCorner = qrCenter
                - (xDirection * halfQR)
                - (zDirection * halfQR);

            //xDirection = xDirection;
            //zDirection = zDirection;
        }
        else if (qrName == "QR_BottomRight")
        {
            startCorner = qrCenter
                + (xDirection * halfQR)
                - (zDirection * halfQR);

            xDirection = -xDirection;
            //zDirection = zDirection;
        }

        for (int x = 0; x < columns; x++)
        {
            for (int z =0; z < rows; z++)
            {
                Vector3 spawnPosition =
                    startCorner +
                    (xDirection * x * cellSize) +
                    (zDirection * z * cellSize);

                GameObject cube = Instantiate(
                    cubePrefab,
                    spawnPosition,
                    trackedImage.transform.rotation
                );

                cube.transform.localScale =
                    new Vector3(cellSize, cubeHeight, cellSize);

                cube.transform.parent = currentGridParent.transform;

                // Colour all quadrants
                bool leftSide = x < columns / 2;
                bool bottomSide = z < rows / 2;

                if (leftSide && !bottomSide)
                    cube.GetComponent<Renderer>().material.color = Color.red;
                else if (!leftSide && !bottomSide)
                    cube.GetComponent<Renderer>().material.color = Color.green;
                else if (leftSide && bottomSide)
                    cube.GetComponent<Renderer>().material.color = Color.blue;
                else
                    cube.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }

        // Make grid stable by detaching from tracking updates
        currentGridParent.transform.position = currentGridParent.transform.position;
    }
}
