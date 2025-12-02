using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InputSystemObjectRecognitionSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private GameObject objectToTrack; // Your physical object prefab/reference
    [SerializeField] private GameObject contentToAnchor; // Content to show when object is detected
    
    [Header("Image Capture for Reference Library")]
    [SerializeField] private bool captureMode = false;
    [SerializeField] private string captureBaseName = "ReferenceImage";
    [SerializeField] private int captureCount = 0;
    
    [Header("Generated Assets")]
    [SerializeField] private XRReferenceImageLibrary generatedLibrary;
    [SerializeField] private SimpleObjectRecognition recognitionComponent;
    
    [Header("Input System Controls")]
    [SerializeField] private InputAction captureAction;
    [SerializeField] private InputAction toggleModeAction;
    
    [Header("Instructions")]
    [TextArea(5, 10)]
    [SerializeField] private string setupInstructions = @"
SIMPLE OBJECT RECOGNITION SETUP (Input System):

1. CAPTURE REFERENCE IMAGES:
   - Enable 'Capture Mode'
   - Point camera at your object
   - Press SPACE to capture multiple angles
   - Capture 3-5 different views (front, sides, top)

2. CREATE REFERENCE LIBRARY:
   - Click 'Create Reference Library'
   - Images will be automatically added

3. ASSIGN CONTENT:
   - Drag your TrackableObject prefab to 'Content To Anchor'
   - Or create custom augmentation content

4. BUILD AND TEST:
   - Build to device (Android/iOS)
   - Point camera at real object
   - Virtual content appears!
