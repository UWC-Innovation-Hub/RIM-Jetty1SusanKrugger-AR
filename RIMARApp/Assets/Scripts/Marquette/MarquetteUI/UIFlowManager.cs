using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


/*
 * This script controls;
 * 1. Welcome screen
 * 2. Token intro transition
 * 3. Scan prompt screem
 * 4. Gameplay UI visibility
 * 5. Instruction text flow
 * 6. Fade transitions
 */

public class UIFlowManager : MonoBehaviour
{
    public static UIFlowManager Instance;

    [Header("UI Groups")]
    [SerializeField] private GameObject startScreenGroup;
    [SerializeField] private GameObject commonInstructionGroup;
    [SerializeField] private GameObject gameplayUIGroup;

    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup startScreenCanvasGroup;
    [SerializeField] private CanvasGroup instructionCanvasGroup;

    [Header("Instruction Text")]
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Token Transition")]
    [SerializeField] private RectTransform animatedToken;
    [SerializeField] private Image animatedTokenImage;
    [SerializeField] private RectTransform tokenStartAnchor;
    [SerializeField] private RectTransform tokenTargetAnchor;
    [SerializeField] private GameObject disabledTokenIcon;

    [Header("Animation Settings")]
    [SerializeField] private float tokenMoveDuration = 0.8f;
    [SerializeField] private float welcomeFadeDuration = 0.35f;
    [SerializeField] private float scanPromptFadeDuration = 0.35f;
    [SerializeField] private float tokenStartScale = 1f;
    [SerializeField] private float tokenEndScale = 0.35f;
    [SerializeField] private AnimationCurve tokenMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isTransitioning = false;
    private bool hasEnteredScanPrompt = false;
    private bool gameplayStarted = false;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        // Initial state
        ShowWelcomeScreen();
    }


    // -----------------------------------
    // STATE 1: Welcome screen
    // -----------------------------------
    public void ShowWelcomeScreen()
    {
        isTransitioning = false;
        hasEnteredScanPrompt = false;
        gameplayStarted = false;

        if (startScreenGroup != null)
            startScreenGroup.SetActive(true);

        if (commonInstructionGroup != null)
            commonInstructionGroup.SetActive(false);

        if (gameplayUIGroup != null)
            gameplayUIGroup.SetActive(false);

        if (disabledTokenIcon != null)
            disabledTokenIcon.SetActive(false);

        if (startScreenCanvasGroup != null)
        {
            startScreenCanvasGroup.alpha = 1f;
            startScreenCanvasGroup.interactable = true;
            startScreenCanvasGroup.blocksRaycasts = true;
        }

        if (instructionCanvasGroup != null)
        {
            instructionCanvasGroup.alpha = 0f;
            instructionCanvasGroup.interactable = false;
            instructionCanvasGroup.blocksRaycasts = false;
        }

        if (animatedToken != null && tokenStartAnchor != null)
        {
            animatedToken.gameObject.SetActive(true);
            animatedToken.position = tokenStartAnchor.position;
            animatedToken.localScale = Vector3.one * tokenStartScale;
        }

        if (animatedTokenImage != null)
        {
            Color c = animatedTokenImage.color;
            c.a = 1f;
            animatedTokenImage.color = c;
        }
    }


    // -----------------------------------
    // STATE 2: Start button pressed
    // -----------------------------------
    public void OnStartButtonPressed()
    {
        if (isTransitioning || hasEnteredScanPrompt)
            return;

        StartCoroutine(PlayIntroTransition());
    }


    private IEnumerator PlayIntroTransition()
    {
        isTransitioning = true;

        if (disabledTokenIcon != null)
            disabledTokenIcon.SetActive(false);

        if (commonInstructionGroup != null)
            commonInstructionGroup.SetActive(true);

        if (instructionCanvasGroup != null)
        {
            instructionCanvasGroup.alpha = 0f;
            instructionCanvasGroup.interactable = false;
            instructionCanvasGroup.blocksRaycasts = false;
        }

        SetInstruction("Scan the QR code to begin.");

        // Fade out the welcome screen while token begins moving
        Coroutine fadeWelcome = StartCoroutine(FadeCanvasGroup(startScreenCanvasGroup, 1f, 0f, welcomeFadeDuration));

        // Play token move + scale + fade
        yield return StartCoroutine(AnimateTokenToTarget());

        if (fadeWelcome != null)
            yield return fadeWelcome;

        if (startScreenGroup  != null)
            startScreenGroup.SetActive(false);

        if (disabledTokenIcon != null)
            disabledTokenIcon.SetActive(false);

        // Fade in scan prompt/instruction text
        yield return StartCoroutine(FadeCanvasGroup(instructionCanvasGroup, 0f, 1f, scanPromptFadeDuration));

        if (instructionCanvasGroup != null)
        {
            instructionCanvasGroup.interactable = true;
            instructionCanvasGroup.blocksRaycasts = true;
        }

        hasEnteredScanPrompt = true;
        isTransitioning = false;
    }


    private IEnumerator AnimateTokenToTarget()
    {
        if (animatedToken == null || tokenStartAnchor == null || tokenTargetAnchor == null)
        {
            Debug.LogError("UIFlowManager: Token transition references are missing.");
            yield break;
        }

        animatedToken.gameObject.SetActive(true);
        animatedToken.position = tokenStartAnchor.position;
        animatedToken.localScale = Vector3.one * tokenStartScale;

        if (animatedToken != null)
        {
            Color startColor = animatedTokenImage.color;
            startColor.a = 1f;
            animatedTokenImage.color = startColor;
        }

        Vector3 startPos = tokenStartAnchor.position;
        Vector3 endPos = tokenTargetAnchor.position;

        float elapsed = 0f;

        while (elapsed < tokenMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tokenMoveDuration);
            float curvedT = tokenMoveCurve.Evaluate(t);

            animatedToken.position = Vector3.Lerp(startPos, endPos, curvedT);   

            float scale = Mathf.Lerp(tokenStartScale, tokenEndScale, curvedT);
            animatedToken.localScale = Vector3.one * scale;

            if (animatedTokenImage != null)
            {
                Color c = animatedTokenImage.color;
                c.a = Mathf.Lerp(1f, 0f, curvedT);
                animatedTokenImage.color = c;
            }

            yield return null;
        }

        animatedToken.position = endPos;
        animatedToken.localScale = Vector3.one * tokenEndScale;

        if (animatedTokenImage != null)
        {
            Color c = animatedTokenImage.color;
            c.a = 0f;
            animatedTokenImage.color = c;
        }

        animatedToken.gameObject.SetActive(false);
    }


    // -----------------------------------
    // STATE 3: QR scanned, gameplay starts
    // -----------------------------------
    public void OnQRCodeScanned()
    {
        if (gameplayStarted)
            return;

        gameplayStarted = true;

        if (commonInstructionGroup != null)
            commonInstructionGroup.SetActive(true);

        if (gameplayUIGroup  != null)
            gameplayUIGroup.SetActive(true);

        if (disabledTokenIcon != null)
            disabledTokenIcon.SetActive(false);

        SetInstruction("Tap once on the location marker to view the intel, double tap to collect the intel.");
    }


    // -----------------------------------
    // Shared instruction text
    // -----------------------------------
    public void SetInstruction(string message)
    {
        if (instructionText != null)
            instructionText.text = message;
    }


    // -----------------------------------
    // Fade helper
    // -----------------------------------
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
