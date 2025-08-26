using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class SceneLoader : MonoBehaviour
{
    [Tooltip("The index of the scene as set in the Build Settings.")]
    public int sceneIndex;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Button and attach the click listener
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene(sceneIndex);
        });
    }
}
