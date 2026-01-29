//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AllSamplesLauncher
{
    public class SampleLauncher : MonoBehaviour
    {
        public GameObject SamplesPanel;
        public GameObject BackCanvas;
        public Button BackButton;
        public Button ButtonSamples;

        private static bool hasLoaded;
        private static string buttonName;
        private List<Button> categoryButtons = new List<Button>();
        private List<string> arfoundationScenes = new List<string>();
        private readonly List<(string, List<(string, string)>)> samples = new List<(string, List<(string, string)>)> {
            ("Device Workflow", new List<(string, string)>()),
            ("Workflow", new List<(string, string)> {
                ("Workflow_ARSession", "fundamental session workflow, frame sources, and availability detection"),
                ("Workflow_FrameSource_CameraDevice", "frame source for object sensing"),
                ("Workflow_FrameSource_ExternalImageStream", "custom camera using video file"),
            }),
            ("Head Mounted Display", new List<(string, string)> {
                ("Combination_BasedOn_AppleVisionPro_ (built-in)", "run the scene on device directly"),
                ("Combination_BasedOn_XREAL_ (built-in)", "run the scene on device directly"),
                (":: Import Other Sample(s) From Extention Package(s)", ""),
                ("Combination_BasedOn_HMD_", "it is a template, reference for device manufacturer, not directly runnable"),
                ("Combination_BasedOn_Pico_", "run the scene on device directly"),
                ("Combination_BasedOn_Rokid_", "run the scene on device directly"),
                ("Xrany and some others", "some extensions are maintained by the device manufacturer"),
                ("Not see your device?", "refernece the HMD template to build your extension, or contact EasyAR sales agent"),
            }),

            ("World Sensing", new List<(string, string)>()),
            ("Mega", new List<(string, string)> {
                ("MegaBlock_Basic", "the first sample to use Mega Block"),
                ("MegaBlock_Ema", "ema importation (advanced usage for giant apps)"),
                ("MegaLandmark_Basic", "Mega Landmark"),
            }),
            ("SpatialMap", new List<(string, string)> {
                ("SpatialMap_Sparse_AllInOne", "full workflow of sparse spatial map"),
                ("SpatialMap_Dense_BallGame", "small game using dense spatial map"),
                ("SpatialMap_Sparse_Building", ""),
                ("SpatialMap_Sparse_Localizing", ""),
            }),
            ("MotionTracking", new List<(string, string)> {
                ("MotionTracking_DeviceMotionAndPlaneDetection", "track device supported by MotionTracker/ARKit/ARCore/AREngine; detect plane on MotionTracker available device"),
            }),
            ("SurfaceTracking", new List<(string, string)> {
                ("SurfaceTracking", "if you are looking for ARKit replacement, you need MotionTracking instead of SurfaceTracking"),
            }),

            ("Object Sensing", new List<(string, string)>()),
            ("ImageTracking", new List<(string, string)> {
                ("ImageTracking_Targets", "the first sample to use image tracking"),
                ("ImageTracking_MotionFusion", "tracking when image out of scope"),
                ("ImageTracking_CloudRecognition", "EasyAR CRS (EasyAR cloud service required)"),
                ("ImageTracking_Video", "track image and playback video in 3D space using Unity video player"),
                ("ImageTracking_TargetOnTheFly", "create target in runtime, no disk I/O or service required"),
                ("ImageTracking_Coloring3D", "the color book"),
                ("ImageTracking_MultiTarget_SingleTracker", "track multiple images with only one tracker"),
                ("ImageTracking_MultiTarget_MultiTracker", "track multiple images with many trackers"),
                ("ImageTracking_MultiTarget_SameImage", "track multiple same images"),
            }),
            ("ObjectTracking", new List<(string, string)> {
                ("ObjectTracking", "rich texture object"),
            }),

            ("Extra", new List<(string, string)>()),
            ("Combination", new List<(string, string)> {
                ("Combination_BasedOn_MotionTracking", "features when motion tracking exist"),
                ("Combination_SpatialMap_Sparse_Dense", "build sparse and dense spatial map in the same time"),
                ("Combination_SpatialMap_ImageTracking", ""),
                ("Combination_ObjectTracking_ImageTracking", ""),
                ("Try Your Own Combination", "you can always combine features by putting them in the same session"),
            }),
            ("VideoRecording", new List<(string, string)> {
                ("VideoRecording", "only work in a few configurations"),
            }),
        };

        private void Awake()
        {
            AdaptInputSystem();
        }

        private void Start()
        {
            if (hasLoaded)
            {
                ButtonSamples.onClick.Invoke();
            }
            hasLoaded = true;
            // most handheld samples based on motion tracking (device slam) are made AR Foundation usable
            var motionTrackingCategories = new List<string> { "Mega", "SpatialMap", "MotionTracking", "MotionFusion" };
            arfoundationScenes = samples.Where(s => motionTrackingCategories.Contains(s.Item1)).SelectMany(s => s.Item2.Select(ss => ss.Item1))
                .Concat(samples.Where(s => new List<string> { "Combination", "ImageTracking" }.Contains(s.Item1)).SelectMany(s => s.Item2.Where(i => motionTrackingCategories.Where(c => i.Item1.Contains(c)).Any()).Select(ss => ss.Item1)))
                .Append("Workflow_ARSession")
                .ToList();
            // and there are a few samples that demonstrate a simple scene without AR Foundation
            arfoundationScenes.Remove("SpatialMap_Sparse_Building");
            arfoundationScenes.Remove("SpatialMap_Sparse_Localizing");

            SetupCanvas();
            SetupBackCanvas();

            Button button = categoryButtons.SingleOrDefault(b => b.name == buttonName) ?? categoryButtons[0];
            button.onClick.Invoke();
        }

        private void SetupCanvas()
        {
            var panelL = DefaultControls.CreatePanel(new DefaultControls.Resources());
            panelL.transform.SetParent(SamplesPanel.transform, false);
            var rectL = panelL.GetComponent<RectTransform>();
            rectL.anchorMin = new Vector2(0, 0);
            rectL.anchorMax = new Vector2(0.4f, 1);
            rectL.offsetMin = Vector2.zero;
            rectL.offsetMax = Vector2.zero;
            panelL.GetComponent<Image>().color = new Color32(206, 206, 206, 146);

            var panelR = DefaultControls.CreatePanel(new DefaultControls.Resources());
            panelR.transform.SetParent(SamplesPanel.transform, false);
            var rectR = panelR.GetComponent<RectTransform>();
            rectR.anchorMin = new Vector2(0.4f, 0);
            rectR.anchorMax = new Vector2(1, 1);
            rectR.offsetMin = Vector2.zero;
            rectR.offsetMax = Vector2.zero;
            panelR.GetComponent<Image>().color = new Color32(255, 255, 255, 51);

            var titles = samples.Where(s => s.Item2.Count <= 0).Select(s => s.Item1);
            var categories = samples.Where(s => s.Item2.Count > 0);
            var titleAnchorX = new Vector2(0, 1);
            var categoryAnchorX = new Vector2(0.1f, 0.9f);
            var space = 0.01f;
            var titleH = 0.05f;
            var categoryH = (1 - titleH * titles.Count() - space * samples.Count) / categories.Count();

            var buttons = new List<(Button, GameObject)>();
            var yMax = 1f;
            foreach (var item in samples)
            {
                Vector2 anchorY;
                if (item.Item2.Count <= 0)
                {
                    var panel = DefaultControls.CreatePanel(new DefaultControls.Resources());
                    panel.transform.SetParent(panelL.transform, false);
                    panel.name = item.Item1;

                    var rectT = panel.GetComponent<RectTransform>();
                    anchorY = new Vector2(yMax - titleH, yMax);
                    rectT.anchorMin = new Vector2(titleAnchorX.x, anchorY.x);
                    rectT.anchorMax = new Vector2(titleAnchorX.y, anchorY.y);
                    rectT.offsetMin = Vector2.zero;
                    rectT.offsetMax = Vector2.zero;

                    var text = DefaultControls.CreateText(new DefaultControls.Resources()).GetComponent<Text>();
                    text.transform.SetParent(panel.transform, false);
                    text.resizeTextForBestFit = true;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.text = item.Item1;

                    var textRectT = text.GetComponent<RectTransform>();
                    textRectT.anchorMin = new Vector2(0.05f, 0.05f);
                    textRectT.anchorMax = new Vector2(0.95f, 0.95f);
                    textRectT.offsetMin = Vector2.zero;
                    textRectT.offsetMax = Vector2.zero;
                }
                else
                {
                    var button = DefaultControls.CreateButton(new DefaultControls.Resources()).GetComponent<Button>();
                    button.transform.SetParent(panelL.transform, false);
                    button.name = item.Item1;

                    var rectT = button.GetComponent<RectTransform>();
                    anchorY = new Vector2(yMax - categoryH, yMax);
                    rectT.anchorMin = new Vector2(categoryAnchorX.x, anchorY.x);
                    rectT.anchorMax = new Vector2(categoryAnchorX.y, anchorY.y);
                    rectT.offsetMin = Vector2.zero;
                    rectT.offsetMax = Vector2.zero;

                    var text = button.GetComponentInChildren<Text>();
                    text.resizeTextForBestFit = true;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.text = item.Item1;

                    var textRectT = text.GetComponent<RectTransform>();
                    textRectT.anchorMin = new Vector2(0.05f, 0.05f);
                    textRectT.anchorMax = new Vector2(0.95f, 0.95f);
                    textRectT.offsetMin = Vector2.zero;
                    textRectT.offsetMax = Vector2.zero;

                    categoryButtons.Add(button);

                    var panel = SetupRightPanel(panelR, item.Item1, item.Item2);
                    buttons.Add((button, panel));
                }

                yMax = anchorY.x - space;
            }

            var panels = buttons.Select(b => b.Item2);
            foreach (var button in buttons)
            {
                button.Item1.onClick.AddListener(() =>
                {
                    button.Item2.SetActive(true);
                    foreach (var panel in panels.Where(p => p != button.Item2))
                    {
                        panel.SetActive(false);
                    }

                    Button recordButton = categoryButtons.Where(b => b.name == buttonName).SingleOrDefault();
                    if (recordButton)
                    {
                        recordButton.targetGraphic.color = new Color(1, 1, 1, 1);
                    }
                    buttonName = button.Item1.name;
                    button.Item1.targetGraphic.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                });
            }
        }

        private GameObject SetupRightPanel(GameObject panelR, string category, List<(string, string)> samples)
        {
            var panel = new GameObject(category, typeof(RectTransform));
            panel.transform.SetParent(panelR.transform, false);
            var panelT = panel.GetComponent<RectTransform>();
            panelT.anchorMin = new Vector2(0, 0);
            panelT.anchorMax = new Vector2(1, 1);
            panelT.offsetMin = Vector2.zero;
            panelT.offsetMax = Vector2.zero;

            var sampleAnchorX = new Vector2(0.1f, 0.9f);

            var space = 0.05f;
            var sampleH = 0.1f;
            if (samples.Count > 9)
            {
                space = 0.01f;
                sampleH = (1 - space * (samples.Count + 1)) / samples.Count;
            }
            else if (samples.Count > 6)
            {
                space = 0.01f;
                sampleH = 0.1f;
            }

            var yMax = 1f - space;
            foreach (var sample in samples)
            {
                var button = DefaultControls.CreateButton(new DefaultControls.Resources()).GetComponent<Button>();
                button.transform.SetParent(panel.transform, false);
                button.name = sample.Item1;

                var rectT = button.GetComponent<RectTransform>();
                var anchorY = new Vector2(yMax - sampleH, yMax);
                rectT.anchorMin = new Vector2(sampleAnchorX.x, anchorY.x);
                rectT.anchorMax = new Vector2(sampleAnchorX.y, anchorY.y);
                rectT.offsetMin = Vector2.zero;
                rectT.offsetMax = Vector2.zero;

                var text = button.GetComponentInChildren<Text>();
                text.resizeTextForBestFit = true;
                text.alignment = TextAnchor.MiddleCenter;
                var desc = sample.Item1;
                if (desc.StartsWith(category + "_"))
                {
                    desc = desc.Substring(category.Length);
                }
                desc = desc.Replace("_", " ");
                if (!string.IsNullOrEmpty(sample.Item2))
                {
                    text.supportRichText = true;
                    desc += Environment.NewLine + $"<color=#777777FF><i><size=10%>{sample.Item2}</size></i></color>";
                }
                text.text = desc;

                var textRectT = text.GetComponent<RectTransform>();
                textRectT.anchorMin = new Vector2(0.05f, 0.05f);
                textRectT.anchorMax = new Vector2(0.95f, 0.95f);
                textRectT.offsetMin = Vector2.zero;
                textRectT.offsetMax = Vector2.zero;

                if (Application.CanStreamedLevelBeLoaded(sample.Item1))
                {
                    button.onClick.AddListener(() =>
                    {
#if ENABLE_ARFOUNDATION
                        if (arfoundationScenes.Contains(sample.Item1))
                        {
                            UnityEngine.XR.ARFoundation.LoaderUtility.Initialize();
                        }
#endif
                        SceneManager.LoadScene(sample.Item1);
                    });
                }
                else
                {
                    button.interactable = false;
                }

                yMax = anchorY.x - space;
            }
            panel.SetActive(false);
            return panel;
        }

        private void SetupBackCanvas()
        {
            BackCanvas.SetActive(false);
            if (!string.IsNullOrEmpty(buttonName)) { return; }

            var mainSceneName = SceneManager.GetActiveScene().name;
            DontDestroyOnLoad(BackCanvas);
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                DynamicGI.UpdateEnvironment();
                BackCanvas.SetActive(scene.name != mainSceneName);
            };

            var scenes = arfoundationScenes;
            BackButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(mainSceneName);
                // avoid crash on iOS when multithreaded rendering is on, which is also observable on Unity official AR Foundation samples
#if ENABLE_ARFOUNDATION && UNITY_ANDROID
                if (scenes.Contains(SceneManager.GetActiveScene().name))
                {
                    UnityEngine.XR.ARFoundation.LoaderUtility.Deinitialize();
                }
#endif
            });
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
