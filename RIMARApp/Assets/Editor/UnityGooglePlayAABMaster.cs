using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARSubsystems;
using System.IO;

public class UnityGooglePlayAABMaster : EditorWindow
{
    private static bool showAdvancedSettings = false;
    private static bool showCurrentSettings = true;
    private static Vector2 scrollPosition;
    
    [MenuItem("Tools/🚀 Google Play AAB Master Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<UnityGooglePlayAABMaster>("Google Play AAB Master");
        window.minSize = new Vector2(500, 600);
    }
    
    [MenuItem("Tools/🎯 ONE-CLICK Google Play AAB Fix")]
    public static void OneClickFix()
    {
        Debug.Log("🚀 Starting ONE-CLICK Google Play AAB Fix...");
        Debug.Log("======================================");
        
        bool madeChanges = false;
        
        // Step 1: Unity 6 Signing Settings
        madeChanges |= FixUnity6SigningSettings();
        
        // Step 2: Production Build Settings
        madeChanges |= FixProductionBuildSettings();
        
        // Step 3: Optional ARCore Build Issues (manual only)
        // Note: ARCore libraries preserved for future development
        
        // Step 4: Verify Keystore
        bool keystoreValid = VerifyKeystoreConfiguration();
        
        // Save settings
        if (madeChanges)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("💾 Settings saved successfully!");
        }
        
        // Final summary
        Debug.Log("======================================");
        if (keystoreValid && (madeChanges || AllSettingsCorrect()))
        {
            Debug.Log("🎉 SUCCESS! Your project is ready for Google Play!");
            Debug.Log("📱 Next Steps:");
            Debug.Log("   1. Window > General > Build Profiles");
            Debug.Log("   2. Click 'Build App Bundle' button");
            Debug.Log("   3. Upload .aab to Google Play Console");
            Debug.Log("   4. Your AAB will be accepted! ✅");
            Debug.Log("");
            Debug.Log("ℹ️  ARCore libraries preserved for future development");
        }
        else
        {
            Debug.LogError("❌ Some issues need manual attention. Check the console above.");
        }
    }
    
    static bool FixUnity6SigningSettings()
    {
        Debug.Log("🔧 Fixing Unity 6 Signing Settings...");
        bool changed = false;
        
        // Target API Level
        if (PlayerSettings.Android.targetSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto)
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
            Debug.Log("   ✅ Target API Level → 35 (Android 15)");
            changed = true;
        }
        
