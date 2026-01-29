//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sample
{
    public class MapLocalizing_SparseSample : MonoBehaviour
    {
        public ARSession Session;
        public SparseSpatialMapController MapController;
        public GameObject TouchRoot;
        public Text Text;

        private SparseSpatialMapTrackerFrameFilter sparse;
        private TouchController touchControl;
        private string text;

        private void Awake()
        {
            AdaptInputSystem();
            text = Text.text;
            Session.StateChanged += (state) =>
            {
                if (state == ARSession.SessionState.Ready)
                {
                    touchControl = new TouchController(TouchRoot.transform, Session.Assembly.Camera, false, false, true);
                }
            };

            if (MapController.Source is SparseSpatialMapController.MapManagerSourceData mapManagerSource && string.IsNullOrEmpty(mapManagerSource.ID.Trim()))
            {
                var message = "Map ID NOT set, please set Source on: " + MapController + Environment.NewLine +
                    "To create SparseSpatialMap, use <SpatialMap_Sparse_AllInOne> sample." + Environment.NewLine +
                    "To get Map ID, use EasyAR Develop Center (www.easyar.com) -> SpatialMap -> Database Details." + Environment.NewLine +
                    "Map ID is used when loading, it can be used to share maps among devices.";
                Text.text = message;
                throw new InvalidOperationException(message);
            }

            sparse = Session.GetComponentInChildren<SparseSpatialMapTrackerFrameFilter>();
            sparse.TargetLoad += (map, status, error) =>
            {
                if (!map) { return; }
                Debug.Log("Load map {name = " + map.Info.Name + ", id = " + map.Info.ID + "} into " + sparse.name + Environment.NewLine +
                    " => " + status + (string.IsNullOrEmpty(error) ? "" : " <" + error + ">"));

                if (!status) { return; }
                StartCoroutine(ShowLoadMessage());
            };

            MapController.TargetFound += () =>
            {
                Debug.Log($"Found target {{name = {MapController.Info.Name}}}");
            };

            MapController.TargetLost += () =>
            {
                if (!MapController) { return; }
                Debug.Log($"Lost target {{name = {MapController.Info.Name}}}");
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
            if (!MapController || !MapController.IsDirectlyTracked) { return; }

            var viewPoint = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
            var points = MapController.HitTest(viewPoint);
            foreach (var point in points)
            {
                TouchRoot.transform.position = MapController.transform.TransformPoint(point);
                break;
            }
        }

        private IEnumerator ShowLoadMessage()
        {
            Text.text = text + "Notice: load map (only the first time each map) will trigger a download in this sample." + Environment.NewLine +
                "Statistical request count will be increased (more details on EasyAR website)." + Environment.NewLine +
                "Map cache is used after a successful download." + Environment.NewLine +
                "Map cache will be cleared if SparseSpatialMapManager.clear is called or app uninstalled.";
            yield return new WaitForSeconds(10);
            Text.text = text;
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
