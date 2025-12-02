using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectTrackingDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool runDiagnosticOnStart = true;
    [SerializeField] private bool showDetailedInfo = true;
    
    [Header("Reference Objects to Check")]
    [SerializeField] private XRReferenceObjectLibrary[] librariesToCheck;
    
    [Header("AR Components")]
    [SerializeField] private ARTrackedObjectManager trackedObjectManager;
    [SerializeField] private ARSession arSession;
    
    [Header("Diagnostic Results")]
    [TextArea(10, 20)]
    [SerializeField] private string diagnosticReport = "Run diagnostic to see results...";
    
    void Start()
    {
        if (runDiagnosticOnStart)
        {
            RunCompleteDiagnostic();
        }
    }
    
    [ContextMenu("Run Complete Diagnostic")]
    public void RunCompleteDiagnostic()
    {
        diagnosticReport = GenerateCompleteDiagnosticReport();
        Debug.Log("=== OBJECT TRACKING DIAGNOSTIC REPORT ===\n" + diagnosticReport);
    }
    
    string GenerateCompleteDiagnosticReport()
    {
        var report = "🔍 3D OBJECT TRACKING DIAGNOSTIC REPORT\n";
        report += "==========================================\n\n";
        
        // Platform Support Check
        report += CheckPlatformSupport();
        
        // AR Components Check
        report += CheckARComponents();
        
        // Reference Object Libraries Check
        report += CheckReferenceObjectLibraries();
        
        // Provider Entries Check
        report += CheckProviderEntries();
        
        // Tracking State Check
        report += CheckTrackingState();
        
        // Recommendations
        report += GenerateRecommendations();
        
        return report;
    }
    
    string CheckPlatformSupport()
    {
        var section = "📱 PLATFORM SUPPORT:\n";
        section += "--------------------\n";
        
        #if UNITY_IOS
        section += "✅ iOS Platform: ARKit object tracking supported\n";
        #elif UNITY_ANDROID
        section += "⚠️  Android Platform: Limited ARCore object tracking\n";
        section += "   (Only some devices support object tracking)\n";
        #else
        section += "❌ Editor/Other Platform: No real object tracking\n";
        section += "   (Use device for testing)\n";
        #endif
        
        section += $"🎯 Target Platform: {Application.platform}\n";
        section += $"🔧 Unity Version: {Application.unityVersion}\n\n";
        
        return section;
    }
    
    string CheckARComponents()
    {
        var section = "🔧 AR COMPONENTS:\n";
        section += "-----------------\n";
        
        // Auto-find components if not assigned
        if (arSession == null) arSession = FindFirstObjectByType<ARSession>();
        if (trackedObjectManager == null) trackedObjectManager = FindFirstObjectByType<ARTrackedObjectManager>();
        
        // Check AR Session
        if (arSession != null)
        {
            section += $"✅ ARSession: Found and {(arSession.enabled ? "enabled" : "DISABLED")}\n";
        }
        else
        {
            section += "❌ ARSession: NOT FOUND\n";
        }
        
        // Check ARTrackedObjectManager
        if (trackedObjectManager != null)
        {
            section += $"✅ ARTrackedObjectManager: Found and {(trackedObjectManager.enabled ? "enabled" : "DISABLED")}\n";
            section += $"   📚 Reference Library: {(trackedObjectManager.referenceLibrary != null ? "Assigned" : "NOT ASSIGNED")}\n";
            
            if (trackedObjectManager.referenceLibrary != null)
            {
                section += $"   📖 Library Name: {trackedObjectManager.referenceLibrary.name}\n";
                section += $"   🔢 Reference Objects: {trackedObjectManager.referenceLibrary.count}\n";
            }
        }
        else
        {
            section += "❌ ARTrackedObjectManager: NOT FOUND\n";
        }
        
        section += "\n";
        return section;
    }
    
    string CheckReferenceObjectLibraries()
    {
        var section = "📚 REFERENCE OBJECT LIBRARIES:\n";
        section += "------------------------------\n";
        
        // Auto-find libraries if not assigned
        if (librariesToCheck == null || librariesToCheck.Length == 0)
        {
            var foundLibraries = new List<XRReferenceObjectLibrary>();
            
            // Check assigned library in ARTrackedObjectManager
            if (trackedObjectManager != null && trackedObjectManager.referenceLibrary != null)
            {
                foundLibraries.Add(trackedObjectManager.referenceLibrary);
            }
            
            #if UNITY_EDITOR
            // Find all libraries in project
            var guids = AssetDatabase.FindAssets("t:XRReferenceObjectLibrary");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var library = AssetDatabase.LoadAssetAtPath<XRReferenceObjectLibrary>(path);
                if (library != null && !foundLibraries.Contains(library))
                {
                    foundLibraries.Add(library);
                }
            }
            #endif
            
            librariesToCheck = foundLibraries.ToArray();
        }
        
        if (librariesToCheck.Length == 0)
        {
            section += "❌ NO REFERENCE OBJECT LIBRARIES FOUND\n";
            section += "   Create one via: Assets > Create > XR > Reference Object Library\n\n";
            return section;
        }
        
        foreach (var library in librariesToCheck)
        {
            if (library == null) continue;
            
            section += $"📖 Library: {library.name}\n";
            section += $"   🔢 Reference Objects: {library.count}\n";
            
            if (library.count == 0)
            {
                section += "   ⚠️  WARNING: No reference objects in library\n";
            }
            else
            {
                for (int i = 0; i < library.count; i++)
                {
                    var refObj = library[i];
                    section += $"   📄 [{i}] Name: \"{refObj.name}\"\n";
                    section += $"       🆔 GUID: {refObj.guid}\n";
                    
                    // Check provider entries
                    var entriesCount = GetProviderEntriesCount(refObj);
                    if (entriesCount == 0)
                    {
                        section += "       ❌ NO PROVIDER ENTRIES (This is the main issue!)\n";
                    }
                    else
                    {
                        section += $"       ✅ Provider Entries: {entriesCount}\n";
                    }
                }
            }
            section += "\n";
        }
        
        return section;
    }
    
    string CheckProviderEntries()
    {
        var section = "🔧 PROVIDER ENTRIES ANALYSIS:\n";
        section += "-----------------------------\n";
        
        bool foundAnyEntries = false;
        bool foundEmptyEntries = false;
        
        foreach (var library in librariesToCheck)
        {
            if (library == null) continue;
            
            for (int i = 0; i < library.count; i++)
            {
                var refObj = library[i];
                var entriesCount = GetProviderEntriesCount(refObj);
                
                if (entriesCount == 0)
                {
                    foundEmptyEntries = true;
                    section += $"❌ \"{refObj.name}\": No tracking data\n";
                    section += "   This object cannot be tracked without provider entries\n";
                }
                else
                {
                    foundAnyEntries = true;
                    section += $"✅ \"{refObj.name}\": Has tracking data\n";
                }
            }
        }
        
        if (!foundAnyEntries)
        {
            section += "\n💥 CRITICAL ISSUE: NO OBJECTS HAVE TRACKING DATA\n";
            section += "   Without provider entries (.arobject files for ARKit),\n";
            section += "   the AR system cannot recognize any objects.\n";
        }
        
        if (foundEmptyEntries)
        {
            section += "\n🛠️  SOLUTION: Add Provider Entries\n";
            section += "   For ARKit (iOS): Import .arobject files\n";
            section += "   For ARCore (Android): Check device compatibility\n";
        }
        
        section += "\n";
        return section;
    }
    
    string CheckTrackingState()
    {
        var section = "🎯 CURRENT TRACKING STATE:\n";
        section += "--------------------------\n";
        
        if (trackedObjectManager == null)
        {
            section += "❌ Cannot check tracking state (no ARTrackedObjectManager)\n\n";
            return section;
        }
        
        if (!trackedObjectManager.enabled)
        {
            section += "⚠️  ARTrackedObjectManager is DISABLED\n\n";
            return section;
        }
        
        if (Application.isPlaying)
        {
            section += $"🔍 Active Trackables: {trackedObjectManager.trackables.count}\n";
            
            if (trackedObjectManager.trackables.count == 0)
            {
                section += "   No objects currently being tracked\n";
            }
            else
            {
                foreach (var trackable in trackedObjectManager.trackables)
                {
                    section += $"   📍 {trackable.referenceObject.name}: {trackable.trackingState}\n";
                }
            }
        }
        else
        {
            section += "⏸️  Not in Play Mode - cannot check active tracking\n";
        }
        
        section += "\n";
        return section;
    }
    
    string GenerateRecommendations()
    {
        var section = "💡 RECOMMENDATIONS:\n";
        section += "-------------------\n";
        
        bool hasIssues = false;
        
        // Check for main issues
        if (trackedObjectManager == null)
        {
            section += "1. ➕ Add ARTrackedObjectManager to XR Origin\n";
            hasIssues = true;
        }
        
        if (librariesToCheck.Length == 0)
        {
            section += "2. 📚 Create Reference Object Library:\n";
            section += "   Assets > Create > XR > Reference Object Library\n";
            hasIssues = true;
        }
        
        bool hasEmptyEntries = false;
        foreach (var library in librariesToCheck)
        {
            if (library == null) continue;
            for (int i = 0; i < library.count; i++)
            {
                if (GetProviderEntriesCount(library[i]) == 0)
                {
                    hasEmptyEntries = true;
                    break;
                }
            }
        }
        
        if (hasEmptyEntries)
        {
            section += "3. 🎯 MAIN ISSUE: Add Object Scanning Data:\n";
            section += "   iOS: Use ARKit Object Scanning to create .arobject files\n";
            section += "   - Use iOS ARKit Object Scanning app\n";
            section += "   - Or Unity's AR Foundation samples\n";
            section += "   - Import .arobject files into Unity\n";
            section += "   - Assign to Reference Object entries\n\n";
            section += "   Android: Check ARCore device compatibility\n";
            section += "   - Limited device support for object tracking\n";
            section += "   - Consider using image tracking as alternative\n";
            hasIssues = true;
        }
        
        if (!hasIssues)
        {
            section += "✅ Setup looks good! Try building to device for testing.\n";
        }
        
        section += "\n🔧 Quick Fix Script Available:\n";
        section += "   Use the ObjectTrackingSetupHelper component\n";
        section += "   to create test reference objects and check setup.\n";
        
        return section;
    }
    
    int GetProviderEntriesCount(XRReferenceObject referenceObject)
    {
        // Use reflection to access private/internal entries
        try
        {
            var type = typeof(XRReferenceObject);
            var entriesField = type.GetField("m_Entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (entriesField != null)
            {
                var entries = entriesField.GetValue(referenceObject) as System.Collections.IList;
                return entries?.Count ?? 0;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not access provider entries: {e.Message}");
        }
        
        return 0;
    }
    
    #if UNITY_EDITOR
    [MenuItem("AR Tools/Run Object Tracking Diagnostic")]
    static void RunDiagnosticFromMenu()
    {
        var diagnostic = FindFirstObjectByType<ObjectTrackingDiagnostic>();
        if (diagnostic == null)
        {
            var go = new GameObject("Object Tracking Diagnostic");
            diagnostic = go.AddComponent<ObjectTrackingDiagnostic>();
        }
        
        diagnostic.RunCompleteDiagnostic();
    }
    #endif
}