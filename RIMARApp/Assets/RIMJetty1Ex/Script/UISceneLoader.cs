using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-specific scene loader that works with buttons and other UI elements
/// </summary>
public class UISceneLoader : MonoBehaviour
{
    [Header("UI Scene Loading")]
    [Tooltip("Button that will trigger scene loading")]
    public Button sceneButton;
    
    [Tooltip("Scene name to load when button is clicked")]
    public string targetScene;
    
    [Header("Optional UI Elements")]
    [Tooltip("Text component to display scene name (optional)")]
    public Text buttonText;
    
    [Tooltip("Text to display on button (leave empty to use scene name)")]
    public string customButtonText;

    private SceneManager sceneManager;

    private void Start()
    {
        // Get or create SceneManager component
        sceneManager = GetComponent<SceneManager>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<SceneManager>();
        }

        // Set up the scene manager with our target scene
        sceneManager.sceneToLoad = targetScene;

        // Set up button if provided
        if (sceneButton != null)
        {
            sceneButton.onClick.AddListener(LoadTargetScene);
            
            // Update button text if text component is provided
            if (buttonText != null)
            {
                string displayText = string.IsNullOrEmpty(customButtonText) ? targetScene : customButtonText;
                buttonText.text = displayText;
            }
        }
        else
        {
            Debug.LogWarning($"No button assigned to UISceneLoader on {gameObject.name}");
        }
    }

    /// <summary>
    /// Load the target scene
    /// </summary>
    public void LoadTargetScene()
    {
        if (sceneManager != null)
        {
            sceneManager.LoadScene();
        }
        else
        {
            Debug.LogError("SceneManager component not found!");
        }
    }

    /// <summary>
    /// Update the target scene at runtime
    /// </summary>
    /// <param name="newSceneName">New scene to target</param>
    public void SetTargetScene(string newSceneName)
    {
        targetScene = newSceneName;
        if (sceneManager != null)
        {
            sceneManager.sceneToLoad = newSceneName;
        }

        // Update button text if available
        if (buttonText != null && string.IsNullOrEmpty(customButtonText))
        {
            buttonText.text = newSceneName;
        }
    }
}