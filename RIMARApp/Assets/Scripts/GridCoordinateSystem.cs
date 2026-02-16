using UnityEngine;
using System.Collections.Generic;

public class GridCoordinateSystem : MonoBehaviour
{
    private Dictionary<string, Transform> cells =
        new Dictionary<string, Transform>();

    void Awake()
    {
        foreach (Transform child in transform)
        {
            cells[child.name] = child;
        }
    }

    public void SpawnAt(string coordinate, GameObject prefab)
    {
        if (cells.ContainsKey(coordinate))
        {
            Instantiate(prefab,
                cells[coordinate].position,
                Quaternion.identity);
        }
        else
        {
            Debug.Log("Coordinate not found: " + coordinate);
        }
    }
}
