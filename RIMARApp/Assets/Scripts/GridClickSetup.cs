using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridClickSetup : MonoBehaviour
{
    [Header("Spawn Object Settings")]
    [SerializeField] private GameObject defaultSpawnObject;
    [SerializeField] private bool assignToAllCubes = true;
    
    [Header("Grid Settings")]
    [SerializeField] private Transform gridRoot;
    
    [Header("Cube Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.05f, 0);
    [SerializeField] private float spawnScale = 1f;
    [SerializeField] private bool spawnOnlyOnce = true;

#if UNITY_EDITOR
    [ContextMenu("Setup All Cubes")]
    public void SetupAllCubes()
    {
        if (gridRoot == null)
        {
            Debug.LogError("Grid Root not assigned!");
            return;
        }

        int setupCount = 0;
        ClickableCube[] existingCubes = gridRoot.GetComponentsInChildren<ClickableCube>();
        
        foreach (Transform child in gridRoot)
        {
            ClickableCube clickable = child.GetComponent<ClickableCube>();
            
            if (clickable == null)
            {
                clickable = child.gameObject.AddComponent<ClickableCube>();
                setupCount++;
            }

            SerializedObject so = new SerializedObject(clickable);
            
            if (assignToAllCubes && defaultSpawnObject != null)
            {
                so.FindProperty("objectToSpawn").objectReferenceValue = defaultSpawnObject;
            }
            
            so.FindProperty("normalColor").colorValue = normalColor;
            so.FindProperty("highlightColor").colorValue = highlightColor;
            so.FindProperty("selectedColor").colorValue = selectedColor;
            so.FindProperty("spawnOffset").vector3Value = spawnOffset;
            so.FindProperty("spawnScale").floatValue = spawnScale;
            so.FindProperty("spawnOnlyOnce").boolValue = spawnOnlyOnce;
            
            so.ApplyModifiedProperties();
        }

        Debug.Log($"Setup complete! {setupCount} new ClickableCube components added. Total: {gridRoot.childCount} cubes configured.");
    }

    [ContextMenu("Remove All ClickableCube Components")]
    public void RemoveAllClickableCubes()
    {
        if (gridRoot == null)
        {
            Debug.LogError("Grid Root not assigned!");
            return;
        }

        ClickableCube[] clickables = gridRoot.GetComponentsInChildren<ClickableCube>();
        
        foreach (ClickableCube clickable in clickables)
        {
            DestroyImmediate(clickable);
        }

        Debug.Log($"Removed {clickables.Length} ClickableCube components");
    }

    [ContextMenu("Reset All Cubes")]
    public void ResetAllCubes()
    {
        if (gridRoot == null)
        {
            Debug.LogError("Grid Root not assigned!");
            return;
        }

        ClickableCube[] clickables = gridRoot.GetComponentsInChildren<ClickableCube>();
        
        foreach (ClickableCube clickable in clickables)
        {
            clickable.ResetCube();
        }

        Debug.Log($"Reset {clickables.Length} cubes");
    }
#endif

    public void ResetAllCubesRuntime()
    {
        if (gridRoot == null) return;

        ClickableCube[] clickables = gridRoot.GetComponentsInChildren<ClickableCube>();
        
        foreach (ClickableCube clickable in clickables)
        {
            clickable.ResetCube();
        }
    }
}
