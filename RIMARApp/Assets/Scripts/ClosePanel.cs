using UnityEngine;

public class ClosePanel : MonoBehaviour
{
    public void ClosePanelButton()
    {
        InfoPanelController.Instance.HidePanel();
    }
}