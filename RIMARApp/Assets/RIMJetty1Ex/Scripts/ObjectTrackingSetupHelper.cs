using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectTrackingSetupHelper : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private XRReferenceObjectLibrary targetLibrary;
    [SerializeField] private string[] objectNames = { "DeckofCards", "TestObject" };
    
    [Header("Test .arobject Files")]
    [SerializeField] private bool useTestARObjectFile = true;
    [SerializeField] private string testARObjectFileName = "TestCube.arobject";
    
    [Header("Actions")]
    [Space(10)]
    public bool runSetup = false;
    public bool copyTestFile = false;
    public bool createLibrary = false;
    
    void OnValidate()
    {
        if (runSetup)
        {
            runSetup = false;
            PerformCompleteSetup();
        }
        
        if (copyTestFile)
        {
            copyTestFile = false;
            CopyTestARObjectFile();
        }
        
        if (createLibrary)
        {
            createLibrary = false;
            CreateNewReferenceObjectLibrary();
        }
    }
    
    [ContextMenu("Perform Complete Setup")]
    public void PerformCompleteSetup()
    {
        Debug.Log("🔧 Starting Object Tracking Setup...");
        
        // Step 1: Ensure we have a reference library
        if (targetLibrary == null)
        {
            CreateNewReferenceObjectLibrary();
        }
        
        // Step 2: Copy test .arobject file if requested
        if (useTestARObjectFile)
        {
            CopyTestARObjectFile();
        }
        
        // Step 3: Set up AR components
        SetupARComponents();
        
        Debug.Log("✅ Object Tracking Setup Complete!");
        Debug.Log("📝 Next Steps:");
        Debug.Log("   1. Build to iOS device for testing");
        Debug.Log("   2. Create real .arobject files using ARKit Object Scanning");
        Debug.Log("   3. Replace test files with real scanning data");
    }
    
    void CreateNewReferenceObjectLibrary()
    {
        #if UNITY_EDITOR
        string path = "Assets/RIMJetty1Ex/Reference Objects/GeneratedObjectLibrary.asset";
        
        // Create the library
        var library = ScriptableObject.CreateInstance<XRReferenceObjectLibrary>();
        
        // Create directory if it doesn't exist
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Save the asset
        AssetDatabase.CreateAsset(library, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        targetLibrary = library;
        
        Debug.Log($"✅ Created new Reference Object Library: {path}");
        
        // Add default objects
        AddReferenceObjectsToLibrary();
        #endif
    }
    
    void AddReferenceObjectsToLibrary()
    {
        #if UNITY_EDITOR
        if (targetLibrary == null) return;
        
        var serializedLibrary = new SerializedObject(targetLibrary);
        var referenceObjectsProperty = serializedLibrary.FindProperty("m_ReferenceObjects");
        
        foreach (string objectName in objectNames)
        {
            // Create a new reference object entry
            referenceObjectsProperty.arraySize++;
            var newEntry = referenceObjectsProperty.GetArrayElementAtIndex(referenceObjectsProperty.arraySize - 1);
            
            var nameProperty = newEntry.FindPropertyRelative("m_Name");
            var guidLowProperty = newEntry.FindPropertyRelative("m_GuidLow");
            var guidHighProperty = newEntry.FindPropertyRelative("m_GuidHigh");
            var entriesProperty = newEntry.FindPropertyRelative("m_Entries");
            
            // Set name
            nameProperty.stringValue = objectName;
            
            // Generate random GUID
            var guid = System.Guid.NewGuid();
            var guidBytes = guid.ToByteArray();
            guidLowProperty.longValue = System.BitConverter.ToInt64(guidBytes, 0);
            guidHighProperty.longValue = System.BitConverter.ToInt64(guidBytes, 8);
            
            // Initialize empty entries array
            entriesProperty.arraySize = 0;
            
            Debug.Log($"➕ Added reference object: {objectName}");
        }
        
        serializedLibrary.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetLibrary);
        AssetDatabase.SaveAssets();
        #endif
    }
    
    void CopyTestARObjectFile()
    {
        #if UNITY_EDITOR
        // Source: Unity's test .arobject file
        string sourcePath = "Packages/com.unity.xr.arkit/Tests/Editor/Assets/TestCube.arobject";
        string targetPath = $"Assets/RIMJetty1Ex/Reference Objects/{testARObjectFileName}";
        
        if (File.Exists(sourcePath))
        {
            // Create directory if needed
            string directory = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Copy the file
            FileUtil.CopyFileOrDirectory(sourcePath, targetPath);
            AssetDatabase.Refresh();
            
            Debug.Log($"✅ Copied test .arobject file to: {targetPath}");
            Debug.Log("📝 This is for testing only - create real .arobject files for production");
        }
        else
        {
            Debug.LogWarning($"❌ Test .arobject file not found at: {sourcePath}");
            Debug.LogWarning("   This is normal if ARKit package is not fully installed");
        }
        #endif
    }
    
    void SetupARComponents()
    {
        // Find or create XR Origin
        var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogWarning("⚠️  XR Origin not found - please ensure you have an AR scene setup");
            return;
        }
        
        // Find or add ARTrackedObjectManager
        var trackedObjectManager = xrOrigin.GetComponent<ARTrackedObjectManager>();
        if (trackedObjectManager == null)
        {
            trackedObjectManager = xrOrigin.gameObject.AddComponent<ARTrackedObjectManager>();
            Debug.Log("➕ Added ARTrackedObjectManager to XR Origin");
        }
        
        // Assign reference library
        if (targetLibrary != null)
        {
            trackedObjectManager.referenceLibrary = targetLibrary;
            Debug.Log($"🔗 Assigned reference library: {targetLibrary.name}");
        }
        
        // Enable the manager
        trackedObjectManager.enabled = true;
        
        // Find or add custom object tracking manager
        var customManager = xrOrigin.GetComponent<ObjectTrackingManager>();
        if (customManager == null)
        {
            customManager = xrOrigin.gameObject.AddComponent<ObjectTrackingManager>();
            Debug.Log("➕ Added ObjectTrackingManager to XR Origin");
        }
        
        // Configure custom manager
        if (targetLibrary != null && customManager != null)
        {
            // Use reflection to set the reference library field
            var field = customManager.GetType().GetField("referenceObjectLibrary", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(customManager, targetLibrary);
            }
        }
    }
    
    [ContextMenu("Show Setup Instructions")]
    public void ShowSetupInstructions()
    {
        string instructions = @"
🎯 3D OBJECT TRACKING SETUP INSTRUCTIONS
========================================

MAIN ISSUE: Your reference objects have no tracking data!

✅ IMMEDIATE FIXES:
1. Click 'Run Setup' to create test configuration
2. This will copy a test .arobject file for initial testing

📱 FOR REAL OBJECT TRACKING:
1. Use iOS device with ARKit
2. Download 'ARKit Object Scanning' app from App Store
3. Scan your real object (deck of cards)
4. Export .arobject file 
5. Import into Unity project
6. Assign to Reference Object Library entries

🔧 ALTERNATIVE: Unity AR Foundation Samples
1. Download AR Foundation Samples from Unity
2. Use Object Tracking scene
3. Follow scanning workflow
4. Export .arobject data

⚠️  ANDROID USERS:
- Object tracking has limited device support
- Consider using Image Tracking instead
- Check ARCore supported devices list

🎮 TESTING:
- Build to iOS device (required for object tracking)
- Editor/simulator cannot do real object recognition
- Use ARKit Object Scanning app to create reference data
";
        
        Debug.Log(instructions);
    }
    
    #if UNITY_EDITOR
    [MenuItem("AR Tools/Object Tracking Setup Helper")]
    static void ShowSetupHelper()
    {
        var helper = FindFirstObjectByType<ObjectTrackingSetupHelper>();
        if (helper == null)
        {
            var go = new GameObject("Object Tracking Setup Helper");
            helper = go.AddComponent<ObjectTrackingSetupHelper>();
        }
        
        Selection.activeGameObject = helper.gameObject;
        helper.ShowSetupInstructions();
    }
    #endif
}