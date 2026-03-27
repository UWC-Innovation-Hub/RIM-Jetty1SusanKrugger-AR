using UnityEngine;
using UnityEngine.UI;


/*
 * This script manages how the screenshots would be displayed on the popup panel at the end of the experience
 */

public class ScreenshotPreview : MonoBehaviour
{
    public RawImage previewImage;


    public void SetImage(Texture2D texture)
    {
        previewImage.texture = texture;
        previewImage.SetNativeSize(); // optional but helpful
        previewImage.rectTransform.sizeDelta = new Vector2(640, 360); // adjust for your UI
    }


    public void Close()
    {
        Destroy(gameObject);
    }
}
