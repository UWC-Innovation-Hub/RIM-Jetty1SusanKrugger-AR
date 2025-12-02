using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshReferenceObjectCreator : MonoBehaviour
{
    [Header("Mesh Reference Object Setup")]
    [SerializeField] private GameObject targetObject;
    [SerializeField] private string referenceObjectName = "MyReferenceObject";
    [SerializeField] private Vector3 physicalSize = new Vector3(0.1f, 0.1f, 0.1f); // in meters
    
    [Header("Mesh Processing")]
    [SerializeField] private bool combineSubmeshes = true;
    [SerializeField] private bool generateNormals = true;
    [SerializeField] private float maxVertexCount = 1000f;
    
    [Header("Debug")]
    [SerializeField] private bool showMeshInfo = true;
    
    #if UNITY_EDITOR
    [ContextMenu("Create Reference Object From Mesh")]
    public void CreateReferenceObjectFromMesh()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target Object is not assigned!");
            return;
        }
        
        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("Target Object must have a MeshFilter component with a valid mesh!");
            return;
        }
        
        Mesh sourceMesh = meshFilter.sharedMesh;
        
        if (showMeshInfo)
        {
            LogMeshInfo(sourceMesh);
        }
        
        // Process the mesh for AR tracking
        Mesh processedMesh = ProcessMeshForTracking(sourceMesh);
        
        // Create the reference object
        CreateReferenceObjectAsset(processedMesh);
    }
    
    private Mesh ProcessMeshForTracking(Mesh sourceMesh)
    {
        Mesh processedMesh = Instantiate(sourceMesh);
        processedMesh.name = $"{referenceObjectName}_ProcessedMesh";
        
        // Combine submeshes if requested
        if (combineSubmeshes && sourceMesh.subMeshCount > 1)
        {
            CombineInstance[] combine = new CombineInstance[sourceMesh.subMeshCount];
            for (int i = 0; i < sourceMesh.subMeshCount; i++)
            {
                combine[i].mesh = sourceMesh;
                combine[i].subMeshIndex = i;
                combine[i].transform = Matrix4x4.identity;
            }
            
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine, true);
            processedMesh = combinedMesh;
        }
        
        // Generate normals if requested
        if (generateNormals && (processedMesh.normals == null || processedMesh.normals.Length == 0))
        {
            processedMesh.RecalculateNormals();
        }
        
        // Simplify mesh if vertex count is too high
        if (processedMesh.vertexCount > maxVertexCount)
        {
            Debug.LogWarning($"Mesh has {processedMesh.vertexCount} vertices. Consider simplifying for better AR tracking performance.");
            // Note: Unity doesn't have built-in mesh simplification. Consider using third-party tools.
        }
        
        return processedMesh;
    }
    
    private void CreateReferenceObjectAsset(Mesh mesh)
    {
        // Create the asset path
        string assetPath = $"Assets/RIMJetty1Ex/ReferenceObjects/{referenceObjectName}.asset";
        
        // Ensure directory exists
        string directory = System.IO.Path.GetDirectoryName(assetPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        // Save the processed mesh as an asset
        AssetDatabase.CreateAsset(mesh, assetPath.Replace(".asset", "_Mesh.asset"));
        
        Debug.Log($"Reference object mesh saved to: {assetPath.Replace(".asset", "_Mesh.asset")}");
        Debug.Log("Note: For actual 3D object tracking, you'll need to scan real objects using ARKit's scanning tools on iOS devices.");
        Debug.Log("This mesh can be used as a visual reference or for custom tracking implementations.");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private void LogMeshInfo(Mesh mesh)
    {
        Debug.Log($"=== Mesh Information for {mesh.name} ===");
        Debug.Log($"Vertices: {mesh.vertexCount}");
        Debug.Log($"Triangles: {mesh.triangles.Length / 3}");
        Debug.Log($"Submeshes: {mesh.subMeshCount}");
        Debug.Log($"Has Normals: {mesh.normals != null && mesh.normals.Length > 0}");
        Debug.Log($"Has UVs: {mesh.uv != null && mesh.uv.Length > 0}");
        Debug.Log($"Bounds: {mesh.bounds}");
        Debug.Log($"Physical Size (assigned): {physicalSize}");
    }
    
    [ContextMenu("Validate Mesh for AR Tracking")]
    public void ValidateMeshForTracking()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target Object is not assigned!");
            return;
        }
        
        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("Target Object must have a MeshFilter component with a valid mesh!");
            return;
        }
        
        Mesh mesh = meshFilter.sharedMesh;
        
        Debug.Log("=== AR Tracking Mesh Validation ===");
        
        // Check vertex count
        if (mesh.vertexCount > 1000)
        {
            Debug.LogWarning($"High vertex count ({mesh.vertexCount}). Consider simplifying mesh for better tracking performance.");
        }
        else
        {
            Debug.Log($"✓ Vertex count ({mesh.vertexCount}) is reasonable for AR tracking.");
        }
        
        // Check for normals
        if (mesh.normals == null || mesh.normals.Length == 0)
        {
            Debug.LogWarning("Mesh has no normals. Normals may improve tracking quality.");
        }
        else
        {
            Debug.Log("✓ Mesh has normals.");
        }
        
        // Check mesh complexity
        float complexity = (float)mesh.triangles.Length / mesh.vertexCount;
        if (complexity > 6)
        {
            Debug.LogWarning($"High mesh complexity ratio ({complexity:F2}). Simpler geometry may track better.");
        }
        else
        {
            Debug.Log($"✓ Mesh complexity ratio ({complexity:F2}) is good for tracking.");
        }
        
        // Check bounds
        Vector3 size = mesh.bounds.size;
        if (size.x < 0.05f || size.y < 0.05f || size.z < 0.05f)
        {
            Debug.LogWarning("Object may be too small for reliable tracking. Consider larger objects.");
        }
        else
        {
            Debug.Log("✓ Object size appears suitable for tracking.");
        }
    }
    
    #endif
    
    public void SetTargetObject(GameObject obj)
    {
        targetObject = obj;
    }
    
    public void SetPhysicalSize(Vector3 size)
    {
        physicalSize = size;
    }
}