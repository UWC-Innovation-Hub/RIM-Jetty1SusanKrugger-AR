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
using System.IO;
using System.Linq;
using UnityEngine;

namespace ObjectTracking
{
    public class ObjectTrackingSample : MonoBehaviour
    {
        public ARSession Session;

        private List<ObjectTargetController> targets = new List<ObjectTargetController>();
        private ObjectTrackerFrameFilter objectTracker;
        private CameraDeviceFrameSource cameraDevice;

#if UNITY_EDITOR
        private static readonly string[] streamingAssetsFiles = new string[] {
            "EasyARSamples/ObjectTargets/hexagon/hexagon.obj",
            "EasyARSamples/ObjectTargets/hexagon/hexagon.mtl",
            "EasyARSamples/ObjectTargets/hexagon/hexagon.jpg",
        };

        [UnityEditor.InitializeOnLoadMethod]
        public static void ImportSampleStreamingAssets()
        {
            var pacakge = $"Packages/{UnityPackage.Name}/Samples~/StreamingAssets/ObjectTargets/ObjectTargets.unitypackage";

            if (streamingAssetsFiles.Where(f => !File.Exists(Path.Combine(Application.streamingAssetsPath, f))).Any() && File.Exists(Path.GetFullPath(pacakge)))
            {
                UnityEditor.AssetDatabase.ImportPackage(pacakge, false);
            }
        }
#endif

        private void Awake()
        {
            AdaptInputSystem();
            objectTracker = Session.GetComponentInChildren<ObjectTrackerFrameFilter>();
            cameraDevice = Session.GetComponentInChildren<CameraDeviceFrameSource>();

            foreach (var controller in
#if UNITY_2022_3_OR_NEWER
                GameObject.FindObjectsByType<ObjectTargetController>(FindObjectsSortMode.None)
#else
                GameObject.FindObjectsOfType<ObjectTargetController>()
#endif
                )
            {
                targets.Add(controller);
                HandleTargetControllerEvents(controller);
                if (!Session.SpecificTargetCenter)
                {
                    Session.SpecificTargetCenter = controller.gameObject;
                }
            }
            HandleTrackerEvents(objectTracker);
        }

        public void Tracking(bool on)
        {
            objectTracker.enabled = on;
        }

        public void UnloadTargets()
        {
            foreach (var target in targets)
            {
                target.Tracker = null;
            }
        }

        public void LoadTargets()
        {
            foreach (var target in targets)
            {
                target.Tracker = objectTracker;
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

        public void EnableCamera(bool enable)
        {
            cameraDevice.enabled = enable;
        }

        public void SwitchHFlipMode()
        {
            Session.HorizontalFlip.FrontCamera = (ARSession.ARHorizontalFlipMode)(((int)Session.HorizontalFlip.FrontCamera + 1) % Enum.GetValues(typeof(ARSession.ARHorizontalFlipMode)).Length);
            Session.HorizontalFlip.BackCamera = (ARSession.ARHorizontalFlipMode)(((int)Session.HorizontalFlip.BackCamera + 1) % Enum.GetValues(typeof(ARSession.ARHorizontalFlipMode)).Length);
        }

        public void NextCamera()
        {
            if (!cameraDevice || !cameraDevice.Opened) { return; }
            if (CameraDeviceFrameSource.CameraCount == 0)
            {
                cameraDevice.Close();
                return;
            }

            var index = cameraDevice.Index;
            index = (index + 1) % CameraDeviceFrameSource.CameraCount;
            cameraDevice.CameraOpenMethod = CameraDeviceFrameSource.CameraDeviceOpenMethod.DeviceIndex;
            cameraDevice.CameraOpenIndex = index;

            cameraDevice.Close();
            cameraDevice.Open();
        }

        private void HandleTargetControllerEvents(ObjectTargetController controller)
        {
            controller.TargetDataLoad += (status) =>
            {
                Debug.Log($"Load data from {controller.Source.GetType()} resource into target {(controller.Target == null ? string.Empty : $"{{id = {controller.Target.runtimeID()}, name = {controller.Target.name()}}} ")}=> {status}");
            };
            controller.TargetFound += () =>
            {
                Debug.Log($"Found target {{id = {controller.Target.runtimeID()}, name = {controller.Target.name()}}}");
            };
            controller.TargetLost += () =>
            {
                if (!controller) { return; }
                Debug.Log($"Lost target {{id = {controller.Target.runtimeID()}, name = {controller.Target.name()}}}");
            };
        }

        private void HandleTrackerEvents(ObjectTrackerFrameFilter tracker)
        {
            tracker.TargetLoad += (controller, status) =>
            {
                if (!controller) { return; }
                Debug.Log($"Load target {{id = {controller.Target.runtimeID()}, name = {controller.Target.name()}}} into {tracker.name} => {status}");
            };
            tracker.TargetUnload += (controller, status) =>
            {
                if (!controller) { return; }
                Debug.Log($"Unload target {{id = {controller.Target.runtimeID()}, name = {controller.Target.name()}}} from {tracker.name} => {status}");
            };
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
