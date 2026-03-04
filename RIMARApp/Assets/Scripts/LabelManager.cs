using System.Collections.Generic;
using UnityEngine;


public class LabelManager : MonoBehaviour
{
    public static LabelManager Instance;

    private List<Transform> labels = new List<Transform>();
    private Camera mainCamera;

    [Header("Declutter Settings")]
    [SerializeField] private float screenSpacing;
    [SerializeField] private float pushStrength;
    [SerializeField] private float smoothSpeed;


    private void Awake()
    {
        Instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }


    public void RegisterLabel(Transform label)
    {
        if (!labels.Contains(label)) 
            labels.Add(label);
    }


    private void LateUpdate()
    {
        if (mainCamera == null) return;

        for (int i = 0; i < labels.Count; i++)
        {
            for (int j = i + 1; j < labels.Count;  j++)
            {
                Transform a = labels[i];
                Transform b = labels[j];

                Vector3 screenA = mainCamera.WorldToScreenPoint(a.position);
                Vector3 screenB = mainCamera.WorldToScreenPoint(b.position);

                float distance = Vector2.Distance(screenA, screenB);

                if (distance < screenSpacing)
                {
                    Vector2 direction = (screenA - screenB).normalized;
                    Vector2 push = direction * pushStrength;

                    screenA += (Vector3)push;
                    screenB -= (Vector3)push;

                    Vector3 worldA = mainCamera.ScreenToWorldPoint(
                        new Vector3(screenA.x, screenA.y, screenA.z)
                    );

                    Vector3 worldB = mainCamera.ScreenToWorldPoint(
                        new Vector3(screenB.x, screenB.y, screenB.z)
                    );

                    a.position = Vector3.Lerp(a.position, worldA, Time.deltaTime * smoothSpeed);
                    b.position = Vector3.Lerp(b.position, worldB, Time.deltaTime * smoothSpeed);
                }
            }
        }
    }
}
