//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sample
{
    public class UIController : MonoBehaviour
    {
        public ARSession Session;
        public GameObject TouchRoot;

        private SurfaceTrackerFrameFilter tracker;
        private TouchController touchControl;

        private void Awake()
        {
            AdaptInputSystem();
            Session.StateChanged += (state) =>
            {
                if (state == ARSession.SessionState.Ready)
                {
                    tracker = Session.Assembly.FrameFilters.Where(f => f is SurfaceTrackerFrameFilter).FirstOrDefault() as SurfaceTrackerFrameFilter;
                    touchControl = new TouchController(TouchRoot.transform, Session.Assembly.Camera, false, true, true);
                }
            };
        }

        private void Update()
        {
            Vector2? position = null;
#if ENABLE_LEGACY_INPUT_MANAGER
            touchControl?.Update(Input.touches.ToDictionary(t => t.fingerId, t => t.position));
            if (Input.touchCount == 1 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) && Input.touches[0].phase == UnityEngine.TouchPhase.Moved)
            {
                position = Input.touches[0].position;
            }
#elif INPUTSYSTEM_PACKAGE_INSTALLED
            if (UnityEngine.InputSystem.Touchscreen.current != null)
            {
                var activeTouches = UnityEngine.InputSystem.Touchscreen.current.touches.Where(t => t.isInProgress).ToList();
                touchControl?.Update(activeTouches.ToDictionary(t => t.touchId.ReadValue(), t => t.position.ReadValue()));
                if (activeTouches.Count == 1 && activeTouches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && !EventSystem.current.IsPointerOverGameObject(activeTouches[0].touchId.ReadValue()))
                {
                    position = activeTouches[0].position.ReadValue();
                }
            }
#endif
            if (position.HasValue)
            {
                AlignTo(position.Value);
            }
        }

        public void SwitchCenterMode()
        {
            if (Session.AvailableCenterMode.Count == 0) { return; }
            while (true)
            {
                Session.CenterMode = (ARSession.ARCenterMode)(((int)Session.CenterMode + 1) % Enum.GetValues(typeof(ARSession.ARCenterMode)).Length);
                if (Session.AvailableCenterMode.Contains(Session.CenterMode)) { break; }
            }
        }

        private void AlignTo(Vector2 screenPoint)
        {
            if (!tracker || !tracker.Target) { return; }

            var viewPoint = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
            tracker.Target.AlignTo(viewPoint);
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
