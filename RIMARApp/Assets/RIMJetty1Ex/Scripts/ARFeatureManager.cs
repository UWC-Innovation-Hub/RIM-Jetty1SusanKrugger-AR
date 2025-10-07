using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ARFeatureManager : MonoBehaviour
{
    [Header("AR Feature Control")]
    [SerializeField] private ARMode currentMode = ARMode.PlaneDetection;
    [SerializeField] private bool autoSwitchOnDetection = true;
    
    [Header("AR Managers")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARTrackedObjectManager objectManager;
    [SerializeField] private ObjectTrackingManager customObjectManager;
    
    [Header("UI Controls (Optional)")]
    [SerializeField] private Button planeDetectionButton;
    [SerializeField] private Button objectTrackingButton;
    [SerializeField] private Button hybridModeButton;
    [SerializeField] private Text currentModeText;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    public enum ARMode
    {
        PlaneDetection,
        ObjectTracking,
        HybridMode,
        Disabled
    }
    
    public ARMode CurrentMode => currentMode;
    
    void Start()
    {
        // Auto-find AR managers if not assigned
        FindARManagers();
        
        // Set up UI if available
        SetupUI();
        
        // Apply initial mode
        SetARMode(currentMode);
        
        if (debugMode)
        {
            Debug.Log($"ARFeatureManager initialized in {currentMode} mode");
        }
    }
    
    void FindARManagers()
    {
        if (planeManager == null)
            planeManager = FindFirstObjectByType<ARPlaneManager>();
            
        if (raycastManager == null)
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
            
        if (objectManager == null)
            objectManager = FindFirstObjectByType<ARTrackedObjectManager>();
            
        if (customObjectManager == null)
            customObjectManager = FindFirstObjectByType<ObjectTrackingManager>();
    }
    
    void SetupUI()
    {
        if (planeDetectionButton != null)
            planeDetectionButton.onClick.AddListener(() => SetARMode(ARMode.PlaneDetection));
            
        if (objectTrackingButton != null)
            objectTrackingButton.onClick.AddListener(() => SetARMode(ARMode.ObjectTracking));
            
        if (hybridModeButton != null)
            hybridModeButton.onClick.AddListener(() => SetARMode(ARMode.HybridMode));
    }
    
    public void SetARMode(ARMode mode)
    {
        currentMode = mode;
        
        switch (mode)
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
        
        UpdateUI();
        
        if (debugMode)
        {
            Debug.Log($"AR Mode switched to: {mode}");
        }
    }
    
    void EnablePlaneDetection()
    {
        if (planeManager != null)
        {
            planeManager.enabled = true;
            // Keep existing planes visible but allow new detection
        }
        
        if (raycastManager != null)
        {
            raycastManager.enabled = true;
        }
    }
    
    void DisablePlaneDetection()
    {
        if (planeManager != null)
        {
            planeManager.enabled = false;
            // Optionally hide existing planes
            // foreach (var plane in planeManager.trackables)
            //     plane.gameObject.SetActive(false);
        }
    }
    
    void EnableObjectTracking()
    {
        if (objectManager != null)
        {
            objectManager.enabled = true;
        }
        
        if (customObjectManager != null)
        {
            customObjectManager.enabled = true;
        }
    }
    
    void DisableObjectTracking()
    {
        if (objectManager != null)
        {
            objectManager.enabled = false;
        }
        
        if (customObjectManager != null)
        {
            customObjectManager.enabled = false;
        }
    }
    
    void UpdateUI()
    {
        if (currentModeText != null)
        {
            currentModeText.text = $"AR Mode: {currentMode}";
        }
        
        // Update button states
        UpdateButtonState(planeDetectionButton, currentMode == ARMode.PlaneDetection);
        UpdateButtonState(objectTrackingButton, currentMode == ARMode.ObjectTracking);
        UpdateButtonState(hybridModeButton, currentMode == ARMode.HybridMode);
    }
    
    void UpdateButtonState(Button button, bool isActive)
    {
        if (button == null) return;
        
        var colors = button.colors;
        colors.normalColor = isActive ? Color.green : Color.white;
        button.colors = colors;
    }
    
    // Public methods for external control
    public void EnablePlaneDetectionMode()
    {
        SetARMode(ARMode.PlaneDetection);
    }
    
    public void EnableObjectTrackingMode()
    {
        SetARMode(ARMode.ObjectTracking);
    }
    
    public void EnableHybridMode()
    {
        SetARMode(ARMode.HybridMode);
    }
    
    public void DisableAllTracking()
    {
        SetARMode(ARMode.Disabled);
    }
    
    // Keyboard shortcuts for testing using Input System
    void Update()
    {
        if (debugMode && Application.isEditor && Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                SetARMode(ARMode.PlaneDetection);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
                SetARMode(ARMode.ObjectTracking);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
                SetARMode(ARMode.HybridMode);
            else if (Keyboard.current.digit0Key.wasPressedThisFrame)
                SetARMode(ARMode.Disabled);
        }
    }
    
    // Auto-switching logic
    void OnEnable()
    {
        if (autoSwitchOnDetection && customObjectManager != null)
        {
            // Subscribe to object detection events to auto-switch modes
            StartCoroutine(MonitorForObjectDetection());
        }
    }
    
    IEnumerator MonitorForObjectDetection()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if (autoSwitchOnDetection && currentMode == ARMode.PlaneDetection && 
                customObjectManager != null && customObjectManager.enabled)
            {
                var trackedObjects = customObjectManager.GetCurrentlyTrackedObjects();
                if (trackedObjects.Count > 0)
                {
                    if (debugMode)
                    {
                        Debug.Log("Object detected! Auto-switching to Object Tracking mode");
                    }
                    SetARMode(ARMode.ObjectTracking);
                }
            }
        }
    }
    
    // Get current tracking information
    public string GetTrackingInfo()
    {
        string info = $"Current Mode: {currentMode}\n";
        
        if (planeManager != null && planeManager.enabled)
        {
            info += $"Planes Detected: {planeManager.trackables.count}\n";
        }
        
        if (objectManager != null && objectManager.enabled)
        {
            info += $"Objects Tracked: {objectManager.trackables.count}\n";
        }
        
        return info;
    }
}