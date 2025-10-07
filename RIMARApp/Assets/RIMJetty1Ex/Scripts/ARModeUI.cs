using UnityEngine;
using UnityEngine.UI;

public class ARModeUI : MonoBehaviour
{
    [Header("AR Feature Manager")]
    [SerializeField] private ARFeatureManager arFeatureManager;
    
    [Header("Mode Buttons")]
    [SerializeField] private Button planeModeButton;
    [SerializeField] private Button objectModeButton;
    [SerializeField] private Button hybridModeButton;
    
    [Header("Status Display")]
    [SerializeField] private Text statusText;
    [SerializeField] private Image modeIndicator;
    
    [Header("Mode Colors")]
    [SerializeField] private Color planeColor = Color.blue;
    [SerializeField] private Color objectColor = Color.green;
    [SerializeField] private Color hybridColor = Color.yellow;
    [SerializeField] private Color disabledColor = Color.gray;
    
    void Start()
    {
        // Auto-find ARFeatureManager if not assigned
        if (arFeatureManager == null)
            arFeatureManager = FindFirstObjectByType<ARFeatureManager>();
        
        // Set up button listeners
        SetupButtons();
        
        // Update initial UI state
        UpdateUI();
    }
    
    void SetupButtons()
    {
        if (planeModeButton != null)
        {
            planeModeButton.onClick.AddListener(() => {
                arFeatureManager?.EnablePlaneDetectionMode();
                UpdateUI();
            });
        }
        
        if (objectModeButton != null)
        {
            objectModeButton.onClick.AddListener(() => {
                arFeatureManager?.EnableObjectTrackingMode();
                UpdateUI();
            });
        }
        
        if (hybridModeButton != null)
        {
            hybridModeButton.onClick.AddListener(() => {
                arFeatureManager?.EnableHybridMode();
                UpdateUI();
            });
        }
    }
    
    void Update()
    {
        // Update UI periodically to reflect current state
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (arFeatureManager == null) return;
        
        // Update status text
        if (statusText != null)
        {
            statusText.text = GetStatusText();
        }
        
        // Update mode indicator color
        if (modeIndicator != null)
        {
            modeIndicator.color = GetModeColor();
        }
        
        // Update button states
        UpdateButtonVisualState();
    }
    
    string GetStatusText()
    {
        var mode = arFeatureManager.CurrentMode;
        string baseText = $"AR Mode: {mode}";
        
        switch (mode)
        {
            case ARFeatureManager.ARMode.PlaneDetection:
                baseText += "\nTap to place objects on surfaces";
                break;
            case ARFeatureManager.ARMode.ObjectTracking:
                baseText += "\nPoint camera at tracked objects";
                break;
            case ARFeatureManager.ARMode.HybridMode:
                baseText += "\nBoth plane & object tracking active";
                break;
            case ARFeatureManager.ARMode.Disabled:
                baseText += "\nAR tracking disabled";
                break;
        }
        
        return baseText;
    }
    
    Color GetModeColor()
    {
        switch (arFeatureManager.CurrentMode)
        {
            case ARFeatureManager.ARMode.PlaneDetection:
                return planeColor;
            case ARFeatureManager.ARMode.ObjectTracking:
                return objectColor;
            case ARFeatureManager.ARMode.HybridMode:
                return hybridColor;
            default:
                return disabledColor;
        }
    }
    
    void UpdateButtonVisualState()
    {
        var currentMode = arFeatureManager.CurrentMode;
        
        UpdateButtonHighlight(planeModeButton, currentMode == ARFeatureManager.ARMode.PlaneDetection);
        UpdateButtonHighlight(objectModeButton, currentMode == ARFeatureManager.ARMode.ObjectTracking);
        UpdateButtonHighlight(hybridModeButton, currentMode == ARFeatureManager.ARMode.HybridMode);
    }
    
    void UpdateButtonHighlight(Button button, bool isActive)
    {
        if (button == null) return;
        
        var colors = button.colors;
        colors.normalColor = isActive ? Color.green : Color.white;
        colors.selectedColor = isActive ? Color.green : Color.white;
        button.colors = colors;
    }
}