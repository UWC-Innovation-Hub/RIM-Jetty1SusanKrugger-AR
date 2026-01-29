//=============================================================================================================================
//
// Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
// EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
// and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//=============================================================================================================================

using easyar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImageTracking_TargetOnTheFly
{
    public class TargetOnTheFlySample : MonoBehaviour
    {
        public ARSession Session;
        public ImageTrackerFrameFilter Filter;
        public GameObject Panda;

        private List<ImageTargetController> targets = new List<ImageTargetController>();

        private void Awake()
        {
            AdaptInputSystem();
        }

        public void CreateTarget()
        {
            StartCoroutine(CaptureAndCreateTarget());
        }

        public void ClearAllTarget()
        {
            StopAllCoroutines();
            foreach (var target in targets) { Destroy(target.gameObject); }
            targets = new List<ImageTargetController>();
        }

        private IEnumerator CaptureAndCreateTarget()
        {
            yield return new WaitForEndOfFrame();

            var size = new Vector2Int((int)(Screen.width * 0.6f), (int)(Screen.height * 0.5f));
            var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(Screen.width * 0.2f, Screen.height * 0.2f, size.x, size.y), 0, 0, false);
            texture.Apply();

            var target = CreateImageTarget(Instantiate(texture));

            targets.Add(target);
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.GetComponent<MeshRenderer>().material.mainTexture = texture;
            quad.transform.SetParent(target.transform, false);
            quad.transform.localScale = new Vector3(1, (float)size.y / size.x, 1);
            var panda = Instantiate(Panda);
            panda.transform.SetParent(target.transform, false);
        }

        private ImageTargetController CreateImageTarget(Texture2D texture)
        {
            var targetObject = ARSessionFactory.CreateController<ImageTargetController>();
            var controller = targetObject.GetComponent<ImageTargetController>();
            controller.TargetDataLoad += (_) => Destroy(texture);
            controller.Source = new ImageTargetController.Texture2DSourceData
            {
                Texture = texture
            };
            controller.Tracker = Filter;
            return controller;
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
