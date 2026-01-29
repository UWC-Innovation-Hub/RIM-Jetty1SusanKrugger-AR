//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ImageTracking_CloudRecognition
{
    public class CloudRecognizerSample : MonoBehaviour
    {
        public Text Status;
        public ARSession Session;
        public bool UseOfflineCache = true;
        public string OfflineCachePath;
        public GameObject Panda;

        private CloudRecognizerFrameFilter cloudRecognizer;
        private ImageTrackerFrameFilter tracker;
        private List<GameObject> targetObjs = new List<GameObject>();
        private List<string> loadedCloudTargetUids = new List<string>();
        private List<ImageMaterial> imageMaterials = new();
        private int cachedTargetCount;
        private ResolveInfo resolveInfo;
        private float resolveInterval = 1f;
        private bool isTracking;
        private bool resolveOn;
        private int minResolveTimeout = 3000;
        private int maxResolveTimeout = 20000;
        private int requestTimeout = 3000;

        private void Awake()
        {
            AdaptInputSystem();
            tracker = Session.GetComponentInChildren<ImageTrackerFrameFilter>();
            cloudRecognizer = Session.GetComponentInChildren<CloudRecognizerFrameFilter>();

            tracker.TargetLoad += (target, result) =>
            {
                if (!target) { return; }
                if (!result)
                {
                    var targetStr = string.Empty;
                    if (target.Source is ImageTargetController.TargetSourceData targetSource)
                    {
                        targetStr = targetSource.Target?.uid();
                    }
                    else if (target.Source is ImageTargetController.TargetDataFileSourceData targetDataFileSource)
                    {
                        targetStr = targetDataFileSource.Path;
                    }
                    Debug.LogError($"target data {targetStr} load failed");
                    return;
                }

                if (target.Source is not ImageTargetController.TargetSourceData)
                {
                    loadedCloudTargetUids.Add(target.Target.uid());
                    cachedTargetCount++;
                }
                AddCubeOnTarget(target);
            };

            if (UseOfflineCache)
            {
                if (string.IsNullOrEmpty(OfflineCachePath))
                {
                    OfflineCachePath = Application.persistentDataPath + "/CloudRecognizerSample";
                }
                if (!Directory.Exists(OfflineCachePath))
                {
                    Directory.CreateDirectory(OfflineCachePath);
                }
                if (Directory.Exists(OfflineCachePath))
                {
                    foreach (var file in Directory.GetFiles(OfflineCachePath).Where(f => Path.GetExtension(f) == ".etd"))
                    {
                        LoadOfflineTarget(file);
                    }
                }
            }

            StartAutoResolve();
        }

        private void Update()
        {
            Status.text =
                (requestTimeout == maxResolveTimeout ? $"Your network condition is possibility not good, resolve timeout has been set to {maxResolveTimeout}" + Environment.NewLine + Environment.NewLine : "") +
                "Resolve: " + ((resolveInfo == null || resolveInfo.Timestamp == 0 || isTracking) ? "OFF" : "ON") + Environment.NewLine +
                "CachedTargets: " + cachedTargetCount + Environment.NewLine +
                "LoadedTargets: " + loadedCloudTargetUids.Count;

            AutoResolve();
        }

        private void OnDestroy()
        {
            foreach (var obj in targetObjs) { Destroy(obj); }
            foreach (var mat in imageMaterials) { mat.Dispose(); }
        }

        public void ClearAll()
        {
            if (Directory.Exists(OfflineCachePath))
            {
                var targetFiles = Directory.GetFiles(OfflineCachePath);
                foreach (var file in Directory.GetFiles(OfflineCachePath).Where(f => Path.GetExtension(f) == ".etd"))
                {
                    File.Delete(file);
                }
            }
            foreach (var obj in targetObjs) { Destroy(obj); }
            targetObjs.Clear();
            loadedCloudTargetUids.Clear();
            cachedTargetCount = 0;
        }

        public void StartAutoResolve()
        {
            resolveOn = true;
            resolveInfo = new ResolveInfo();
            requestTimeout = minResolveTimeout;
        }

        public void StopResolve()
        {
            resolveInfo = null;
            resolveOn = false;
        }

        private void AutoResolve()
        {
            var time = Time.realtimeSinceStartup;
            if (!resolveOn || isTracking || resolveInfo.Running || time - resolveInfo.ResolveTime < resolveInterval) { return; }

            resolveInfo.Running = true;
            resolveInfo.ResolveTime = time;

            cloudRecognizer.Resolve(requestTimeout, (result) =>
            {
                if (resolveInfo == null)
                {
                    return;
                }
                resolveInfo.Running = false;

                resolveInfo.CostTime = result.TotalResponseDuration;
                resolveInfo.Timestamp = result.Timestamp;
                resolveInfo.CloudStatus = result.Status;

                resolveInfo.SlowResponseCount = resolveInfo.CloudStatus == CloudRecognizationStatus.UnknownError && resolveInfo.CostTime > requestTimeout / 1000f ? resolveInfo.SlowResponseCount + 1 : 0;
                resolveInfo.FastResponseCount = resolveInfo.CostTime < minResolveTimeout / 1000f ? resolveInfo.FastResponseCount + 1 : 0;
                if (resolveInfo.SlowResponseCount == 3 && requestTimeout < maxResolveTimeout)
                {
                    requestTimeout = maxResolveTimeout;
                    resolveInfo.SlowResponseCount = 0;
                    resolveInfo.FastResponseCount = 0;
                }
                if (resolveInfo.FastResponseCount == 5 && requestTimeout > minResolveTimeout)
                {
                    requestTimeout = minResolveTimeout;
                    resolveInfo.SlowResponseCount = 0;
                    resolveInfo.FastResponseCount = 0;
                }

                var target = result.Target;
                if (target.OnSome)
                {
                    var targetValue = target.Value;

                    if (!loadedCloudTargetUids.Contains(targetValue.uid()))
                    {
                        LoadCloudTarget(targetValue.Clone());
                    }
                }
            });
        }

        private void LoadCloudTarget(ImageTarget target)
        {
            if (UseOfflineCache && Directory.Exists(OfflineCachePath))
            {
                if (target.save(Path.Combine(OfflineCachePath, target.uid() + ".etd")))
                {
                    cachedTargetCount++;
                }
            }

            var uid = target.uid();
            loadedCloudTargetUids.Add(uid);
            var go = new GameObject(uid);
            targetObjs.Add(go);
            var targetController = go.AddComponent<ImageTargetController>();
            targetController.TargetDataLoad += (_) => target.Dispose();
            targetController.Source = new ImageTargetController.TargetSourceData
            {
                Target = target
            };
            LoadTargetIntoTracker(targetController);
        }

        private void LoadOfflineTarget(string file)
        {
            var go = new GameObject(Path.GetFileNameWithoutExtension(file) + "-offline");
            targetObjs.Add(go);
            var targetController = go.AddComponent<ImageTargetController>();
            targetController.Source = new ImageTargetController.TargetDataFileSourceData
            {
                PathType = PathType.Absolute,
                Path = file,
            };
            LoadTargetIntoTracker(targetController);
        }

        private void LoadTargetIntoTracker(ImageTargetController controller)
        {
            controller.Tracker = tracker;
            controller.TargetFound += () =>
            {
                isTracking = true;
            };
            controller.TargetLost += () =>
            {
                isTracking = false;
            };
        }

        private void AddCubeOnTarget(ImageTargetController controller)
        {
            var panda = Instantiate(Panda);
            panda.transform.SetParent(controller.transform, false);

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(controller.transform, false);
            quad.transform.localScale = new Vector3(1f, 1f / controller.Target.aspectRatio(), 0.02f);

            var imageList = controller.Target.images();
            if (imageList.Count > 0)
            {
                var imageMaterial = new ImageMaterial(imageList[0]);
                quad.GetComponent<MeshRenderer>().material = imageMaterial.Material;
                imageMaterials.Add(imageMaterial);
            }
            foreach (var image in imageList)
            {
                image.Dispose();
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

        private class ResolveInfo
        {
            public double Timestamp;
            public bool Running = false;
            public float ResolveTime = 0;
            public float CostTime = 0;
            public CloudRecognizationStatus CloudStatus = CloudRecognizationStatus.UnknownError;
            public int SlowResponseCount = 0;
            public int FastResponseCount = 0;
        }
    }
}
