using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GestureManager : MonoBehaviour
{
    [Header("Placeholders")]
    public Transform historicalPeriodsRoot; // children are Period_01, Period_02...
    public Transform storyLayersRoot; // children are layers (ordered bottom->top)
    public Transform worldContent; // large AR content to pan

    [Header("Tuning")]
    public float swipeThresholdPx = 125f;
    public float verticalHorizontalRatio = 1.5f;
    public float twoFingerPinchThresh = 5f; // pixels change to treat as pinch
    public float panSpeed = 0.0015f; // world units per screen pixel

    int currentPeriodIndex = 0;
    int currentLayerIndex = 0;

    void Start()
    {
        // init placeholders to first child active
        SetActiveChild(historicalPeriodsRoot, currentPeriodIndex);
        SetActiveChild(storyLayersRoot, currentLayerIndex);
    }

    void Update()
    {
        var touches = Touchscreen.current?.touches;
        if (touches == null) return;

        // Gather active touches
        List<TouchControl> activeTouches = new List<TouchControl>();
        foreach (var t in touches)
            if (t.isInProgress)
                activeTouches.Add(t);

        if (activeTouches.Count == 0) return;

        if (activeTouches.Count == 1)
        {
            HandleSingleTouch(activeTouches[0]);
        }
        else if (activeTouches.Count == 2)
        {
            HandleTwoFinger(activeTouches[0], activeTouches[1]);
        }
        // ignore >2 for now
    }

    // ---------- Single touch handling (swipe detection) ----------
    Vector2 startPos;
    double startTime;
    bool touchStarted = false;

    void HandleSingleTouch(TouchControl touch)
    {
        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            touchStarted = true;
            startPos = touch.position.ReadValue();
            startTime = Time.time;
        }
        else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended && touchStarted)
        {
            Vector2 endPos = touch.position.ReadValue();
            Vector2 delta = endPos - startPos;

            if (delta.magnitude >= swipeThresholdPx)
            {
                // decide horizontal or vertical
                if (Mathf.Abs(delta.x) >= verticalHorizontalRatio * Mathf.Abs(delta.y))
                {
                    // horizontal swipe
                    if (delta.x > 0) SwipeRight();
                    else SwipeLeft();
                }
                else if (Mathf.Abs(delta.y) >= verticalHorizontalRatio * Mathf.Abs(delta.x))
                {
                    // vertical swipe
                    if (delta.y > 0) SwipeUp();
                    else SwipeDown();
                }
            }

            touchStarted = false;
        }
    }

    // ---------- Two-finger pan (and pinch guard) ----------
    Vector2 lastPos0, lastPos1;
    bool twoFingerActive = false;

    void HandleTwoFinger(TouchControl t0, TouchControl t1)
    {
        var phase0 = t0.phase.ReadValue();
        var phase1 = t1.phase.ReadValue();

        Vector2 pos0 = t0.position.ReadValue();
        Vector2 pos1 = t1.position.ReadValue();

        if (phase0 == UnityEngine.InputSystem.TouchPhase.Began || phase1 == UnityEngine.InputSystem.TouchPhase.Began)
        {
            twoFingerActive = true;
            lastPos0 = pos0;
            lastPos1 = pos1;
            return;
        }

        if (!twoFingerActive) return;

        // compute deltas
        Vector2 d0 = pos0 - lastPos0;
        Vector2 d1 = pos1 - lastPos1;

        // distance change = pinch detection
        float lastDistance = Vector2.Distance(lastPos0, lastPos1);
        float curDistance = Vector2.Distance(pos0, pos1);
        float distanceDelta = Mathf.Abs(curDistance - lastDistance);

        if (distanceDelta > twoFingerPinchThresh)
        {
            // user is pinching -> ignore pan (could route to zoom)
            lastPos0 = pos0; lastPos1 = pos1;
            return;
        }

        // check direction similarity (so it isn't rotate)
        float angle = Vector2.Angle(d0, d1);
        if (angle > 45f) // not moving the same way
        {
            lastPos0 = pos0; lastPos1 = pos1;
            return;
        }

        // average movement -> pan
        Vector2 avgDelta = (d0 + d1) * 0.5f;

        PanWorldByScreenDelta(avgDelta);

        lastPos0 = pos0;
        lastPos1 = pos1;
    }

    void PanWorldByScreenDelta(Vector2 screenDelta)
    {
        if (worldContent == null) return;
        Vector3 move = new Vector3(-screenDelta.x * panSpeed, 0f, -screenDelta.y * panSpeed);
        // apply in worldContent's parent space for consistent behavior
        worldContent.Translate(move, Space.Self);
    }

    // ---------- Action handlers ----------
    void SwipeLeft()
    {
        currentPeriodIndex = Mathf.Max(0, currentPeriodIndex - 1);
        SetActiveChild(historicalPeriodsRoot, currentPeriodIndex);
        Debug.Log("SwipeLeft -> Period " + currentPeriodIndex);
    }

    void SwipeRight()
    {
        currentPeriodIndex = Mathf.Min(historicalPeriodsRoot.childCount - 1, currentPeriodIndex + 1);
        SetActiveChild(historicalPeriodsRoot, currentPeriodIndex);
        Debug.Log("SwipeRight -> Period " + currentPeriodIndex);
    }

    void SwipeUp()
    {
        currentLayerIndex = Mathf.Min(storyLayersRoot.childCount - 1, currentLayerIndex + 1);
        SetActiveChild(storyLayersRoot, currentLayerIndex);
        Debug.Log("SwipeUp -> Layer " + currentLayerIndex);
    }

    void SwipeDown()
    {
        currentLayerIndex = Mathf.Max(0, currentLayerIndex - 1);
        SetActiveChild(storyLayersRoot, currentLayerIndex);
        Debug.Log("SwipeDown -> Layer " + currentLayerIndex);
    }

    void SetActiveChild(Transform parent, int activeIndex)
    {
        if (parent == null) return;
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(i == activeIndex);
        }
    }
}
