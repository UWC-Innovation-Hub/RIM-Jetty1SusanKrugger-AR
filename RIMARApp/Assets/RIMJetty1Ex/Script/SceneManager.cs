using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    [Header("Scene Management")]
    [Tooltip("Name of the scene to load (must match exactly with scene name in Build Settings)")]
    public string sceneToLoad;
    
    [Header("Optional Settings")]
    [Tooltip("Delay before loading scene (in seconds)")]
    public float loadDelay = 0f;
    
    [Tooltip("Enable this to load scene additively")]
    public bool loadAdditively = false;

    private void Start()
    {
        // Validate scene name on start
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning($"Scene name is empty on {gameObject.name}. Please specify a scene to load.");
        }
    }

    /// <summary>
    /// Load the specified scene. Call this method from buttons or other UI elements.
    /// </summary>
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("No scene specified to load!");
            return;
        }

        if (loadDelay > 0f)
        {
            Invoke(nameof(LoadSceneDelayed), loadDelay);
        }
        else
        {
            LoadSceneDelayed();
        }
    }

    /// <summary>
    /// Load scene with specified name (can be called from UnityEvents)
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty!");
            return;
        }

        sceneToLoad = sceneName;
        LoadScene();
    }

    private void LoadSceneDelayed()
    {
        try
        {
            if (loadAdditively)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
                Debug.Log($"Loading scene '{sceneToLoad}' additively...");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
                Debug.Log($"Loading scene '{sceneToLoad}'...");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{sceneToLoad}': {e.Message}");
        }
    }

    /// <summary>
    /// Reload the current scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
        Debug.Log($"Reloading current scene: {currentScene.name}");
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Load scene by build index
    /// </summary>
    /// <param name="sceneIndex">Build index of the scene</param>
    public void LoadSceneByIndex(int sceneIndex)
    {
        try
        {
            if (loadAdditively)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
            }
            Debug.Log($"Loading scene at index {sceneIndex}...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene at index {sceneIndex}: {e.Message}");
        }
    }

    // Click detection for 3D objects
    private void OnMouseDown()
    {
        LoadScene();
    }
}