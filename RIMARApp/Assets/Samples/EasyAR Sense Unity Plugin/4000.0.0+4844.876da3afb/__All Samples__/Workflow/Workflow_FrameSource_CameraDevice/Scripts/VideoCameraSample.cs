//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Camera_VideoCamera
{
    public class VideoCameraSample : MonoBehaviour
    {
        public ARSession arSession;
        public SkinnedMeshRenderer MeshRenderer;
        public Toggle FlipSwitch;

        private CameraDeviceFrameSource videoCamera;
        private CameraImageRenderer cameraRenderer;
        private Texture cubeTexture;
        private Action<Camera, RenderTexture> targetTextureEventHandler;
        private bool torch;
        private int sizeIndex;

        private void Awake()
        {
            AdaptInputSystem();
            arSession.StateChanged += (state) =>
            {
                if (state == ARSession.SessionState.Ready)
                {
                    cameraRenderer = arSession.Assembly.CameraImageRenderer.Value;
                    videoCamera = arSession.Assembly.FrameSource as CameraDeviceFrameSource;
                    if (videoCamera)
                    {
                        videoCamera.DeviceOpened += (success, _, _) =>
                        {
                            if (!success) { return; }
                            var flip = videoCamera.CameraType == CameraDeviceType.Front ? arSession.HorizontalFlip.FrontCamera : arSession.HorizontalFlip.BackCamera;
                            FlipSwitch.isOn = flip == ARSession.ARHorizontalFlipMode.World;
                        };
                    }
                }
            };

            cubeTexture = MeshRenderer.material.mainTexture;
            targetTextureEventHandler = (camera, texture) =>
            {
                if (!MeshRenderer) { return; }
                MeshRenderer.material.mainTexture = texture ? texture : cubeTexture;
            };
        }

        public void NextCamera()
        {
            if (!videoCamera || !videoCamera.Opened) { return; }
            if (CameraDeviceFrameSource.CameraCount == 0)
            {
                videoCamera.Close();
                return;
            }

            var index = videoCamera.Index;
            index = (index + 1) % CameraDeviceFrameSource.CameraCount;
            videoCamera.CameraOpenMethod = CameraDeviceFrameSource.CameraDeviceOpenMethod.DeviceIndex;
            videoCamera.CameraOpenIndex = index;

            videoCamera.Close();
            videoCamera.Open();
        }

        public void Capture(bool on)
        {
            if (!cameraRenderer) { return; }

            if (on)
            {
                cameraRenderer.RequestTargetTexture(targetTextureEventHandler);
            }
            else
            {
                cameraRenderer.DropTargetTexture(targetTextureEventHandler);
            }
        }

        public void EnableCamera(bool enable)
        {
            if (!videoCamera) { return; }
            videoCamera.enabled = enable;
        }

        public void ShowCameraImage(bool show)
        {
            if (!cameraRenderer) { return; }
            cameraRenderer.enabled = show;
        }

        public void HFlip(bool flip)
        {
            arSession.HorizontalFlip.FrontCamera = flip ? ARSession.ARHorizontalFlipMode.World : ARSession.ARHorizontalFlipMode.None;
            arSession.HorizontalFlip.BackCamera = flip ? ARSession.ARHorizontalFlipMode.World : ARSession.ARHorizontalFlipMode.None;
        }

        public void FlashTorch()
        {
            if (!videoCamera || !videoCamera.Opened) { return; }
            torch = !torch;
            videoCamera.SetFlashTorch(torch);
        }

        public void LoopSize()
        {
            if (!videoCamera || !videoCamera.Opened) { return; }
            var supportedSize = videoCamera.SupportedSize;
            if (supportedSize.Count == 0) { return; }
            videoCamera.Size = supportedSize[sizeIndex % supportedSize.Count];
            sizeIndex++;
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
