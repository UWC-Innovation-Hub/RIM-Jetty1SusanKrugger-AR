using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectRecognitionUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button captureButton;
    [SerializeField] private Button toggleModeButton;
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Toggle smoothingToggle;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    
    [Header("References")]
    [SerializeField] private SimpleObjectRecognition recognitionSystem;
    [SerializeField] private ObjectRecognitionSetup setupHelper;
    
    [Header("UI Settings")]
    [SerializeField] private bool showInstructions = true;
    [SerializeField] private float statusUpdateInterval = 0.5f;
    
    private float lastStatusUpdate = 0f;
    
    void Start()
    {
        SetupUI();
        FindReferences();
        UpdateInstructions();
    }
    
    void Update()
    {
        UpdateStatusDisplay();
    }
    
    void SetupUI()
    {
        // Setup capture button
        if (captureButton != null)
        {
            captureButton.onClick.AddListener(OnCaptureButtonPressed);
        }
        
        // Setup mode toggle button
        if (toggleModeButton != null)
        {
            toggleModeButton.onClick.AddListener(OnToggleModePressed);
        }
        
        // Setup scale slider
        if (scaleSlider != null)
        {
            scaleSlider.value = 1f;
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);
        }
        
        // Setup smoothing toggle
        if (smoothingToggle != null)
        {
            smoothingToggle.isOn = true;
            smoothingToggle.onValueChanged.AddListener(OnSmoothingToggled);
        }
    }
    
    void FindReferences()
    {
        // Auto-find components if not assigned
        if (recognitionSystem == null)
            recognitionSystem = FindFirstObjectByType<SimpleObjectRecognition>();
        
        if (setupHelper == null)
            setupHelper = FindFirstObjectByType<ObjectRecognitionSetup>();
    }
    
    void UpdateInstructions()
    {
        if (instructionsText != null && showInstructions)
        {
            string instructions = "SIMPLE OBJECT RECOGNITION\n\n";
            instructions += "📱 SETUP STEPS:\n";
            instructions += "1. Capture 3-5 photos of your object\n";
            instructions += "2. Build to device\n";
            instructions += "3. Point camera at real object\n";
            instructions += "4. Virtual content appears!\n\n";
            instructions += "🎮 CONTROLS:\n";
            instructions += "📸 Capture = Take reference photo\n";
            instructions += "🔄 Mode = Switch recognition method\n";
            instructions += "📏 Scale = Adjust content size\n";
            instructions += "🎯 Smooth = Enable smooth tracking\n";
            
            instructionsText.text = instructions;
        }
    }
    
    void UpdateStatusDisplay()
    {
        if (statusText != null && Time.time - lastStatusUpdate > statusUpdateInterval)
        {
            lastStatusUpdate = Time.time;
            
            string status = "STATUS: ";
            
            if (recognitionSystem != null)
            {
                // Get status from recognition system using reflection
                var modeField = recognitionSystem.GetType().GetField("mode", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (modeField != null)
                {
                    var mode = modeField.GetValue(recognitionSystem);
                    status += $"Mode: {mode}\n";
                }
                
                // Check if tracking objects
                var trackedObjectsField = recognitionSystem.GetType().GetField("trackedObjects", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (trackedObjectsField != null)
                {
                    var trackedObjects = trackedObjectsField.GetValue(recognitionSystem) as System.Collections.IDictionary;
                    status += $"Objects Tracked: {trackedObjects?.Count ?? 0}\n";
                }
            }
            else
            {
                status += "Recognition system not found\n";
            }
            
            if (setupHelper != null)
            {
                // Get capture count
                var captureCountField = setupHelper.GetType().GetField("captureCount", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (captureCountField != null)
                {
                    var captureCount = captureCountField.GetValue(setupHelper);
                    status += $"Images Captured: {captureCount}\n";
                }
                
                // Get capture mode status
                var captureModeField = setupHelper.GetType().GetField("captureMode", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (captureModeField != null)
                {
                    var captureMode = captureModeField.GetValue(setupHelper);
                    status += $"Capture Mode: {(bool)captureMode}\n";
                }
            }
            
            status += $"\nFPS: {(1f / Time.deltaTime):F0}";
            
            statusText.text = status;
        }
    }
    
    void OnCaptureButtonPressed()
    {
        if (setupHelper != null)
        {
            // Trigger capture via reflection
            var captureMethod = setupHelper.GetType().GetMethod("CaptureReferenceImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (captureMethod != null)
            {
                captureMethod.Invoke(setupHelper, null);
                ShowFeedback("📸 Image Captured!");
            }
        }
        else
        {
            ShowFeedback("❌ Setup helper not found");
        }
    }
    
    void OnToggleModePressed()
    {
        if (setupHelper != null)
        {
            // Toggle capture mode via reflection
            var captureModeField = setupHelper.GetType().GetField("captureMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (captureModeField != null)
            {
                bool currentMode = (bool)captureModeField.GetValue(setupHelper);
                captureModeField.SetValue(setupHelper, !currentMode);
                
                ShowFeedback($"🔄 Capture Mode: {(!currentMode ? "ON" : "OFF")}");
            }
        }
    }
    
    void OnScaleChanged(float value)
    {
        if (recognitionSystem != null)
        {
            // Set content scale via reflection
            var setScaleMethod = recognitionSystem.GetType().GetMethod("SetContentScale");
            if (setScaleMethod != null)
            {
                setScaleMethod.Invoke(recognitionSystem, new object[] { value });
                ShowFeedback($"📏 Scale: {value:F1}x");
            }
        }
    }
    
    void OnSmoothingToggled(bool enabled)
    {
        if (recognitionSystem != null)
        {
            // Toggle smoothing via reflection
            var toggleMethod = recognitionSystem.GetType().GetMethod("ToggleSmoothing");
            if (toggleMethod != null)
            {
                var smoothingField = recognitionSystem.GetType().GetField("smoothTracking", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (smoothingField != null)
                {
                    smoothingField.SetValue(recognitionSystem, enabled);
                    ShowFeedback($"🎯 Smoothing: {(enabled ? "ON" : "OFF")}");
                }
            }
        }
    }
    
    void ShowFeedback(string message)
    {
        Debug.Log(message);
        
        // You could add visual feedback here, like:
        // - Temporary text overlay
        // - Color flash
        // - Sound effect
        
        StartCoroutine(ShowTemporaryFeedback(message));
    }
    
    System.Collections.IEnumerator ShowTemporaryFeedback(string message)
    {
        if (statusText != null)
        {
            string originalText = statusText.text;
            statusText.text = message;
            statusText.color = Color.green;
            
            yield return new WaitForSeconds(1.5f);
            
            statusText.text = originalText;
            statusText.color = Color.white;
        }
    }
    
    public void CreateReferenceLibrary()
    {
        if (setupHelper != null)
        {
            // Call CreateReferenceLibrary method
            var createMethod = setupHelper.GetType().GetMethod("CreateReferenceLibrary");
            if (createMethod != null)
            {
                createMethod.Invoke(setupHelper, null);
                ShowFeedback("📚 Library Created!");
            }
        }
    }
    
    public void ResetSetup()
    {
        if (setupHelper != null)
        {
            // Call ResetSetup method
            var resetMethod = setupHelper.GetType().GetMethod("ResetSetup");
            if (resetMethod != null)
            {
                resetMethod.Invoke(setupHelper, null);
                ShowFeedback("🔄 Setup Reset!");
            }
        }
    }
}