";
    
    void Awake()
    {
        // Initialize Input Actions
        if (captureAction == null)
        {
            captureAction = new InputAction("Capture", InputActionType.Button, "<Keyboard>/space");
        }
        
        if (toggleModeAction == null)
        {
            toggleModeAction = new InputAction("ToggleMode", InputActionType.Button, "<Keyboard>/c");
        }
    }
    
    void OnEnable()
    {
        captureAction.Enable();
        toggleModeAction.Enable();
        
        captureAction.performed += OnCapturePerformed;
        toggleModeAction.performed += OnToggleModePerformed;
    }
    
    void OnDisable()
    {
        captureAction.performed -= OnCapturePerformed;
        toggleModeAction.performed -= OnToggleModePerformed;
        
        captureAction.Disable();
        toggleModeAction.Disable();
    }
    
    void Start()
    {
        if (autoSetup)
        {
            PerformAutoSetup();
        }
        
        ShowInstructions();
    }
    
    void OnCapturePerformed(InputAction.CallbackContext context)
    {
        if (captureMode)
        {
            CaptureReferenceImage();
        }
    }
    
    void OnToggleModePerformed(InputAction.CallbackContext context)
    {
        ToggleCaptureMode();
    }
    
    void PerformAutoSetup()
    {
        Debug.Log("🔧 Performing automatic object recognition setup...");
        
        // Find or create SimpleObjectRecognition component
        recognitionComponent = GetComponent<SimpleObjectRecognition>();
        if (recognitionComponent == null)
        {
            recognitionComponent = gameObject.AddComponent<SimpleObjectRecognition>();
        }
        
        // Setup AR components if needed
        SetupARComponents();
        
        // Assign content if provided
        if (contentToAnchor != null && recognitionComponent != null)
        {
            // Use reflection to set the content prefab
            var field = recognitionComponent.GetType().GetField("contentPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(recognitionComponent, contentToAnchor);
            }
        }
        
        Debug.Log("✅ Auto setup complete!");
    }
    
    void SetupARComponents()
    {
        // Find XR Origin
        var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogWarning("⚠️ XR Origin not found - ensure you have an AR scene setup");
            return;
        }
        
        // Ensure ARTrackedImageManager exists and is enabled
        var imageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
        if (imageManager == null)
        {
            imageManager = xrOrigin.gameObject.AddComponent<ARTrackedImageManager>();
            Debug.Log("➕ Added ARTrackedImageManager");
        }
        
        imageManager.enabled = true;
        
        // Assign library if we have one
        if (generatedLibrary != null)
        {
            imageManager.referenceLibrary = generatedLibrary;
        }
    }
    
    void CaptureReferenceImage()
    {
        #if UNITY_EDITOR
        Debug.Log($"📸 Capturing reference image {captureCount + 1}");
        
        // In editor, we can't capture real camera, so we'll guide user to capture manually
        string imagePath = $"Assets/RIMJetty1Ex/Reference Images/{captureBaseName}_{captureCount:D2}.png";
        
        // Create directory if it doesn't exist
        string directory = Path.GetDirectoryName(imagePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        Debug.Log($"💡 Please manually save a photo of your object as: {imagePath}");
        Debug.Log("   Then refresh the Project window and continue setup");
        
        captureCount++;
        #else
        // On device, you could implement actual camera capture
        CaptureDeviceCamera();
        #endif
    }
    
    void CaptureDeviceCamera()
    {
        // On device implementation for capturing camera frames
        var arCamera = Camera.main;
        if (arCamera == null) return;
        
        // Create render texture to capture camera view
        RenderTexture renderTexture = new RenderTexture(1024, 1024, 24);
        arCamera.targetTexture = renderTexture;
        arCamera.Render();
        
        // Convert to Texture2D
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0);
        screenshot.Apply();
        
        // Save to persistent data path
        byte[] data = screenshot.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, $"{captureBaseName}_{captureCount:D2}.png");
        File.WriteAllBytes(filePath, data);
        
        Debug.Log($"📸 Captured image: {filePath}");
        
        // Clean up
        arCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        Destroy(screenshot);
        
        captureCount++;
    }
    
    void ToggleCaptureMode()
    {
        captureMode = !captureMode;
        Debug.Log($"📷 Capture mode: {(captureMode ? "ON" : "OFF")}");
        
        if (captureMode)
        {
            Debug.Log("📋 Instructions:");
            Debug.Log("   - Point camera at your object");
            Debug.Log("   - Press SPACE to capture");
            Debug.Log("   - Capture 3-5 different angles");
            Debug.Log("   - Press C again to exit capture mode");
        }
    }
    
    [ContextMenu("Create Reference Library")]
    public void CreateReferenceLibrary()
    {
        #if UNITY_EDITOR
        CreateReferenceImageLibrary();
        #endif
    }
    
    #if UNITY_EDITOR
    void CreateReferenceImageLibrary()
    {
        // Create reference image library
        string libraryPath = "Assets/RIMJetty1Ex/Reference Images/SimpleObjectLibrary.asset";
        
        // Create directory if needed
        string directory = Path.GetDirectoryName(libraryPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Create the library
        var library = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
        AssetDatabase.CreateAsset(library, libraryPath);
        
        // Find all PNG files in the reference images directory
        string[] imageGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/RIMJetty1Ex/Reference Images" });
        
        var serializedLibrary = new SerializedObject(library);
        var imagesProperty = serializedLibrary.FindProperty("m_Images");
        
        foreach (string guid in imageGuids)
        {
            string imagePath = AssetDatabase.GUIDToAssetPath(guid);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            
            if (texture != null && imagePath.EndsWith(".png"))
            {
                // Add to library
                imagesProperty.arraySize++;
                var newEntry = imagesProperty.GetArrayElementAtIndex(imagesProperty.arraySize - 1);
                
                var textureProperty = newEntry.FindPropertyRelative("m_Texture");
                var nameProperty = newEntry.FindPropertyRelative("m_Name");
                var specifyWidthProperty = newEntry.FindPropertyRelative("m_SpecifySize");
                var widthProperty = newEntry.FindPropertyRelative("m_Width");
                
                textureProperty.objectReferenceValue = texture;
                nameProperty.stringValue = texture.name;
                specifyWidthProperty.boolValue = true;
                widthProperty.floatValue = 0.1f; // 10cm reference size - adjust as needed
                
                Debug.Log($"➕ Added reference image: {texture.name}");
            }
        }
        
        serializedLibrary.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        generatedLibrary = library;
        
        // Auto-assign to ARTrackedImageManager
        var imageManager = FindFirstObjectByType<ARTrackedImageManager>();
        if (imageManager != null)
        {
            imageManager.referenceLibrary = library;
        }
        
        // Auto-assign to SimpleObjectRecognition
        if (recognitionComponent != null)
        {
            var field = recognitionComponent.GetType().GetField("imageLibrary", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(recognitionComponent, library);
            }
        }
        
        Debug.Log($"✅ Reference library created: {libraryPath}");
        Debug.Log($"📚 Images added: {imagesProperty.arraySize}");
    }
    #endif
    
    void ShowInstructions()
    {
        if (Application.isEditor)
        {
            Debug.Log("🎯 SIMPLE OBJECT RECOGNITION SETUP (Input System)");
            Debug.Log("================================================");
            Debug.Log(setupInstructions);
            Debug.Log("\n🎮 CONTROLS:");
            Debug.Log("   C = Toggle capture mode");
            Debug.Log("   SPACE = Capture reference image (in capture mode)");
        }
    }
    
    [ContextMenu("Reset Setup")]
    public void ResetSetup()
    {
        captureCount = 0;
        captureMode = false;
        Debug.Log("🔄 Setup reset");
    }
}