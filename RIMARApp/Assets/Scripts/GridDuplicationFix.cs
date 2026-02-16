using UnityEngine;
using System.Linq;

public class GridDuplicationFix : MonoBehaviour
{
    private void Start()
    {
        CheckForDuplicateGrids();
    }

    [ContextMenu("Check For Duplicate Grids")]
    public void CheckForDuplicateGrids()
    {
        // Find all GameObjects with GridCoordinateSystem
        GridCoordinateSystem[] grids = FindObjectsOfType<GridCoordinateSystem>();
        Debug.Log($"=== GRID DUPLICATION CHECK ===");
        Debug.Log($"Number of GridCoordinateSystem found: {grids.Length}");
        
        foreach (GridCoordinateSystem grid in grids)
        {
            Debug.Log($"Grid: {grid.gameObject.name} at {grid.transform.position}");
            Debug.Log($"  - Children: {grid.transform.childCount}");
            Debug.Log($"  - Active: {grid.gameObject.activeSelf}");
        }
        
        // Count all cubes
        ClickableCube[] cubes = FindObjectsOfType<ClickableCube>();
        Debug.Log($"Total ClickableCube components: {cubes.Length}");
        
        // Group by position to find overlapping cubes
        var positionGroups = cubes.GroupBy(c => c.transform.position).Where(g => g.Count() > 1);
        
        if (positionGroups.Any())
        {
            Debug.LogWarning("⚠️ DUPLICATE CUBES FOUND AT SAME POSITION:");
            foreach (var group in positionGroups)
            {
                Debug.LogWarning($"  Position {group.Key}: {group.Count()} cubes");
                foreach (var cube in group)
                {
                    Debug.LogWarning($"    - {cube.gameObject.name} (Path: {GetGameObjectPath(cube.gameObject)})");
                }
            }
        }
        else
        {
            Debug.Log("✓ No overlapping cubes found");
        }
        
        // Check for cloned GameObjects (names with Clone)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        var clones = allObjects.Where(go => go.name.Contains("(Clone)")).ToArray();
        
        if (clones.Length > 0)
        {
            Debug.LogWarning($"⚠️ FOUND {clones.Length} CLONED OBJECTS:");
            foreach (GameObject clone in clones)
            {
                Debug.LogWarning($"  - {clone.name} at {clone.transform.position}");
            }
        }
        
        Debug.Log($"===============================");
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return "/" + path;
    }
}
