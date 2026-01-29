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

namespace ImageTracking_Video
{
    [RequireComponent(typeof(MeshRenderer), typeof(UnityEngine.Video.VideoPlayer))]
    public class VideoOnTargetSample : MonoBehaviour
    {
        public Button SwitchButton;

        private MeshRenderer meshRenderer;
        private UnityEngine.Video.VideoPlayer player;
        private bool prepared;
        private bool found;
        private RenderTexture renderTexture;

        private void Awake()
        {
            AdaptInputSystem();
        }

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            player = GetComponent<UnityEngine.Video.VideoPlayer>();
            if (SwitchButton)
            {
                SwitchButton.GetComponentInChildren<Text>().text = $"SwitchRatioFit\ncur: {player.aspectRatio}";
            }
            StatusChanged();

            var controller = GetComponentInParent<ImageTargetController>();
            controller.ActiveController.OverrideStrategy = ActiveController.Strategy.ActiveAfterFirstTracked;
            controller.TargetDataLoad += (status) =>
            {
                if (!status) { return; }
                if (player.renderMode == UnityEngine.Video.VideoRenderMode.RenderTexture)
                {
                    var height = 1080;
                    renderTexture = new RenderTexture((int)(height * controller.Target.aspectRatio()), height, 0);
                    renderTexture.wrapMode = TextureWrapMode.Clamp;
                    renderTexture.filterMode = FilterMode.Bilinear;
                    player.targetTexture = renderTexture;
                    meshRenderer.material.mainTexture = renderTexture;
                }
            };
            controller.TargetFound += () =>
            {
                if (player.renderMode == UnityEngine.Video.VideoRenderMode.RenderTexture)
                {
                    transform.localScale = new Vector3(1, 1 / controller.Target.aspectRatio(), 1);
                }
                found = true;
                StatusChanged();
            };
            controller.TargetLost += () =>
            {
                found = false;
                if (!this || !meshRenderer) { return; }
                StatusChanged();
            };

            player.prepareCompleted += (source) =>
            {
                prepared = true;
                StatusChanged();
            };
        }

        private void OnDestroy()
        {
            if (renderTexture) { Destroy(renderTexture); }
        }

        public void ChangeVideoAspectRatio()
        {
            if (player.renderMode != UnityEngine.Video.VideoRenderMode.RenderTexture) { return; }
            player.aspectRatio = (UnityEngine.Video.VideoAspectRatio)(((int)player.aspectRatio + 1) % Enum.GetValues(typeof(UnityEngine.Video.VideoAspectRatio)).Length);
            var session =
#if UNITY_2022_3_OR_NEWER
                FindAnyObjectByType<ARSession>();
#else
                FindObjectOfType<ARSession>();
#endif
            if (SwitchButton)
            {
                SwitchButton.GetComponentInChildren<Text>().text = $"SwitchRatioFit\ncur: {player.aspectRatio}";
            }
        }

        private void StatusChanged()
        {
            if (found)
            {
                meshRenderer.enabled = prepared;
                if (player && player.gameObject.activeInHierarchy)
                {
                    player.Play();
                }
            }
            else
            {
                meshRenderer.enabled = false;
                if (player && player.gameObject.activeInHierarchy)
                {
                    player.Pause();
                }
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
