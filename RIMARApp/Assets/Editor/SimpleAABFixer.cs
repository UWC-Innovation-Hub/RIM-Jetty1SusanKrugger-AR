using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARSubsystems;
using System.IO;

public class SimpleAABFixer : EditorWindow
{
    [MenuItem("Tools/Simple AAB Fixer")]
    public static void ShowWindow()
    {
        GetWindow<SimpleAABFixer>("Simple AAB Fixer");
    }
    
    [MenuItem("Tools/Quick Fix All AAB Issues")]
    public static void QuickFixAll()
    {
        Debug.Log("🚀 Running complete AAB fix...");
        
        bool hasChanges = false;
        
        // Fix 1: Delete empty reference image libraries
        hasChanges |= DeleteEmptyImageLibraries();
        
        // Fix 2: Fix Unity 6 signing settings
        hasChanges |= FixUnity6Settings();
        
        // Fix 3: Verify keystore
        VerifyKeystoreSettings();
        
        if (hasChanges)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("🎉 All AAB issues fixed!");
        }
        else
        {
            Debug.Log("✅ No issues found - ready to build!");
        }
        
        Debug.Log("\n🚀 NEXT STEPS:");
        Debug.Log("   1. Window > General > Build Profiles");
        Debug.Log("   2. Click 'Build App Bundle' button");
        Debug.Log("   3. Upload .aab file to Google Play Console");
    }
    
    static bool DeleteEmptyImageLibraries()
    {
        Debug.Log("🔍 Checking for empty image libraries...");
        
        bool hasDeleted = false;
        string[] libraryGuids = AssetDatabase.FindAssets("t:XRReferenceImageLibrary");
        
        foreach (string guid in libraryGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var library = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(assetPath);
            
            if (library != null && library.count == 0)
            {
                Debug.Log($"🗑️ Deleting empty library: {assetPath}");
                AssetDatabase.DeleteAsset(assetPath);
                hasDeleted = true;
            }
        }
        
        if (!hasDeleted)
        {
            Debug.Log("✅ No empty libraries found");
        }
        
        return hasDeleted;
    }
    
    static bool FixUnity6Settings()
    {
        Debug.Log("🔧 Checking Unity 6 settings...");
        
        bool changed = false;
        
        // Fix Target API Level
        if (PlayerSettings.Android.targetSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto)
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            Debug.Log("✅ Fixed: Target API Level → 34");
            changed = true;
        }
        
        // Fix Scripting Backend
        if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            Debug.Log("✅ Fixed: Scripting Backend → IL2CPP");
            changed = true;
        }
        
        // Fix Architecture
        if (!PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARM64))
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log("✅ Fixed: Architecture → ARM64");
            changed = true;
        }
        
        // Fix Build App Bundle
        if (!EditorUserBuildSettings.buildAppBundle)
        {
            EditorUserBuildSettings.buildAppBundle = true;
            Debug.Log("✅ Fixed: Build App Bundle → Enabled");
            changed = true;
        }
        
        return changed;
    }
    
    static void VerifyKeystoreSettings()
    {
        Debug.Log("🔑 Verifying keystore settings...");
        
        if (!PlayerSettings.Android.useCustomKeystore)
        {
            Debug.LogError("❌ Custom keystore not enabled!");
            Debug.LogError("   → Go to Player Settings > Publishing Settings");
            return;
        }
        
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystoreName))
        {
            Debug.LogError("❌ Keystore path not set!");
            return;
        }
        
        if (!File.Exists(PlayerSettings.Android.keystoreName))
        {
            Debug.LogError($"❌ Keystore file not found: {PlayerSettings.Android.keystoreName}");
            return;
        }
        
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
        {
            Debug.LogError("❌ Keystore passwords not set!");
            return;
        }
        
        Debug.Log("✅ Keystore settings verified");
        Debug.Log($"   Keystore: {PlayerSettings.Android.keystoreName}");
        Debug.Log($"   Alias: {PlayerSettings.Android.keyaliasName}");
    }
    
    void OnGUI()
    {
        GUILayout.Label("🛠️ Simple AAB Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("🚀 Fix All AAB Issues", GUILayout.Height(40)))
        {
            QuickFixAll();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("🗑️ Delete Empty Image Libraries"))
        {
            bool result = DeleteEmptyImageLibraries();
            if (result) AssetDatabase.SaveAssets();
        }
        
        if (GUILayout.Button("🔧 Fix Unity 6 Settings"))
        {
            bool result = FixUnity6Settings();
            if (result) AssetDatabase.SaveAssets();
        }
        
        if (GUILayout.Button("🔑 Verify Keystore"))
        {
            VerifyKeystoreSettings();
        }
        
        GUILayout.Space(20);
        
        GUILayout.Label("Current Settings:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Target API Level:", PlayerSettings.Android.targetSdkVersion.ToString());
        EditorGUILayout.LabelField("Scripting Backend:", PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android).ToString());
        EditorGUILayout.LabelField("Build App Bundle:", EditorUserBuildSettings.buildAppBundle.ToString());
        EditorGUILayout.LabelField("Custom Keystore:", PlayerSettings.Android.useCustomKeystore.ToString());
    }
}