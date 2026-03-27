using UnityEngine;
using TMPro;


/*
 * This script controls the UI visibility and instructional text flow
 */

public class UIFlowManager : MonoBehaviour
{
    public static UIFlowManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject mainUIGroup;
    [SerializeField] private TextMeshProUGUI instructionText;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        // Initial state
        ShowInitialInstruction();
    }


    // Step 1: App launch
    public void ShowInitialInstruction()
    {
        if (mainUIGroup != null)
            mainUIGroup.SetActive(false);

        SetInstruction("Scan the QR code to begin.");
    }


    // Step 2: After QR scan
    public void OnQRCodeScanned()
    {
        if (mainUIGroup != null)
            mainUIGroup.SetActive(true);

        SetInstruction("Tap once on the location marker to view the intel, double tap to collect the intel.");
    }


    // Change the step according to the step in the experience
    public void SetInstruction(string message)
    {
        if (instructionText != null)
            instructionText.text = message;
    }
}
