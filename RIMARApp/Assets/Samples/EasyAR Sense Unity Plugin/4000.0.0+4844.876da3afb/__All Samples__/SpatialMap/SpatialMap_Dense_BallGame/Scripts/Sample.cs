//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpatialMap_Dense_BallGame
{
    public class Sample : MonoBehaviour
    {
        public ARSession Session;
        public GameObject Ball;
        public int MaxBallCount = 30;
        public float BallLifetime = 15;

        private Color meshColor;
        private DenseSpatialMapBuilderFrameFilter dense;
        private List<GameObject> balls = new List<GameObject>();

        private void Awake()
        {
            AdaptInputSystem();
            Session.Diagnostics.DeveloperModeSwitch = DiagnosticsController.DeveloperModeSwitchType.Custom;
            Session.StateChanged += (state) =>
            {
                if (state == ARSession.SessionState.Ready)
                {
                    dense = Session.Assembly.FrameFilters.Where(f => f is DenseSpatialMapBuilderFrameFilter).FirstOrDefault() as DenseSpatialMapBuilderFrameFilter;
                    meshColor = dense.MeshColor;
                }
            };
        }

        private void Update()
        {
            Vector3 touchPosition = new Vector3();
#if ENABLE_LEGACY_INPUT_MANAGER
            if ((Input.touchCount == 1 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) && Input.touches[0].phase == UnityEngine.TouchPhase.Began) || (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()))
            {
                touchPosition = Input.mousePosition;
            }
            else
            {
                return;
            }
#elif INPUTSYSTEM_PACKAGE_INSTALLED
            if (UnityEngine.InputSystem.Touchscreen.current != null)
            {
                var activeTouches = UnityEngine.InputSystem.Touchscreen.current.touches.Where(t => t.isInProgress).ToList();
                if (activeTouches.Count == 1 && activeTouches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(activeTouches[0].touchId.ReadValue()))
                {
                    touchPosition = activeTouches[0].position.ReadValue();
                }
                else
                {
                    return;
                }
            }
            else if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
            {
                touchPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            }
            else
            {
                return;
            }
#endif
            if (Session.State >= ARSession.SessionState.Ready)
            {
                Ray ray = Session.Assembly.Camera.ScreenPointToRay(touchPosition);
                var launchPoint = Session.Assembly.Camera.transform;
                var ball = Instantiate(Ball, launchPoint.position, launchPoint.rotation);
                var rigid = ball.GetComponent<Rigidbody>();
#if UNITY_6000_0_OR_NEWER
                rigid.linearVelocity = Vector3.zero;
#else
                rigid.velocity = Vector3.zero;
#endif
                rigid.AddForce(ray.direction * 15f + Vector3.up * 5f);
                if (balls.Count > 0 && balls.Count == MaxBallCount)
                {
                    Destroy(balls[0]);
                    balls.RemoveAt(0);
                }
                balls.Add(ball);
                StartCoroutine(Kill(ball, BallLifetime));
            }
        }

        public void RenderMesh(bool show)
        {
            if (!dense) { return; }
            dense.RenderMesh = show;
        }


        public void TransparentMesh(bool trans)
        {
            if (!dense) { return; }
            dense.MeshColor = trans ? Color.clear : meshColor;
        }

        private IEnumerator Kill(GameObject ball, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            if (balls.Remove(ball)) { Destroy(ball); }
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
