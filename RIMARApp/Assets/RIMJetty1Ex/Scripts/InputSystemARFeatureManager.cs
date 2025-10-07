using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;

public class InputSystemARFeatureManager : MonoBehaviour
{
    [Header("AR Managers")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARTrackedObjectManager trackedObjectManager;
    [SerializeField] private ObjectTrackingManager customObjectManager;
    
    [Header("Current Mode")]
    [SerializeField] private ARMode currentMode = ARMode.PlaneDetection;
    
    [Header("Input System Actions")]
    [SerializeField] private InputAction planeModeAction;
    [SerializeField] private InputAction objectModeAction;
    [SerializeField] private InputAction hybridModeAction;
    [SerializeField] private InputAction disableAction;
    
    [Header("Auto-Switch Settings")]
    [SerializeField] private bool autoSwitchOnDetection = true;
    [SerializeField] private float autoSwitchDelay = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    public enum ARMode
    {
        PlaneDetection,
        ObjectTracking,
        HybridMode,
        Disabled
    }
    
    public ARMode CurrentMode => currentMode;
    
    // Events
    public System.Action<ARMode> OnModeChanged;
    
    void Awake()
    {
        InitializeInputActions();
    }
    
    void InitializeInputActions()
    {
        // Initialize input actions if not set in inspector
        if (planeModeAction == null)
        {
            planeModeAction = new InputAction("PlaneMode", InputActionType.Button, "<Keyboard>/1");
        }
        
        if (objectModeAction == null)
        {
            objectModeAction = new InputAction("ObjectMode", InputActionType.Button, "<Keyboard>/2");
        }
        
        if (hybridModeAction == null)
        {
            hybridModeAction = new InputAction("HybridMode", InputActionType.Button, "<Keyboard>/3");
        }
        
        if (disableAction == null)
        {
            disableAction = new InputAction("DisableMode", InputActionType.Button, "<Keyboard>/4");
        }
    }
    
    void OnEnable()
    {
        // Enable input actions
        planeModeAction.Enable();
        objectModeAction.Enable();
        hybridModeAction.Enable();
        disableAction.Enable();
        
        // Subscribe to input events
        planeModeAction.performed += OnPlaneModePressed;
        objectModeAction.performed += OnObjectModePressed;
        hybridModeAction.performed += OnHybridModePressed;
        disableAction.performed += OnDisableModePressed;
    }
    
    void OnDisable()
    {
        // Unsubscribe from input events
        planeModeAction.performed -= OnPlaneModePressed;
        objectModeAction.performed -= OnObjectModePressed;
        hybridModeAction.performed -= OnHybridModePressed;
        disableAction.performed -= OnDisableModePressed;
        
        // Disable input actions
        planeModeAction.Disable();
        objectModeAction.Disable();
        hybridModeAction.Disable();
        disableAction.Disable();
    }
    
    void Start()
    {
        FindARComponents();
        SetMode(currentMode);
        
        if (showDebugInfo)
        {
            Debug.Log("🎮 AR Feature Manager initialized with Input System");
            Debug.Log("🔘 Controls: 1=Plane, 2=Object, 3=Hybrid, 4=Disable");
        }
    }
    
    void FindARComponents()
    {
        // Auto-find AR components if not assigned
        if (planeManager == null)
            planeManager = FindFirstObjectByType<ARPlaneManager>();
        
        if (raycastManager == null)
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
        
        if (trackedObjectManager == null)
            trackedObjectManager = FindFirstObjectByType<ARTrackedObjectManager>();
        
        if (customObjectManager == null)
            customObjectManager = FindFirstObjectByType<ObjectTrackingManager>();
    }
    
    void OnPlaneModePressed(InputAction.CallbackContext context)
    {
        EnablePlaneDetectionMode();
    }
    
    void OnObjectModePressed(InputAction.CallbackContext context)
    {
        EnableObjectTrackingMode();
    }
    
    void OnHybridModePressed(InputAction.CallbackContext context)
    {
        EnableHybridMode();
    }
    
    void OnDisableModePressed(InputAction.CallbackContext context)
    {
        DisableAllFeatures();
    }
    
    public void EnablePlaneDetectionMode()
    {
        SetMode(ARMode.PlaneDetection);
    }
    
    public void EnableObjectTrackingMode()
    {
        SetMode(ARMode.ObjectTracking);
    }
    
    public void EnableHybridMode()
    {
        SetMode(ARMode.HybridMode);
    }
    
    public void DisableAllFeatures()
    {
        SetMode(ARMode.Disabled);
    }
    
    void SetMode(ARMode mode)
    {
        if (currentMode == mode) return;
        
        ARMode previousMode = currentMode;
        currentMode = mode;
        
        ApplyModeSettings();
        
        OnModeChanged?.Invoke(currentMode);
        
        if (showDebugInfo)
        {
            Debug.Log($"🔄 AR Mode changed: {previousMode} → {currentMode}");
        }
    }
    
    void ApplyModeSettings()
    {
        switch (currentMode)
        {
            case ARMode.PlaneDetection:
                EnablePlaneDetection();
                DisableObjectTracking();
                break;
                
            case ARMode.ObjectTracking:
                DisablePlaneDetection();
                EnableObjectTracking();
                break;
                
            case ARMode.HybridMode:
                EnablePlaneDetection();
                EnableObjectTracking();
                break;
                
            case ARMode.Disabled:
                DisablePlaneDetection();
                DisableObjectTracking();
                break;
        }
    }
    
    void EnablePlaneDetection()
    {
        if (planeManager != null)
        {
            planeManager.enabled = true;
            if (showDebugInfo) Debug.Log("✅ Plane detection enabled");
        }
        
        if (raycastManager != null)
        {
            raycastManager.enabled = true;
            if (showDebugInfo) Debug.Log("✅ Raycast manager enabled");
        }
    }
    
    void DisablePlaneDetection()
    {
        if (planeManager != null)
        {
            planeManager.enabled = false;
            if (showDebugInfo) Debug.Log("⏸️ Plane detection disabled");
        }
        
        if (raycastManager != null)
        {
            raycastManager.enabled = false;
            if (showDebugInfo) Debug.Log("⏸️ Raycast manager disabled");
        }
    }
    
    void EnableObjectTracking()
    {
        if (trackedObjectManager != null)
        {
            trackedObjectManager.enabled = true;
            if (showDebugInfo) Debug.Log("✅ Object tracking enabled");
        }
        
        if (customObjectManager != null)
        {
            customObjectManager.enabled = true;
            if (showDebugInfo) Debug.Log("✅ Custom object manager enabled");
        }
    }
    
    void DisableObjectTracking()
    {
        if (trackedObjectManager != null)
        {
            trackedObjectManager.enabled = false;
            if (showDebugInfo) Debug.Log("⏸️ Object tracking disabled");
        }
        
        if (customObjectManager != null)
        {
            customObjectManager.enabled = false;
            if (showDebugInfo) Debug.Log("⏸️ Custom object manager disabled");
        }
    }
    
    public void ToggleAutoSwitch()
    {
        autoSwitchOnDetection = !autoSwitchOnDetection;
        if (showDebugInfo)
        {
            Debug.Log($"🔄 Auto-switch: {(autoSwitchOnDetection ? "ON" : "OFF")}");
        }
    }
    
    public void SetAutoSwitchDelay(float delay)
    {
        autoSwitchDelay = Mathf.Max(0.1f, delay);
    }
    
    // Public methods for UI integration
    public void SetModeByIndex(int modeIndex)
    {
        if (modeIndex >= 0 && modeIndex < System.Enum.GetValues(typeof(ARMode)).Length)
        {
            SetMode((ARMode)modeIndex);
        }
    }
    
    public string GetModeDescription()
    {
        switch (currentMode)
        {
            case ARMode.PlaneDetection:
                return "Detecting horizontal and vertical planes for object placement";
            case ARMode.ObjectTracking:
                return "Tracking 3D objects for augmented content";
            case ARMode.HybridMode:
                return "Both plane detection and object tracking active";
            case ARMode.Disabled:
                return "All AR features disabled";
            default:
                return "Unknown mode";
        }
    }
    
    public bool IsFeatureEnabled(string featureName)
    {
        switch (featureName.ToLower())
        {
            case "plane":
            case "planes":
                return planeManager != null && planeManager.enabled;
            case "object":
            case "objects":
                return trackedObjectManager != null && trackedObjectManager.enabled;
            case "raycast":
                return raycastManager != null && raycastManager.enabled;
            default:
                return false;
        }
    }
    
    void OnDestroy()
    {
        // Clean up input actions
        planeModeAction?.Dispose();
        objectModeAction?.Dispose();
        hybridModeAction?.Dispose();
        disableAction?.Dispose();
    }
}