//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Sample
{
    public class Sample : MonoBehaviour
    {
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
