using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ARScenePrefabExporter : MonoBehaviour
{
    [Header("Export Configuration")]
    [SerializeField] private string prefabName = "QRSceneAnchor";
    [SerializeField] private string exportPath = "Assets/Prefabs/Export/";
    
    [Header("Scene Objects")]
    [SerializeField] private List<SceneObjectData> sceneObjects = new List<SceneObjectData>();

#if UNITY_EDITOR
    [ContextMenu("Create AR Scene Prefab")]
    public void CreateARScenePrefab()
    {
        GameObject rootAnchor = new GameObject(prefabName);
        rootAnchor.transform.position = Vector3.zero;
        rootAnchor.transform.rotation = Quaternion.identity;

        AddARTrackedImageMarker(rootAnchor);

        foreach (var objData in sceneObjects)
        {
            if (objData.prefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(objData.prefab) as GameObject;
                if (instance != null)
                {
                    instance.transform.SetParent(rootAnchor.transform);
                    instance.transform.localPosition = objData.relativePosition;
                    instance.transform.localRotation = Quaternion.Euler(objData.relativeRotation);
                    instance.transform.localScale = objData.scale;
                    instance.name = objData.objectName;
                }
            }
        }

        if (!System.IO.Directory.Exists(exportPath))
        {
            System.IO.Directory.CreateDirectory(exportPath);
        }

        string fullPath = exportPath + prefabName + ".prefab";
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(rootAnchor, fullPath);
        
        DestroyImmediate(rootAnchor);

        Debug.Log($"AR Scene Prefab created at: {fullPath}");
        EditorGUIUtility.PingObject(prefabAsset);
        Selection.activeObject = prefabAsset;
    }

    private void AddARTrackedImageMarker(GameObject rootObject)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
        marker.name = "ImageTrackingMarker";
        marker.transform.SetParent(rootObject.transform);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localRotation = Quaternion.Euler(90, 0, 0);
        marker.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 1f, 0f, 0.3f);
            renderer.material = mat;
        }

        DestroyImmediate(marker.GetComponent<MeshCollider>());
    }

    [ContextMenu("Populate From Scene Spawner")]
    public void PopulateFromSceneSpawner()
    {
        QRSceneSpawner spawner = FindFirstObjectByType<QRSceneSpawner>();
        if (spawner != null)
        {
            sceneObjects.Clear();
            
            var mappingsField = spawner.GetType().GetField("sceneMappings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (mappingsField != null)
            {
                var mappings = mappingsField.GetValue(spawner) as List<QRSceneMapping>;
                if (mappings != null && mappings.Count > 0)
                {
                    foreach (var sceneObj in mappings[0].sceneObjects)
                    {
                        sceneObjects.Add(new SceneObjectData
                        {
                            objectName = sceneObj.objectName,
                            prefab = sceneObj.prefab,
                            relativePosition = sceneObj.relativePosition,
                            relativeRotation = sceneObj.relativeRotation,
                            scale = sceneObj.scale
                        });
                    }
                    
                    Debug.Log($"Populated {sceneObjects.Count} objects from QRSceneSpawner");
                }
            }
        }
        else
        {
            Debug.LogWarning("QRSceneSpawner not found in scene");
        }
    }
#endif
}

[System.Serializable]
public class SceneObjectData
{
    public string objectName;
    public GameObject prefab;
    public Vector3 relativePosition;
    public Vector3 relativeRotation;
    public Vector3 scale = Vector3.one;
}
