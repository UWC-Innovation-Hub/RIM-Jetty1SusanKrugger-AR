using UnityEngine;

public class ClickDiagnostic : MonoBehaviour
{
    private void Update()
    {
        // Test clicks in editor
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("=== CLICK DIAGNOSTIC ===");
            
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("❌ No Main Camera found!");
                return;
            }
            
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.Log($"✓ Camera: {cam.name}");
            Debug.Log($"✓ Ray Origin: {ray.origin}");
            Debug.Log($"✓ Ray Direction: {ray.direction}");
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                Debug.Log($"✓ HIT OBJECT: {hit.collider.gameObject.name}");
                Debug.Log($"  - Position: {hit.point}");
                Debug.Log($"  - Distance: {hit.distance}");
                Debug.Log($"  - Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                
                ClickableCube clickable = hit.collider.GetComponent<ClickableCube>();
                if (clickable != null)
                {
                    Debug.Log($"✓ Has ClickableCube component!");
                }
                else
                {
                    Debug.LogWarning($"❌ NO ClickableCube component on {hit.collider.gameObject.name}");
                }
                
                BoxCollider collider = hit.collider.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    Debug.Log($"✓ BoxCollider enabled: {collider.enabled}");
                }
            }
            else
            {
                Debug.LogWarning("❌ Raycast hit nothing!");
                Debug.Log("Check:");
                Debug.Log("  1. Is GridRoot visible in Game view?");
                Debug.Log("  2. Are cubes in front of camera?");
                Debug.Log("  3. Do cubes have colliders?");
            }
            
            Debug.Log("======================");
        }
    }
}
