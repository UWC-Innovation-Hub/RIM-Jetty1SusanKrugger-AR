//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sample
{
    public class Sample : MonoBehaviour
    {
        public Text Status;
        public ARSession Session;
        public Button UnlockPlaneButton;
        public GameObject Plane;
        public GameObject TouchRoot;
        private TouchController touchControl;

        private void Awake()
        {
            AdaptInputSystem();
            Session.StateChanged += (state) =>
            {
                if (state == ARSession.SessionState.Ready)
                {
                    Status.text = $"Frame source: {ARSessionFactory.DefaultName(Session.Assembly.FrameSource.GetType())}" + Environment.NewLine +
                    "Device motion tracking: YES" + Environment.NewLine +
                    $"Plane tracking: {(Session.Assembly.FrameSource is ARFoundationFrameSource || Session.Assembly.FrameSource is MotionTrackerFrameSource ? "YES" : "NO (try other frame source)")}" + Environment.NewLine +
                    Environment.NewLine;

                    if (Session.Assembly.FrameSource is ARFoundationFrameSource)
                    {
                        touchControl = new TouchController(TouchRoot.transform, Session.Assembly.Camera, false, false, true);
                        UnlockPlaneButton.gameObject.SetActive(false);
                        Status.text +=
                            "Plane Detection: Enabled" + Environment.NewLine +
#if ENABLE_ARFOUNDATION
                            "Plane Count: " + Session.Assembly.Origin.Value.GetComponent<UnityEngine.XR.ARFoundation.ARPlaneManager>().trackables.count + Environment.NewLine +
#endif
                            Environment.NewLine +
                            "Gesture Instruction" + Environment.NewLine +
                            "\tMove on Detected Plane: One Finger Move" + Environment.NewLine +
                            "\tScale: Two Finger Pinch";
                    }
                    else if (Session.Assembly.FrameSource is MotionTrackerFrameSource)
                    {
                        touchControl = new TouchController(TouchRoot.transform, Session.Assembly.Camera, false, true, true);
                        UnlockPlaneButton.gameObject.SetActive(true);
                        Status.text +=
                            "Gesture Instruction" + Environment.NewLine +
                            "\tMove on Detected Plane: One Finger Move" + Environment.NewLine +
                            "\tRotate: Two Finger Horizontal Move" + Environment.NewLine +
                            "\tScale: Two Finger Pinch";
                    }
                    else
                    {
                        touchControl = new TouchController(TouchRoot.transform, Session.Assembly.Camera, true, true, true);
                        UnlockPlaneButton.gameObject.SetActive(false);
                        Status.text +=
                            "Gesture Instruction" + Environment.NewLine +
                            "\tMove in View: One Finger Move" + Environment.NewLine +
                            "\tMove Near/Far: Two Finger Vertical Move" + Environment.NewLine +
                            "\tRotate: Two Finger Horizontal Move" + Environment.NewLine +
                            "\tScale: Two Finger Pinch";
                    }
                }
            };
        }

        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            touchControl?.Update(Input.touches.ToDictionary(t => t.fingerId, t => t.position));
#elif INPUTSYSTEM_PACKAGE_INSTALLED
            if (UnityEngine.InputSystem.Touchscreen.current != null)
            {
                var activeTouches = UnityEngine.InputSystem.Touchscreen.current.touches.Where(t => t.isInProgress).ToList();
                touchControl?.Update(activeTouches.ToDictionary(t => t.touchId.ReadValue(), t => t.position.ReadValue()));
            }
#endif
            if (Session.State >= ARSession.SessionState.Ready)
            {
                if (Session.Assembly.FrameSource is MotionTrackerFrameSource)
                {
                    if (!UnlockPlaneButton.interactable)
                    {
                        DetectAndPlacePlane(new Vector2(0.5f, 0.333f));
                    }
                }

                Vector2? position = null;
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.touchCount == 1 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    var touch = Input.touches[0];
                    if (touch.phase == UnityEngine.TouchPhase.Moved || touch.phase == UnityEngine.TouchPhase.Stationary)
                    {
                        position = touch.position;
                    }
                }
#elif INPUTSYSTEM_PACKAGE_INSTALLED
                if (UnityEngine.InputSystem.Touchscreen.current != null)
                {
                    var activeTouches = UnityEngine.InputSystem.Touchscreen.current.touches.Where(t => t.isInProgress).ToList();
                    if (activeTouches.Count == 1 && !EventSystem.current.IsPointerOverGameObject(activeTouches[0].touchId.ReadValue()))
                    {
                        if (activeTouches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || activeTouches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary)
                        {
                            position = activeTouches[0].position.ReadValue();
                        }
                    }
                }
#endif
                if (position.HasValue)
                {
                    PlaceObject(position.Value);
                }
            }
        }

        public void SwitchCenterMode()
        {
            if (Session.AvailableCenterMode.Count == 0) { return; }
            while (true)
            {
                Session.CenterMode = (ARSession.ARCenterMode)(((int)Session.CenterMode + 1) % Enum.GetValues(typeof(ARSession.ARCenterMode)).Length);
                if (Session.AvailableCenterMode.Contains(Session.CenterMode) && Session.CenterMode != ARSession.ARCenterMode.Camera) { break; }
            }
        }

        private void DetectAndPlacePlane(Vector2 viewPoint)
        {
            if (Session.State < ARSession.SessionState.Ready || !(Session.Assembly.FrameSource is MotionTrackerFrameSource)) { return; }

            var motionTracker = Session.Assembly.FrameSource as MotionTrackerFrameSource;
            var points = motionTracker.HitTestAgainstHorizontalPlane(viewPoint);
            if (points.Count <= 0) { return; }

            var viewportPoint = Session.Assembly.Camera.WorldToViewportPoint(Plane.transform.position);
            if (!Plane.activeSelf || viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || Mathf.Abs(Plane.transform.position.y - points[0].y) > 0.15)
            {
                Plane.SetActive(true);
                Plane.transform.position = points[0];
                Plane.transform.localScale = Vector3.one * (Session.Assembly.Camera.transform.position - points[0]).magnitude;
            }
        }

        private void PlaceObject(Vector2 touchPosition)
        {
            if (Session.Assembly.FrameSource is MotionTrackerFrameSource)
            {
                Ray ray = Session.Assembly.Camera.ScreenPointToRay(touchPosition);
                if (Physics.Raycast(ray, out var hitInfo))
                {
                    TouchRoot.transform.position = hitInfo.point;
                    UnlockPlaneButton.interactable = true;
                }
            }
            else if (Session.Assembly.FrameSource is ARFoundationFrameSource)
            {
#if ENABLE_ARFOUNDATION
                var raycastManager = Session.Assembly.Origin.Value.GetComponent<UnityEngine.XR.ARFoundation.ARRaycastManager>();
                var hits = new List<UnityEngine.XR.ARFoundation.ARRaycastHit>();
                if (raycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                {
                    var hitPose = hits[0].pose;
                    TouchRoot.transform.position = hitPose.position;
                }
#endif
            }
        }

        private void AdaptInputSystem()
        {
            if (!UnityEngine.EventSystems.EventSystem.current) { return; }
#if ENABLE_INPUT_SYSTEM && INPUTSYSTEM_PACKAGE_INSTALLED
#if ENABLE_LEGACY_INPUT_MANAGER
            var inputMD = typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule);
            var inputM = typeof(UnityEngine.EventSystems.StandaloneInputModule);
#else
            var inputMD = typeof(UnityEngine.EventSystems.StandaloneInputModule);
            var inputM = typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule);
#endif
            if (UnityEngine.EventSystems.EventSystem.current.GetComponent(inputMD)) { Destroy(UnityEngine.EventSystems.EventSystem.current.GetComponent(inputMD)); }
            if (!UnityEngine.EventSystems.EventSystem.current.GetComponent(inputM)) { UnityEngine.EventSystems.EventSystem.current.gameObject.AddComponent(inputM); }
#endif
        }
    }
}
