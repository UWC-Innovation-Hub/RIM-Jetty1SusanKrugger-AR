//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEngine;
using UnityEngine.Rendering;
#if EASYAR_URP_ENABLE
using UnityEngine.Rendering.Universal;
#if EASYAR_URP_17_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
#else
using ScriptableRendererFeature = UnityEngine.ScriptableObject;
#endif

namespace Sample
{
    public class SampleOneShotRendererFeature : ScriptableRendererFeature
    {
        public static bool IsActive { get; private set; }
#if EASYAR_URP_ENABLE
        CameraImageRenderPass renderPass;

#if EASYAR_URP_17_OR_NEWER
        easyar.Optional<RTHandleSystem> rtHandleSystem;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (rtHandleSystem.OnSome)
                {
                    rtHandleSystem.Value.Dispose();
                    rtHandleSystem = null;
                }
            }
        }
#endif
        public override void Create()
        {
            IsActive = true;
            renderPass = new CameraImageRenderPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;
            if (!camera) { return; }
            var oneshot = camera.GetComponent<OneShot>();
            if (!oneshot) { return; }

#if EASYAR_URP_17_OR_NEWER
            if (rtHandleSystem.OnNone)
            {
                rtHandleSystem = new RTHandleSystem();
                rtHandleSystem.Value.Initialize(Screen.width, Screen.height);
            }
            renderPass.SetupRTHandleSystem(rtHandleSystem.Value);
            renderPass.Setup(oneshot);
            //For Compatibility Mode Only
#pragma warning disable 618, 672
            renderPass.SetupCameraColorTarget(renderer.cameraColorTargetHandle);
#pragma warning restore 618, 672
#else
            renderPass.Setup(oneshot,
#if EASYAR_URP_13_1_OR_NEWER
#pragma warning disable 618, 672
                renderer.cameraColorTargetHandle
#pragma warning restore 618, 672
#else
                renderer.cameraColorTarget
#endif
                );
#endif
            renderer.EnqueuePass(renderPass);
        }

        class CameraImageRenderPass : ScriptableRenderPass
        {
            OneShot oneshot;
            RenderTargetIdentifier colorTarget;
            public CameraImageRenderPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            }
#if EASYAR_URP_17_OR_NEWER
            const string kRenderGraphDisablePassName = "One Shot Render Pass (Render Graph Disabled)";
            const string kRenderGraphEnablePassName  = "One Shot Render Pass (Render Graph Enabled)";

            PassData renderPassData = new PassData();
            easyar.Optional<RTHandleSystem> rtHandleSystem;

            public void Setup(OneShot shot) => oneshot = shot;

            public void SetupCameraColorTarget(RenderTargetIdentifier colorTarget) => this.colorTarget = colorTarget;

            public void SetupRTHandleSystem(RTHandleSystem system) => rtHandleSystem = system;

            static void ExecuteRasterRenderGraphPass(PassData passData, RasterGraphContext rasterContext)
            {
                if (passData.oneshot.destTexture.OnNone)
                {
                    return;
                }
                var destTexture = passData.oneshot.destTexture.Value;
                if (passData.oneshot.mirror)
                {
                    //flip vertical
                    Blitter.BlitTexture2D(rasterContext.cmd, passData.src, new Vector4(1.0f, -1.0f, 0.0f, 1.0f), 0, true);
                }
                else
                {
                    Blitter.BlitTexture2D(rasterContext.cmd, passData.src, new Vector4(1.0f, 1.0f, 0.0f, 0.0f), 0, true);
                }
                AsyncGPUReadback.Request(destTexture, 0, TextureFormat.RGB24, request =>
                {
                    if (request.hasError)
                    {
                        Debug.LogWarning("GPU readback error at OneShot Sampling.");
                        return;
                    }

                    var texture = new Texture2D(destTexture.width, destTexture.height, TextureFormat.RGB24, false);
                    texture.LoadRawTextureData(request.GetData<byte>());
                    texture.Apply();

                    passData.oneshot.callback?.Invoke(texture);
                    Destroy(destTexture);
                    passData.oneshot.destTexture = null;
                });
            }
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass(kRenderGraphEnablePassName, out renderPassData))
                {
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                    renderPassData.oneshot = oneshot;
                    renderPassData.src = resourceData.activeColorTexture;

                    if (rtHandleSystem.OnNone || oneshot.destTexture.OnNone)
                    {
                        return;
                    }
                    RTHandle destinationRtHandle = rtHandleSystem.Value.Alloc(oneshot.destTexture.Value);
                    renderPassData.dst = renderGraph.ImportTexture(destinationRtHandle);

                    builder.UseTexture(renderPassData.src);
                    builder.SetRenderAttachment(renderPassData.dst, 0);
                    builder.SetRenderFunc<PassData>(ExecuteRasterRenderGraphPass);
                }
            }

            // For Compatibility Mode Only
#pragma warning disable 618, 672
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                renderPassData.oneshot = oneshot;

                var destTexture = new RenderTexture(Screen.width, Screen.height, 0);
                var cmd = CommandBufferPool.Get(kRenderGraphDisablePassName);
                if (oneshot.mirror)
                {
                    cmd.Blit(colorTarget, destTexture, oneshot.material);
                }
                else
                {
                    cmd.Blit(colorTarget, destTexture);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                context.Submit();

                RenderTexture.active = destTexture;
                var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                texture.Apply();
                RenderTexture.active = null;
                Destroy(destTexture);

                oneshot.callback(texture);
                Destroy(oneshot);
            }
#pragma warning restore 618, 672

            class PassData
            {
                internal OneShot oneshot;
                internal TextureHandle src;
                internal TextureHandle dst;
            }
        }
#else
            public void Setup(OneShot shot, RenderTargetIdentifier color)
            {
                oneshot = shot;
                colorTarget = color;
            }

#pragma warning disable 618, 672
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var destTexture = new RenderTexture(Screen.width, Screen.height, 0);
                var cmd = CommandBufferPool.Get();
                if (oneshot.mirror)
                {
                    cmd.Blit(colorTarget, destTexture, oneshot.material);
                }
                else
                {
                    cmd.Blit(colorTarget, destTexture);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                context.Submit();

                RenderTexture.active = destTexture;
                var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                texture.Apply();
                RenderTexture.active = null;
                Destroy(destTexture);

                oneshot.callback(texture);
                Destroy(oneshot);
            }
#pragma warning restore 618, 672

            public override void FrameCleanup(CommandBuffer commandBuffer)
            {
            }
        }
#endif
#endif
    }
}
