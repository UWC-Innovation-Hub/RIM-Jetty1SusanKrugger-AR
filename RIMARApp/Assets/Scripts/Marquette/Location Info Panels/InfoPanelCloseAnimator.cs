using System.Collections;
using UnityEngine;


/*
 * This script handles smooth close animation for info panels. It:
 * 1. Scales it down
 * 2. Fades out
 * 3. Destroys the panel
 */

public class InfoPanelCloseAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float closeDuration = 0.2f;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;


    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null )
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalScale = transform.localScale;
    }


    public void PlayClose(System.Action onComplete = null)
    {
        StartCoroutine(CloseRoutine(onComplete));
    }


    private IEnumerator CloseRoutine(System.Action onComplete)
    {
        float elapsed = 0f;

        Vector3 targetScale = originalScale * 0.85f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);

            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            if (canvasGroup != null ) 
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        onComplete?.Invoke();

        Destroy(gameObject);
    }
}
