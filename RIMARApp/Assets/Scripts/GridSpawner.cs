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

    private bool gridSpawned = false;


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
            if (!gridSpawned)
            {
                SpawnGridFromCorner(trackedImage);
                gridSpawned = true;
            }
        }
    }


    void SpawnGridFromCorner(ARTrackedImage trackedImage)
    {
        string qrName = trackedImage.referenceImage.name;

        int columns = Mathf.RoundToInt(marquetteWidth / cellSize);
        int rows = Mathf.RoundToInt(marquetteHeight /  cellSize);

        Vector3 startPosition = trackedImage.transform.position;

        Color quadrantColor = Color.white;

        Vector3 xDirection = trackedImage.transform.right;
        Vector3 zDirection = trackedImage.transform.forward;

        if (qrName == "QR_TopLeft")
        {
            quadrantColor = Color.red;
        }
        else if (qrName == "QR_TopRight")
        {
            quadrantColor = Color.green;
            xDirection = -xDirection;
        }
        else if (qrName == "QR_BottomLeft")
        {
            quadrantColor = Color.blue;
            zDirection = -zDirection;
        }
        else if (qrName == "QR_BottomRight")
        {
            quadrantColor = Color.yellow;
            xDirection = -xDirection;
            zDirection = -zDirection;
        }

        for (int x = 0; x < columns; x++)
        {
            for (int z =0; z < rows; z++)
            {
                Vector3 spawnPosition =
                    startPosition +
                    (xDirection * x * cellSize) +
                    (zDirection * z * cellSize);

                GameObject cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

                cube.transform.localScale = new Vector3(cellSize, cubeHeight, cellSize);

                if (x < columns / 2 && z < rows / 2)
                {
                    cube.GetComponent<Renderer>().material.color = quadrantColor;
                }
            }
        }
    }
}
