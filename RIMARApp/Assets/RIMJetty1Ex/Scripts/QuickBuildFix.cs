using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
#endif

/// <summary>
/// Quick fix for ARCore build errors caused by empty reference image libraries
/// </summary>
public class QuickBuildFix : MonoBehaviour
{
    #if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void FixBuildIssues()
    {
        // Auto-fix when Unity loads
        FixEmptyLibraries();
    }
    
    [MenuItem("Tools/Quick Fix ARCore Build")]
    public static void FixEmptyLibraries()
    {
        Debug.Log("🛠️ Running Quick ARCore Build Fix...");
        
        int fixedCount = 0;
        
        // Find and fix empty XRReferenceImageLibrary assets
        string[] libraryGuids = AssetDatabase.FindAssets("t:XRReferenceImageLibrary");
        
        foreach (string guid in libraryGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var library = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(assetPath);
            
            if (library != null && library.count == 0)
            {
                Debug.Log($"⚠️ Found empty library: {assetPath}");
                
                // Delete empty library to prevent build errors
                AssetDatabase.DeleteAsset(assetPath);
                Debug.Log($"🗑️ Deleted empty library: {assetPath}");
                fixedCount++;
            }
        }
        
        // Remove empty library references from AR managers
        var imageManagers = FindObjectsByType<ARTrackedImageManager>(FindObjectsSortMode.None);
        foreach (var manager in imageManagers)
        {
            // Unity 6 compatibility: Check if library is XRReferenceImageLibrary and empty
            if (manager.referenceLibrary == null || 
                (manager.referenceLibrary is XRReferenceImageLibrary xrLib && xrLib.count == 0))
            {
                manager.referenceLibrary = null;
                EditorUtility.SetDirty(manager);
                Debug.Log($"🛠️ Cleared empty library from {manager.name}");
                fixedCount++;
            }
        }
        
        // Check for AR managers in prefabs as well
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                var manager = prefab.GetComponent<ARTrackedImageManager>();
                if (manager != null)
                {
                    // Unity 6 compatibility: Check if library is XRReferenceImageLibrary and empty
                    if (manager.referenceLibrary == null || 
                        (manager.referenceLibrary is XRReferenceImageLibrary xrLib && xrLib.count == 0))
                    {
                        manager.referenceLibrary = null;
                        EditorUtility.SetDirty(prefab);
                        Debug.Log($"🛠️ Fixed prefab: {prefabPath}");
                        fixedCount++;
                    }
                }
            }
        }
        
        if (fixedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✅ Fixed {fixedCount} ARCore build issues!");
            Debug.Log("   You can now build successfully");
        }
        else
        {
            Debug.Log("✅ No ARCore build issues found");
        }
    }
    #endif
}