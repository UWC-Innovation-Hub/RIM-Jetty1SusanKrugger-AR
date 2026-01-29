//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using UnityEngine;

namespace Sample
{
    public class OneShot : MonoBehaviour
    {
        internal bool mirror;
        internal Action<Texture2D> callback;
        internal bool capturing;
        internal Material material;
#if EASYAR_URP_17_OR_NEWER
        internal easyar.Optional<RenderTexture> destTexture;
#endif

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            if (!capturing) { return; }

            var destTexture = new RenderTexture(Screen.width, Screen.height, 0);
            if (mirror)
            {
                material.mainTexture = source;
                Graphics.Blit(null, destTexture, material);
            }
            else
            {
                Graphics.Blit(source, destTexture);
            }

            RenderTexture.active = destTexture;
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            Destroy(destTexture);

            callback(texture);
            Destroy(this);
        }

        public void Shot(Material material, bool mirror, Action<Texture2D> callback)
        {
            if (callback == null) { return; }
            this.material = material;
            this.mirror = mirror;
            this.callback = callback;
#if EASYAR_URP_17_OR_NEWER
            this.destTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
#endif
            capturing = true;
        }
    }
}
