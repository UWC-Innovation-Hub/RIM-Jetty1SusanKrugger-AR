using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class ClickableCube : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.05f, 0);
    [SerializeField] private bool spawnOnlyOnce = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.green;
    
    [Header("Spawn Options")]
    [SerializeField] private bool destroyOnNextClick = false;
    [SerializeField] private float spawnScale = 1f;
    
    private MeshRenderer meshRenderer;
    private Material cubeMaterial;
    private GameObject spawnedObject;
    private bool hasSpawned = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer != null)
        {
            cubeMaterial = meshRenderer.material;
            cubeMaterial.color = normalColor;
        }
        
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
    }

    private void OnMouseDown()
    {
        OnCubeClicked();
    }

    public void OnCubeClicked()
    {
        if (spawnOnlyOnce && hasSpawned)
        {
            if (destroyOnNextClick && spawnedObject != null)
            {
                Destroy(spawnedObject);
                spawnedObject = null;
                hasSpawned = false;
                
                if (cubeMaterial != null)
                {
                    cubeMaterial.color = normalColor;
                }
                
                Debug.Log($"Destroyed object on cube: {gameObject.name}");
            }
            else
            {
                Debug.Log($"Cube {gameObject.name} already spawned an object");
            }
            return;
        }

        SpawnObject();
    }

    private void SpawnObject()
    {
        if (objectToSpawn == null)
        {
            Debug.LogWarning($"No object assigned to spawn on cube: {gameObject.name}");
            CreateDefaultObject();
            return;
        }

        Vector3 spawnPosition = transform.position + spawnOffset;
        
        spawnedObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        
        // Only apply spawnScale if it's different from 1, otherwise keep prefab's scale
        if (spawnScale != 1f)
        {
            spawnedObject.transform.localScale = objectToSpawn.transform.localScale * spawnScale;
        }
        
        spawnedObject.name = $"{objectToSpawn.name}_SpawnedBy_{gameObject.name}";
        
        hasSpawned = true;
        
        if (cubeMaterial != null)
        {
            cubeMaterial.color = selectedColor;
        }
        
        Debug.Log($"Spawned {spawnedObject.name} at position {spawnPosition}");
    }

    private void CreateDefaultObject()
    {
        GameObject defaultSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        defaultSphere.transform.position = transform.position + spawnOffset;
        defaultSphere.transform.localScale = Vector3.one * 0.03f * spawnScale;
        defaultSphere.name = $"DefaultSphere_SpawnedBy_{gameObject.name}";
        
        MeshRenderer renderer = defaultSphere.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
        }
        
        spawnedObject = defaultSphere;
        hasSpawned = true;
        
        if (cubeMaterial != null)
        {
            cubeMaterial.color = selectedColor;
        }
        
        Debug.Log($"Spawned default sphere on cube: {gameObject.name}");
    }

    public void SetObjectToSpawn(GameObject obj)
    {
        objectToSpawn = obj;
    }

    public void ResetCube()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
        
        hasSpawned = false;
        
        if (cubeMaterial != null)
        {
            cubeMaterial.color = normalColor;
        }
    }

    public GameObject GetSpawnedObject()
    {
        return spawnedObject;
    }

    public bool HasSpawned()
    {
        return hasSpawned;
    }

    private void OnMouseEnter()
    {
        if (cubeMaterial != null && !hasSpawned)
        {
            cubeMaterial.color = highlightColor;
        }
    }

    private void OnMouseExit()
    {
        if (cubeMaterial != null && !hasSpawned)
        {
            cubeMaterial.color = normalColor;
        }
    }
}
