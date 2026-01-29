//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================


using easyar;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sample
{
    public class SparseSpatailMapSample : MonoBehaviour
    {
        public ARSession Session;
        public GameObject TouchRoot;
        public ImageTargetController ImageTarget;

        private SparseSpatialMapBuilderFrameFilter sparse;
        private TouchController touchControl;

        private void Awake()
        {
            AdaptInputSystem();
            Session.StateChanged += (state) =>
            {
                if (state == ARSession.SessionState.Ready)
                {
                    sparse = Session.Assembly.FrameFilters.Where(f => f is SparseSpatialMapBuilderFrameFilter).FirstOrDefault() as SparseSpatialMapBuilderFrameFilter;
                    touchControl = new TouchController(TouchRoot.transform, Session.Assembly.Camera, false, false, true);
                }
            };
        }

        private void Update()
        {
            Vector2? position = null;
#if ENABLE_LEGACY_INPUT_MANAGER
            touchControl?.Update(Input.touches.ToDictionary(t => t.fingerId, t => t.position));
            if ((Input.touchCount == 1 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) && Input.touches[0].phase == UnityEngine.TouchPhase.Moved) || (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()))
            {
                position = Input.mousePosition;
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
            else if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.isPressed && !EventSystem.current.IsPointerOverGameObject())
            {
                position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            }
#endif
            if (position.HasValue)
            {
                MoveOnMap(position.Value);
            }
        }

        private void MoveOnMap(Vector2 screenPoint)
        {
            if (!sparse || !sparse.Target) { return; }

            var viewPoint = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
            var points = sparse.Target.HitTest(viewPoint);
            foreach (var point in points)
            {
                TouchRoot.transform.position = sparse.Target.transform.TransformPoint(point);
                break;
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
