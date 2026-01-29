//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MultiTarget_SameImage
{
    public class MultiTargetSameImageSample : MonoBehaviour
    {
        public Texture2D TargetTexture;
        public ARSession Session;
        public GameObject Panda;
        private ImageTrackerFrameFilter imageTracker;

        private void Awake()
        {
            AdaptInputSystem();
        }

        private void Start()
        {
            imageTracker = Session.GetComponentInChildren<ImageTrackerFrameFilter>();
            HandleTrackerEvents(imageTracker);
            var source = new ImageTargetController.Texture2DSourceData()
            {
                Texture = TargetTexture,
                Name = TargetTexture.name,
                Scale = 0.09f
            };
            if (!source.IsTextureLoadable)
            {
                throw new Exception($"Error loading target Texture2D data for {source.Name}: {source.TextureUnloadableReason}");
            }
            LoadTexture2DData(source);
        }

        private unsafe void LoadTexture2DData(ImageTargetController.Texture2DSourceData source)
        {
            var data = source.Texture.GetRawTextureData<byte>();
            var size = new Vector2Int(source.Texture.width, source.Texture.height);
            var pixelFormat = source.TexturePixelFormat.Value;
            var scale = source.Scale;

            var ptr = data.GetUnsafeReadOnlyPtr();
            int oneLineLength = size.x * ((pixelFormat == PixelFormat.RGBA8888 || pixelFormat == PixelFormat.BGRA8888) ? 4 : ((pixelFormat == PixelFormat.RGB888 || pixelFormat == PixelFormat.BGR888) ? 3 : 1));
            int totalLength = oneLineLength * size.y;
            using (var buffer = easyar.Buffer.create(totalLength))
            {
                for (int i = 0; i < size.y; i++)
                {
                    buffer.tryCopyFrom(new IntPtr(ptr), oneLineLength * i, totalLength - oneLineLength * (i + 1), oneLineLength);
                }
                using (var image = easyar.Image.create(buffer, pixelFormat, size.x, size.y, size.x, size.y))
                {
                    CreateMultipleTargetsFromOneImage(image, 10, name, scale);
                }
            }
        }

        private void CreateMultipleTargetsFromOneImage(easyar.Image image, int count, string name, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                using (var param = new ImageTargetParameters())
                {
                    param.setImage(image);
                    param.setName(name);
                    param.setScale(scale);
                    param.setUid(Guid.NewGuid().ToString());
                    param.setMeta(string.Empty);
                    var targetOptional = ImageTarget.createFromParameters(param);
                    if (targetOptional.OnSome)
                    {
                        var target = targetOptional.Value;
                        var go = new GameObject(name + " <" + i + ">", typeof(ImageTargetController));
                        var controller = go.GetComponent<ImageTargetController>();

                        HandleTargetControllerEvents(controller);
                        controller.TargetDataLoad += (_) => target.Dispose();
                        controller.Source = new ImageTargetController.TargetSourceData
                        {
                            Target = target
                        };
                        controller.Tracker = imageTracker;

                        var panda = Instantiate(Panda);
                        panda.transform.SetParent(controller.transform, false);
                    }
                    else
                    {
                        throw new Exception("invalid parameter");
                    }
                }
            }
        }

        private void HandleTargetControllerEvents(ImageTargetController controller)
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

        private void HandleTrackerEvents(ImageTrackerFrameFilter tracker)
        {
            tracker.TargetLoad += (controller, status) =>
            {
                if (!controller) { return; }
                Debug.Log($"Load target {{id = {controller.Target.runtimeID()}, name = {controller.Target.name()}, size = {controller.Size}}} into {tracker.name} => {status}");
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
