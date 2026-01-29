//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace easyar
{
    [RequireComponent(typeof(UnityEngine.Video.VideoPlayer))]
    public class CustomVideoFrameSource : ExternalImageStreamFrameSource
    {
        private UnityEngine.Video.VideoPlayer player;
        CameraParameters cameraParameters;
        RenderTexture renderTexture;
        private long frameIndex = -1;
        private bool started;
        private FrameSourceCamera deviceCamera;

        protected override bool IsHMD => false;
        protected override Camera Camera => Camera.main;
        protected override bool IsCameraUnderControl => true;
        protected override IDisplay Display => easyar.Display.DefaultSystemDisplay;
        protected override Optional<bool> IsAvailable => true;
        protected override bool CameraFrameStarted => started;
        protected override List<FrameSourceCamera> DeviceCameras => new List<FrameSourceCamera> { deviceCamera };

        protected override void Awake()
        {
            base.Awake();
            player = GetComponent<UnityEngine.Video.VideoPlayer>();
        }

        protected override void OnSessionStart(ARSession session)
        {
            base.OnSessionStart(session);

            // NOTICE: The parameters (intrinsics) bellow can be used only with the same video (Pixel2_ARCore_Portrait.mp4).
            //         The video is captured using ARCore on Pixel2 using camera callback (important: not screen recording).
            //         The intrinsics is captured in the same time.
            //         Make sure to calibrate your camera or video frames if you want to use another video or device as frame source.
            //         For the fundamental of camera calibration, you need to learn it from other resources.
            //         Try searches like https://www.bing.com/search?q=camera+calibration
            var size = new Vector2Int(640, 360);
            var cameraType = CameraDeviceType.Back;
            var cameraOrientation = 90;
            cameraParameters = new CameraParameters(size.ToEasyARVector(), new Vec2F(506.085f, 505.3105f), new Vec2F(318.1032f, 177.6514f), cameraType, cameraOrientation);
            deviceCamera = new FrameSourceCamera(cameraType, cameraOrientation, size, new Vector2(30, 30));
            started = true;
            player.Play();
            StartCoroutine(VideoDataToInputFrames());
        }

        protected override void OnSessionStop()
        {
            base.OnSessionStop();

            StopAllCoroutines();
            player.Stop();
            if (renderTexture) { Destroy(renderTexture); }
            cameraParameters?.Dispose();
            cameraParameters = null;
            frameIndex = -1;
            started = false;
            deviceCamera?.Dispose();
            deviceCamera = null;
        }

        private IEnumerator VideoDataToInputFrames()
        {
            yield return new WaitUntil(() => player.isPrepared);

            var pixelSize = new Vector2Int((int)player.width, (int)player.height);
            renderTexture = new RenderTexture(pixelSize.x, pixelSize.y, 0);
            player.targetTexture = renderTexture;

            yield return new WaitUntil(() => player.isPlaying && player.frame >= 0);
            while (true)
            {
                yield return null;
                if (frameIndex == player.frame) { continue; }
                frameIndex = player.frame;

                RenderTexture.active = renderTexture;
                var texture = new Texture2D(pixelSize.x, pixelSize.y, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, pixelSize.x, pixelSize.y), 0, 0);
                texture.Apply();
                RenderTexture.active = null;

                var pixelFormat = PixelFormat.RGB888;
                var bufferO = TryAcquireBuffer(pixelSize.x * pixelSize.y * 3);
                if (bufferO.OnNone) { continue; }

                var buffer = bufferO.Value;
                CopyRawTextureData(buffer, texture.GetRawTextureData<byte>(), pixelSize);

                using (buffer)
                using (var image = Image.create(buffer, pixelFormat, pixelSize.x, pixelSize.y, pixelSize.x, pixelSize.y))
                {
                    HandleCameraFrameData(player.time, image, cameraParameters);
                }
            }
        }

        private static unsafe void CopyRawTextureData(Buffer buffer, Unity.Collections.NativeArray<byte> data, Vector2Int size)
        {
            int oneLineLength = size.x * 3;
            int totalLength = oneLineLength * size.y;
            var ptr = new IntPtr(data.GetUnsafeReadOnlyPtr());
            for (int i = 0; i < size.y; i++)
            {
                buffer.tryCopyFrom(ptr, oneLineLength * i, totalLength - oneLineLength * (i + 1), oneLineLength);
            }
        }
    }
}
