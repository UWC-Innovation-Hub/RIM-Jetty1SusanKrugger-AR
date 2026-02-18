using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class SimpleARUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button clearButton;

    [Header("AR References")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private QRCodeAnchor qrCodeAnchor;

    void Start()
    {
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(OnClearButtonClicked);
        }

        UpdateStatusText("Point camera at QR code to begin");
    }

    void Update()
    {
        if (arSession != null && statusText != null)
        {
            string status = arSession.enabled ? "AR Active - Scanning for QR codes..." : "AR Inactive";
            UpdateStatusText(status);
        }
    }

    void OnClearButtonClicked()
    {
        if (qrCodeAnchor != null)
        {
            qrCodeAnchor.ClearAllContent();
            UpdateStatusText("All content cleared");
        }
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
