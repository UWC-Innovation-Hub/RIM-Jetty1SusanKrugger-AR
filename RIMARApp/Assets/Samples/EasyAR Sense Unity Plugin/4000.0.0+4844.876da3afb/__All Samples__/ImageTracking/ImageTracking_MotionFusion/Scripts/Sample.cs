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
using UnityEngine.UI;

namespace Sample
{
    public class Sample : MonoBehaviour
    {
        public Text Status;
        public ARSession Session;

        private void Awake()
        {
            AdaptInputSystem();
            Session.StateChanged += (state) =>
            {
                if (Session.State == ARSession.SessionState.Assembling)
                {
                    Status.text = "Please wait while checking all frame source availability...";
                }

                if (Session.State == ARSession.SessionState.Ready)
                {
                    if (Session.Assembly.FrameSource is CameraDeviceFrameSource)
                    {
                        Status.text = "Motion tracking capability not available on this device." + Environment.NewLine +
                            "Fallback to image tracking without motion fusion.";
                    }
                    else
                    {
                        var tracker = Session.Assembly.FrameFilters.Where(f => f is ImageTrackerFrameFilter).FirstOrDefault() as ImageTrackerFrameFilter;
                        Status.text = "Motion Fusion: " + tracker.EnableMotionFusion + Environment.NewLine +
                            (tracker.EnableMotionFusion ? "Image must NOT move in real world." : "Image is free to move in real world.") + Environment.NewLine +
                        Environment.NewLine +
                        "    Image target scale must be set to physical image width." + Environment.NewLine +
                        "    Scale is preset to match long edge of A4 paper." + Environment.NewLine +
                        "    Suggest to print out images for test.";
                    }
                }
            };
        }

        public void SwitchMotionFusion(bool on)
        {
            Session.GetComponentInChildren<ImageTrackerFrameFilter>().EnableMotionFusion = on;
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