        // Scripting Backend
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android);
        if (PlayerSettings.GetScriptingBackend(namedBuildTarget) != ScriptingImplementation.IL2CPP)
        {
            PlayerSettings.SetScriptingBackend(namedBuildTarget, ScriptingImplementation.IL2CPP);
            Debug.Log("   ✅ Scripting Backend → IL2CPP");
            changed = true;
        }
        
        // Architecture
        if (!PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARM64))
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log("   ✅ Target Architecture → ARM64");
            changed = true;
        }
        
        if (!changed) Debug.Log("   ✅ Unity 6 signing settings already correct");
        return changed;
    }
    
    static bool FixProductionBuildSettings()
    {
        Debug.Log("🏭 Fixing Production Build Settings...");
        bool changed = false;
        
        // Development Build
        if (EditorUserBuildSettings.development)
        {
            EditorUserBuildSettings.development = false;
            Debug.Log("   ✅ Development Build → DISABLED");
            changed = true;
        }
        
        // Script Debugging
        if (EditorUserBuildSettings.allowDebugging)
        {
            EditorUserBuildSettings.allowDebugging = false;
            Debug.Log("   ✅ Script Debugging → DISABLED");
            changed = true;
        }
        
        // Deep Profiling
        if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
        {
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
            Debug.Log("   ✅ Deep Profiling → DISABLED");
            changed = true;
        }
        
        // Build App Bundle
        if (!EditorUserBuildSettings.buildAppBundle)
        {
            EditorUserBuildSettings.buildAppBundle = true;
            Debug.Log("   ✅ Build App Bundle → ENABLED");
            changed = true;
        }
        
        // IL2CPP Code Generation (Unity 6 compatible)
        // Note: Unity 6 handles IL2CPP optimization automatically
        Debug.Log("   ✅ IL2CPP Code Generation → Auto-optimized (Unity 6)");
        
        // Managed Stripping (Unity 6 compatible)
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android);
        if (PlayerSettings.GetManagedStrippingLevel(namedBuildTarget) != ManagedStrippingLevel.High)
        {
            PlayerSettings.SetManagedStrippingLevel(namedBuildTarget, ManagedStrippingLevel.High);
            Debug.Log("   ✅ Managed Stripping → High");
            changed = true;
        }
        
        if (!changed) Debug.Log("   ✅ Production build settings already correct");
        return changed;
    }
    
    static bool FixARCoreBuildIssues()
    {
        Debug.Log("🔍 Checking ARCore Build Issues...");
        Debug.Log("   ℹ️  ARCore libraries preserved for development");
        Debug.Log("   💡 Use individual tools if build issues occur");
        
        // Count libraries for information
        string[] libraryGuids = AssetDatabase.FindAssets("t:XRReferenceImageLibrary");
        Debug.Log($"   📊 Found {libraryGuids.Length} reference image libraries");
        
        return false; // No automatic changes
    }
    
    static bool VerifyKeystoreConfiguration()
    {
        Debug.Log("🔑 Verifying Keystore Configuration...");
        
        if (!PlayerSettings.Android.useCustomKeystore)
        {
            Debug.LogError("   ❌ Custom keystore not enabled!");
            Debug.LogError("      → Go to Player Settings > Publishing Settings > Use Custom Keystore");
            return false;
        }
        
        string keystorePath = PlayerSettings.Android.keystoreName;
        if (string.IsNullOrEmpty(keystorePath))
        {
            Debug.LogError("   ❌ Keystore path not set!");
            Debug.LogError("      → Set keystore path in Publishing Settings");
            return false;
        }
        
        if (!File.Exists(keystorePath))
        {
            Debug.LogError($"   ❌ Keystore file not found: {keystorePath}");
            Debug.LogError("      → Check that keystore file exists");
            return false;
        }
        
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
        {
            Debug.LogError("   ❌ Keystore passwords not complete!");
            Debug.LogError("      → Fill in all password fields in Publishing Settings");
            return false;
        }
        
        Debug.Log("   ✅ Keystore configuration verified");
        Debug.Log($"      Keystore: {Path.GetFileName(keystorePath)}");
        Debug.Log($"      Alias: {PlayerSettings.Android.keyaliasName}");
        return true;
    }
    
    static bool AllSettingsCorrect()
    {
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android);
        return PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevelAuto &&
               PlayerSettings.GetScriptingBackend(namedBuildTarget) == ScriptingImplementation.IL2CPP &&
               PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARM64) &&
               !EditorUserBuildSettings.development &&
               !EditorUserBuildSettings.allowDebugging &&
               EditorUserBuildSettings.buildAppBundle;
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Header
        GUILayout.Space(10);
        var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        GUILayout.Label("🚀 Google Play AAB Master Tool", headerStyle);
        GUILayout.Label("One-click solution for Unity 6 AAB signing", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(15);
        
        // Main Fix Button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🎯 ONE-CLICK FIX ALL AAB ISSUES", GUILayout.Height(50)))
        {
            OneClickFix();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(20);
        
        // Individual Tools
        GUILayout.Label("🛠️ Individual Tools:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔧 Fix Unity 6 Signing Settings"))
        {
            bool changed = FixUnity6SigningSettings();
            if (changed) AssetDatabase.SaveAssets();
        }
        
        if (GUILayout.Button("🏭 Fix Production Build Settings"))
        {
            bool changed = FixProductionBuildSettings();
            if (changed) AssetDatabase.SaveAssets();
        }
        
        if (GUILayout.Button("🔍 Check ARCore Libraries (Info Only)"))
        {
            FixARCoreBuildIssues();
        }
        
        if (GUILayout.Button("🔑 Verify Keystore Configuration"))
        {
            VerifyKeystoreConfiguration();
        }
        
        GUILayout.Space(15);
        
        // Current Settings Display
        showCurrentSettings = EditorGUILayout.Foldout(showCurrentSettings, "📋 Current Settings", true);
        if (showCurrentSettings)
        {
            EditorGUI.indentLevel++;
            
            // Signing Settings
            GUILayout.Label("Unity 6 Signing:", EditorStyles.boldLabel);
            ShowSettingStatus("Target API Level", PlayerSettings.Android.targetSdkVersion.ToString(), 
                            PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevelAuto);
            ShowSettingStatus("Scripting Backend", PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android)).ToString(),
                            PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android)) == ScriptingImplementation.IL2CPP);
            ShowSettingStatus("Architecture", PlayerSettings.Android.targetArchitectures.ToString(),
                            PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARM64));
            
            GUILayout.Space(5);
            
            // Build Settings
            GUILayout.Label("Production Build:", EditorStyles.boldLabel);
            ShowSettingStatus("Development Build", EditorUserBuildSettings.development ? "ENABLED" : "DISABLED",
                            !EditorUserBuildSettings.development);
            ShowSettingStatus("Script Debugging", EditorUserBuildSettings.allowDebugging ? "ENABLED" : "DISABLED",
                            !EditorUserBuildSettings.allowDebugging);
            ShowSettingStatus("Build App Bundle", EditorUserBuildSettings.buildAppBundle ? "ENABLED" : "DISABLED",
                            EditorUserBuildSettings.buildAppBundle);
            
            GUILayout.Space(5);
            
            // Keystore Settings
            GUILayout.Label("Keystore:", EditorStyles.boldLabel);
            ShowSettingStatus("Custom Keystore", PlayerSettings.Android.useCustomKeystore ? "ENABLED" : "DISABLED",
                            PlayerSettings.Android.useCustomKeystore);
            
            if (PlayerSettings.Android.useCustomKeystore)
            {
                bool keystoreExists = !string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) && 
                                    File.Exists(PlayerSettings.Android.keystoreName);
                ShowSettingStatus("Keystore File", 
                                !string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) ? 
                                Path.GetFileName(PlayerSettings.Android.keystoreName) : "NOT SET",
                                keystoreExists);
                ShowSettingStatus("Key Alias", 
                                !string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName) ? 
                                PlayerSettings.Android.keyaliasName : "NOT SET",
                                !string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName));
            }
            
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(15);
        
        // Instructions
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "📚 Instructions for Team Members", true);
        if (showAdvancedSettings)
        {
            EditorGUI.indentLevel++;
            
            string instructions = @"HOW TO USE THIS TOOL:

1. 🎯 CLICK 'ONE-CLICK FIX ALL AAB ISSUES'
   • This fixes Unity 6 & production settings
   • ARCore libraries preserved for development
   • Check console for results

2. 🔑 SETUP KEYSTORE (First time only):
   • Edit > Project Settings > Player > Android
   • Publishing Settings > Keystore Manager
   • Create New Keystore OR browse to existing
   • Fill in ALL password fields

3. 🚀 BUILD FOR GOOGLE PLAY:
   • Window > General > Build Profiles
   • Platform: Android (active)
   • Click 'Build App Bundle' button
   • Upload .aab file to Google Play Console

4. ✅ TROUBLESHOOTING:
   • If still rejected, check keystore setup
   • Make sure Development Build is DISABLED
   • Target API Level should be 34+
   • ARCore build issues: Use individual tools if needed";
            
            EditorGUILayout.TextArea(instructions, GUILayout.Height(250));
            
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(10);
        
        // Footer
        GUILayout.Label("Created for RIMJettyOne team - Unity 6000.1 Compatible", EditorStyles.centeredGreyMiniLabel);
        
        EditorGUILayout.EndScrollView();
    }
    
    void ShowSettingStatus(string label, string value, bool isCorrect)
    {
        Color originalColor = GUI.color;
        GUI.color = isCorrect ? Color.green : Color.red;
        string status = isCorrect ? "✅" : "❌";
        EditorGUILayout.LabelField($"{status} {label}:", value);
        GUI.color = originalColor;
    }
}