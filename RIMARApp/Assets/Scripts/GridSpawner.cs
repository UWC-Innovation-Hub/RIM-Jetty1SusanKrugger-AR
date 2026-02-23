using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
//using static UnityEditor.FilePathAttribute;


public class GridSpawner : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;

    public GameObject cubePrefab;
    public GameObject locationMarkerPrefab;

    [SerializeField] private float marquetteWidth; // 2 meters
    [SerializeField] private float marquetteHeight; // 1 meter
    [SerializeField] private float cellSize; // 2.5cm
    [SerializeField] private float cubeHeight; // 0.002f
    [SerializeField] private float qrSize; // 0.025f | 0.0125f for spawning at the corner of the qr code

    private GameObject currentGridParent; // parent all the cubes under one object so can delete them easily.

    private GameObject[,] gridArray;


    [System.Serializable]
    public class LocationPoint
    {
        public string locationNames;
        public float x_cm;
        public float z_cm;
    }

    public List<LocationPoint> locations = new List<LocationPoint>();


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
            SpawnGrid(trackedImage);
        }
    }


    void SpawnGrid(ARTrackedImage trackedImage)
    {
        // Delete old grid if exists
        if (currentGridParent != null)
            Destroy(currentGridParent);

        currentGridParent = new GameObject("GridParent");

        int columns = Mathf.RoundToInt(marquetteWidth / cellSize);
        int rows = Mathf.RoundToInt(marquetteHeight /  cellSize);

        gridArray = new GameObject[columns, rows];

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

                gridArray[x, z] = cube;
            }
        }

        // Make grid stable by detaching from tracking updates
        currentGridParent.transform.position = currentGridParent.transform.position;

        MapAllLocations();
    }


    void MapAllLocations()
    {
        foreach (LocationPoint location in locations)
        {
            int xIndex = Mathf.RoundToInt(location.x_cm / 2.5f);
            int zIndex = Mathf.RoundToInt(location.z_cm / 2.5f);

            if (xIndex >= 0 && xIndex < gridArray.GetLength(0) &&
                zIndex >= 0 && zIndex < gridArray.GetLength(1))
            {
                GameObject cube = gridArray[xIndex, zIndex];

                if (cube != null)
                {
                    cube.GetComponent<Renderer>().material.color = Color.magenta;

                    if (locationMarkerPrefab != null)
                    {
                        Instantiate(
                            locationMarkerPrefab,
                            cube.transform.position + Vector3.up * 0.01f,
                            Quaternion.identity,
                            currentGridParent.transform
                        );
                    }

                    Debug.Log("Mapped: " + location.locationNames +
                        " -> Grid (" + xIndex + ", " + zIndex + ")");
                }
            }
            else
            {
                Debug.LogWarning("Location out of bounds: " + location.locationNames);
            }
        }
    }


    // Optional: Pull full list of cubes
    public List<GameObject> GetAllCubes()
    {
        List<GameObject> cubeList = new List<GameObject>();

        foreach (GameObject cube in gridArray)
        {
            if (cube != null)
                cubeList.Add(cube);
        }

        return cubeList;
    }
}
