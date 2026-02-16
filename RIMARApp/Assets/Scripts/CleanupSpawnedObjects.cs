using UnityEngine;

public class CleanupSpawnedObjects : MonoBehaviour
{
    [ContextMenu("Clear All Spawned Objects")]
    public void ClearAllSpawnedObjects()
    {
        // Find all spawned objects by name pattern
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("SpawnedBy") || obj.name.Contains("ExampleSpawnObject"))
            {
                DestroyImmediate(obj);
                count++;
            }
        }
        
        // Reset all cubes
        ClickableCube[] cubes = FindObjectsOfType<ClickableCube>();
        foreach (ClickableCube cube in cubes)
        {
            cube.ResetCube();
        }
        
        Debug.Log($"Cleaned up {count} spawned objects and reset {cubes.Length} cubes");
    }

    [ContextMenu("List All Objects In Scene")]
    public void ListAllObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        Debug.Log($"=== Total Objects: {allObjects.Length} ===");
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.GetComponent<MeshRenderer>() != null)
            {
                Debug.Log($"Mesh Object: {obj.name} at {obj.transform.position}");
            }
        }
    }
}
