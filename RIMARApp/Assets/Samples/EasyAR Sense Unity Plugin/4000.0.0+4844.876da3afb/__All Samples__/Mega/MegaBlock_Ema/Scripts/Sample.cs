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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Networking;

#if EASYAR_ENABLE_MEGA
using EasyAR.Mega.Ema.Unity;
using EasyAR.Mega.Scene;
using System.Linq;
#endif

namespace Sample
{
    public class Sample : MonoBehaviour
    {
        public ARSession Session;
        public GameObject MsgBox;
        public GameObject Panda;
        public string EmaInStreamingAssets;

        private MegaTrackerFrameFilter megaTracker = null;
        private Material urpDefaultMaterial;

        private void Awake()
        {
            AdaptInputSystem();
#if !EASYAR_ENABLE_MEGA
            ShowMessage(1000, "package com.easyar.mega is required to use EasyAR Mega", "");
            Session.AutoStart = false;
#else
            megaTracker = Session.GetComponentInChildren<MegaTrackerFrameFilter>();
            MsgBox.SetActive(false);
#endif
        }

        private void Start()
        {
            // read ema file. Please put your ema to Assets/StreamingAssets or elsewhere you can read in scripts
            string emaAssetPath = Application.streamingAssetsPath + "/" + EmaInStreamingAssets;
            StartCoroutine(LoadEmaFile(emaAssetPath));
        }

        private IEnumerator LoadEmaFile(string path)
        {
            string filePath = String.Empty;
            if (string.IsNullOrEmpty(path) || path.StartsWith("jar:file://") || path.StartsWith("file://"))
            {
                filePath = path;
            }
            else if (Application.platform == RuntimePlatform.OSXEditor ||
                     Application.platform == RuntimePlatform.OSXPlayer ||
                     Application.platform == RuntimePlatform.IPhonePlayer ||
                     Application.platform == RuntimePlatform.Android
#if UNITY_VISIONOS
                     || Application.platform == RuntimePlatform.VisionOS
#endif
                )
            {
                filePath = "file://" + path;
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                filePath = "file:///" + path;
            }

            using (var request = UnityWebRequest.Get(filePath))
            {
                yield return request.SendWebRequest();
                while (!request.isDone)
                {
                    yield return 0;
                }
                if (String.IsNullOrEmpty(request.downloadHandler.text))
                {
                    ShowMessage(20, $"Fail to load file {path}: text is empty", string.Empty);
                    yield break;
                }
                try
                {
                    LoadEma(request.downloadHandler.text);
                }
                catch (Exception e)
                {
                    ShowMessage(20, e.Message, string.Empty);
                    throw;
                }
            }
        }

        private void LoadEma(string emaData)
        {
#if EASYAR_ENABLE_MEGA
            var emaDecoded = EasyAR.Mega.Ema.EmaDecoder.Decode(emaData);
            if (!(emaDecoded is EasyAR.Mega.Ema.v0_5.Ema ema))
            {
                var message = $"ema {emaDecoded.Version} usage is not written in the sample";
                ShowMessage(10, message, string.Empty);
                throw new InvalidOperationException(message);
            }
            var blockHolder = megaTracker.BlockHolder;

            foreach (var item in ema.blocks)
            {
                var info = new BlockController.BlockInfo { ID = item.id.ToString(), Timestamp = item.timestamp };
                if (!item.keepTransform && item.location.OnSome)
                {
                    blockHolder.Hold(info, item.location.Value);
                }
                else
                {
                    blockHolder.Hold(info, item.transform.ToUnity());
                }
            }

            var nodes = ema.annotations.Where(a => (a is EasyAR.Mega.Ema.v0_5.Node node) && (node.geometry == "cube" || node.geometry == "point")).Select(a => a as EasyAR.Mega.Ema.v0_5.Node);
            foreach (var node in nodes)
            {
                var name = node.Name.OnSome ? node.Name.Value : string.Empty;
                var info = new AnnotationNode.AnnotationNodeInfo
                {
                    ID = node.id.ToString(),
                    Timestamp = node.timestamp,
                    Geometry = node.geometry == "cube" ? AnnotationNode.GeometryType.Cube : AnnotationNode.GeometryType.Point,
                    FeatureType = node.featureType.OnSome ? node.featureType.Value : null
                };

                // here is where the "cube" or "sphere" in the sample is created
                GameObject placeholder = null;
                if (info.Geometry == AnnotationNode.GeometryType.Cube)
                {
                    placeholder = new GameObject();
                    var cube = CreatePrimitiveGameObject(PrimitiveType.Cube);
                    cube.transform.SetParent(placeholder.transform, false);
                    var panda = Instantiate(Panda);
                    panda.transform.SetParent(placeholder.transform, false);
                    panda.transform.localPosition = new Vector3(0, -0.5f, 0);
                }
                else if (info.Geometry == AnnotationNode.GeometryType.Point)
                {
                    placeholder = new GameObject();
                    var sphere = CreatePrimitiveGameObject(PrimitiveType.Sphere);
                    sphere.transform.SetParent(placeholder.transform, false);
                    sphere.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    var panda = Instantiate(Panda);
                    panda.transform.SetParent(placeholder.transform, false);
                    panda.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                }

                if (node.parent is EasyAR.Mega.Ema.v0_5.Node.WorldParent wp)
                {
                    AnnotationNode.Setup(placeholder, blockHolder.BlockRoot, info, node.transform.ToUnity(), wp.location);
                }
                else if (node.parent is EasyAR.Mega.Ema.v0_5.Node.BlockParent bp)
                {
                    AnnotationNode.Setup(placeholder, blockHolder.GetBlock(bp.id.ToString()), info, node.transform.ToUnity());
                }
            }
            ShowMessage(20, $"ema {ema.Version} loaded. {nodes.Count()} cube/point placeholders are created in runtime to show ema nodes.\nYou should make your own model and put it directly into your app to replace these objects.", string.Empty);
#endif
        }

        private void ShowMessage(int time, string messageToDeveloper, string messageToUser)
        {
            StopAllCoroutines();
            MsgBox.GetComponentInChildren<Text>().text =
                "Message for developer:" + Environment.NewLine + messageToDeveloper + Environment.NewLine
                + Environment.NewLine +
                "Message for user:" + Environment.NewLine + messageToUser;
            if (!MsgBox.activeSelf) { MsgBox.SetActive(true); }
            StartCoroutine(ClearMessage(time));
        }

        private IEnumerator ClearMessage(int time)
        {
            yield return new WaitForSeconds(time);
            MsgBox.GetComponentInChildren<Text>().text = string.Empty;
            if (MsgBox.activeSelf) { MsgBox.SetActive(false); }
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

        private GameObject CreatePrimitiveGameObject(PrimitiveType primitiveType)
        {
            var go = GameObject.CreatePrimitive(primitiveType);
#if EASYAR_URP_ENABLE
            if (GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)
            {
                if (!urpDefaultMaterial)
                {
                    urpDefaultMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                }
                go.GetComponent<MeshRenderer>().material = urpDefaultMaterial;
            }
#endif
            return go;
        }
    }
}
