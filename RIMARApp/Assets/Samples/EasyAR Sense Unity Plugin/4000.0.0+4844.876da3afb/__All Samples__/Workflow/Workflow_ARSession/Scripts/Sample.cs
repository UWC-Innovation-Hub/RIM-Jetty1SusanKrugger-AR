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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class Sample : MonoBehaviour
    {
        public ARSession Session;
        public GameObject MsgBox;
        public GameObject ToggleTemplate;
        public GameObject DropdownTemplate;
        public GameObject AssembleOptionsPanel;
        public GameObject AvailabilityPanel;
        public GameObject RunningOptionsPanel;
        public Button AssembleButton;
        public Button StartButton;
        public Button StopButton;
        public Toggle KeepLastFrameToggle;
        public Toggle RecordEIFToggle;
        public Dropdown FrameSourceDropdown;

        private Dropdown aFrameSource;
        private Toggle aCustomFrameSource;
        private List<(Toggle, FrameSource)> aFrameSourceToggleList;
        private Dropdown aFrameFilter;
        private List<(Toggle, FrameFilter)> aFrameFilterToggleList;

        private List<(Text, FrameSource)> aFrameSourceTextList;
        private List<(Text, FrameFilter)> aFrameFilterTextList;

        private Toggle rSessionToggle;
        private Toggle rCameraImageToggle;
        private Toggle rFrameSourceToggle;
        private List<(Toggle, FrameFilter)> rFrameFilterToggleList;
        private Dropdown rCenterModeDropdown;

        private SessionReport.AvailabilityReport lastReport;

        private void Awake()
        {
            AdaptInputSystem();
            SetupButtons();
            SetupAssembleOptionsPanel();
            SetupAvailabilityPanel();
            SetupRunningOptionsPanel();
            UpdateUIInteractable(Session.State);
            ShowMessage(5, $"You can tap the screen 8 times to open developer panel when Session.Diagnostics.DeveloperModeSwitch is set to its default value. Suggest keeping it unchanged or implementing your own switch and release in your app. It is helpful when you or your user encounters some problem and needs to report.", "");
            Session.AutoStart = false;
            Session.StateChanged += HandleSessionStateChange;
            Session.StateChanged += UpdateUIInteractable;
            Session.AssembleUpdate += ShowAvailabilityUpdate;
        }

        private void OnDestroy()
        {
            if (Session)
            {
                Session.AssembleUpdate -= ShowAvailabilityUpdate;
                Session.StateChanged -= UpdateUIInteractable;
                Session.StateChanged -= HandleSessionStateChange;
            }
        }

        private void Assemble()
        {
            StartCoroutine(Session.Assemble());
        }

        private void StartSession()
        {
            Session.StartSession();
        }

        private void StopSession(bool keepLastFrame)
        {
            Session.StopSession(keepLastFrame);
        }

        private void SortFrameSource(int idx)
        {
            ARSessionFactory.SortFrameSource(Session.gameObject, idx switch
            {
                0 => new ARSessionFactory.FrameSourceSortMethod { ARCore = ARSessionFactory.FrameSourceSortMethod.ARCoreSortMethod.PreferEasyAR, ARKit = ARSessionFactory.FrameSourceSortMethod.ARKitSortMethod.PreferEasyAR, MotionTracker = ARSessionFactory.FrameSourceSortMethod.MotionTrackerSortMethod.PreferSystem },
                1 => new ARSessionFactory.FrameSourceSortMethod { ARCore = ARSessionFactory.FrameSourceSortMethod.ARCoreSortMethod.PreferARFoundation, ARKit = ARSessionFactory.FrameSourceSortMethod.ARKitSortMethod.PreferARFoundation, MotionTracker = ARSessionFactory.FrameSourceSortMethod.MotionTrackerSortMethod.PreferSystem },
                2 => new ARSessionFactory.FrameSourceSortMethod { ARCore = ARSessionFactory.FrameSourceSortMethod.ARCoreSortMethod.PreferEasyAR, ARKit = ARSessionFactory.FrameSourceSortMethod.ARKitSortMethod.PreferEasyAR, MotionTracker = ARSessionFactory.FrameSourceSortMethod.MotionTrackerSortMethod.PreferEasyAR },
                _ => throw new ArgumentOutOfRangeException(),
            });
            SetupAssembleOptionsPanel();
            SetupAvailabilityPanel();
        }

        private bool ToggleSession(bool on)
        {
            Session.enabled = on;
            return true;
        }

        private bool ToggleCameraImage(bool on)
        {
            if (Session.Assembly == null || Session.Assembly.CameraImageRenderer.OnNone) { return false; }
            Session.Assembly.CameraImageRenderer.Value.enabled = on;
            return true;
        }

        private bool ToggleFrameSource(bool on)
        {
            if (Session.Assembly == null) { return false; }
            Session.Assembly.FrameSource.enabled = on;
            return true;
        }

        private bool ToggleFrameFilter(FrameFilter filter, bool on)
        {
            if (Session.Assembly == null || !Session.Assembly.FrameFilters.Contains(filter)) { return false; }
            filter.enabled = on;
            return true;
        }

        private bool ToggleFrameRecorder(bool on)
        {
            if (Session.Assembly == null || Session.Assembly.FrameRecorder.OnNone) { return false; }
            Session.Assembly.FrameRecorder.Value.enabled = on;
            return true;
        }

        private bool SetCenterMode(ARSession.ARCenterMode mode)
        {
            if (Session.Assembly == null || !Session.Assembly.AvailableCenterMode.Contains(mode)) { return false; }
            Session.CenterMode = mode;
            return true;
        }

        private void HandleSessionStateChange(ARSession.SessionState status)
        {
            if (status == ARSession.SessionState.Broken)
            {
                if (Session.Report.BrokenReason == SessionReport.SessionBrokenReason.NoAvailabileFrameSource || Session.Report.BrokenReason == SessionReport.SessionBrokenReason.FrameFilterNotAvailabile)
                {
                    ShowMessage(20,
                        Session.Report.Exception + Environment.NewLine +
                        $"EasyAR Session Broken with reason {Session.Report.BrokenReason}." + Environment.NewLine +
                        "Strictly, it means the device is not supported by EasyAR with (and only with) your configurations (features and settings you choose in the session object)." + Environment.NewLine +
                        $"Configurations you need to examine include but not limited to if there are missing/extra frame source/filter objects under the session, options like {nameof(ARSession)}.{nameof(ARSession.AssembleOptions)}, etc.." + Environment.NewLine +
                        $"You can get detailed availability report from {nameof(ARSession)}.{nameof(ARSession.Report)}." + Environment.NewLine +
                        $"The session may recover automatically if the device list updated from online data during session start and found device supported." + Environment.NewLine +
                        $"EasyAR Session 损坏（原因：{Session.Report.BrokenReason}）。" + Environment.NewLine +
                        "严格来说，这意味着你配置（且仅该配置）下的EasyAR无法在该设备上运行。配置指你在session物体中所选择的功能和设置。" + Environment.NewLine +
                        $"你需要检查的配置包括但不限于，session下是否有缺失或多余的frame source/filter物体，一些选项比如{nameof(ARSession)}.{nameof(ARSession.AssembleOptions)}等。" + Environment.NewLine +
                        $"你可以从{nameof(ARSession)}.{nameof(ARSession.Report)}获得详细的可用性报告。" + Environment.NewLine +
                        $"如果在启动session时联网更新设备列表时发现设备已被支持，session有可能自动恢复。"
                        ,
                        $"EasyAR Session Broken with reason {Session.Report.BrokenReason}." + Environment.NewLine +
                        "The EasyAR features selected by the App developer cannot run on your device." + Environment.NewLine +
                        $"EasyAR Session 损坏（原因：{Session.Report.BrokenReason}）。" + Environment.NewLine +
                        "应用开发者选择的EasyAR功能无法在你的设备上运行。"
                    );
                }
                else
                {
                    ShowMessage(20,
                        Session.Report.Exception + Environment.NewLine +
                        $"EasyAR Session Broken with reason {Session.Report.BrokenReason}." + Environment.NewLine +
                        "This is usually device-irrelevant, Please debug your project using exception details." + Environment.NewLine +
                        $"EasyAR Session 损坏（原因：{Session.Report.BrokenReason}）。" + Environment.NewLine +
                        "这通常是设备无关的，请使用详细异常信息调试你的工程。"
                        ,
                        $"EasyAR Session Broken with reason {Session.Report.BrokenReason}, please ask the app developer for help." + Environment.NewLine +
                        $"EasyAR Session 损坏（原因：{Session.Report.BrokenReason}），请向应用开发者寻求帮助。"
                    );
                }
            }
            else
            {
                StartCoroutine(ClearMessage(5));
            }
        }

        #region UI Setup
        private void SetupButtons()
        {
            AssembleButton.onClick.AddListener(() => Assemble());
            StartButton.onClick.AddListener(() => StartSession());
            StopButton.onClick.AddListener(() => StopSession(KeepLastFrameToggle.isOn));
            RecordEIFToggle.onValueChanged.AddListener((v) => ToggleFrameRecorder(v));
            FrameSourceDropdown.onValueChanged.AddListener((v) => SortFrameSource(v));
        }
        private void SetupAssembleOptionsPanel()
        {
            var lartaFrameSourceToggleList = aFrameSourceToggleList;
            var lartaFrameFilterToggleList = aFrameFilterToggleList;
            aFrameSource = InstantiateDropdown("Frame Source", Enum.GetNames(typeof(AssembleOptions.FrameSourceSelection)).ToList(), (v) => PrepareAssembleOptions());
            aCustomFrameSource = InstantiateToggle("Enable Custom Camera", (v) => PrepareAssembleOptions());
            aFrameSourceToggleList = Session.GetComponentsInChildren<FrameSource>(true).Where(f => f.transform != Session.transform).OrderBy(f => f.transform.GetSiblingIndex()).Select(f => (InstantiateToggle(ARSessionFactory.DefaultName(f.GetType()), (v) => PrepareAssembleOptions()), f)).ToList();
            aFrameFilter = InstantiateDropdown("Frame Filter", Enum.GetNames(typeof(AssembleOptions.FrameFilterSelection)).ToList(), (v) => PrepareAssembleOptions());
            aFrameFilterToggleList = Session.GetComponentsInChildren<FrameFilter>(true).Select(f => (InstantiateToggle(ARSessionFactory.DefaultName(f.GetType()), (v) => PrepareAssembleOptions()), f)).ToList();

            Layout(AssembleOptionsPanel.transform, new List<RectTransform>
            {
                aFrameSource.transform.parent.GetComponent<RectTransform>(),
                aCustomFrameSource.GetComponent<RectTransform>(),
            }
            .Concat(aFrameSourceToggleList.Select(f => f.Item1.GetComponent<RectTransform>()))
            .Concat(new List<RectTransform>
            {
                aFrameFilter.transform.parent.GetComponent<RectTransform>(),
            })
            .Concat(aFrameFilterToggleList.Select(f => f.Item1.GetComponent<RectTransform>())).ToList());

            if (lartaFrameSourceToggleList != null)
            {
                foreach (var item in aFrameSourceToggleList)
                {
                    if (lartaFrameSourceToggleList.Where(t => t.Item2 == item.Item2).Any())
                    {
                        item.Item1.isOn = lartaFrameSourceToggleList.Where(t => t.Item2 == item.Item2).First().Item1.isOn;
                    }
                }
            }
            if (lartaFrameFilterToggleList != null)
            {
                foreach (var item in aFrameFilterToggleList)
                {
                    if (lartaFrameFilterToggleList.Where(t => t.Item2 == item.Item2).Any())
                    {
                        item.Item1.isOn = lartaFrameFilterToggleList.Where(t => t.Item2 == item.Item2).First().Item1.isOn;
                    }
                }
            }
        }

        private void SetupAvailabilityPanel()
        {
            aFrameSourceTextList = aFrameSourceToggleList.Select(t => (InstantiateText("Unknown"), t.Item2)).ToList();
            aFrameFilterTextList = aFrameFilterToggleList.Select(t => (InstantiateText("Unknown"), t.Item2)).ToList();

            Layout(AvailabilityPanel.transform, new List<RectTransform>
            {
                InstantiateText(string.Empty).GetComponent<RectTransform>(),
                InstantiateText(string.Empty).GetComponent<RectTransform>(),
            }
            .Concat(aFrameSourceTextList.Select(f => f.Item1.GetComponent<RectTransform>()))
            .Concat(new List<RectTransform>
            {
                InstantiateText(string.Empty).GetComponent<RectTransform>(),
            })
            .Concat(aFrameFilterTextList.Select(f => f.Item1.GetComponent<RectTransform>())).ToList());

            ShowAvailabilityUpdate(lastReport);
        }

        private void SetupRunningOptionsPanel()
        {
            rSessionToggle = InstantiateToggle("Session", (v) => ToggleSession(v));
            rCameraImageToggle = InstantiateToggle("Camera Image", (v) => ToggleCameraImage(v));
            rFrameSourceToggle = InstantiateToggle("Frame Source", (v) => ToggleFrameSource(v));
            rFrameFilterToggleList = Session.GetComponentsInChildren<FrameFilter>(true).Select(f => (InstantiateToggle(ARSessionFactory.DefaultName(f.GetType()), (v) => ToggleFrameFilter(f, v)), f)).ToList();
            rCenterModeDropdown = InstantiateDropdown("Center Mode", Enum.GetNames(typeof(ARSession.ARCenterMode)).ToList(), (v) =>
            {
                if (!SetCenterMode((ARSession.ARCenterMode)v))
                {
                    rCenterModeDropdown.SetValueWithoutNotify((int)Session.CenterMode);
                }
            });

            Layout(RunningOptionsPanel.transform, new List<RectTransform>
            {
                rSessionToggle.GetComponent<RectTransform>(),
                rCenterModeDropdown.transform.parent.GetComponent<RectTransform>(),
                rCameraImageToggle.GetComponent<RectTransform>(),
                rFrameSourceToggle.GetComponent<RectTransform>(),
            }
            .Concat(rFrameFilterToggleList.Select(f => f.Item1.GetComponent<RectTransform>())).ToList());
        }

        private Toggle InstantiateToggle(string name, UnityEngine.Events.UnityAction<bool> call)
        {
            var go = Instantiate(ToggleTemplate);
            go.name = name;
            go.SetActive(true);
            go.GetComponentInChildren<Text>().text = name;
            var toggle = go.GetComponent<Toggle>();
            if (call != null)
            {
                toggle.onValueChanged.AddListener(call);
            }
            return toggle;
        }

        private Dropdown InstantiateDropdown(string name, List<string> options, UnityEngine.Events.UnityAction<int> call)
        {
            var go = Instantiate(DropdownTemplate);
            go.name = name;
            go.SetActive(true);
            go.GetComponentsInChildren<Text>().Where(t => t.name == "Text").SingleOrDefault().text = name;
            var dropdown = go.GetComponentInChildren<Dropdown>();
            dropdown.AddOptions(options);
            if (call != null)
            {
                dropdown.onValueChanged.AddListener(call);
            }
            return dropdown;
        }

        private Text InstantiateText(string name)
        {
            var text = DefaultControls.CreateText(new DefaultControls.Resources()).GetComponent<Text>();
            text.text = name;
            text.resizeTextForBestFit = true;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            return text;
        }

        private void Layout(Transform parent, List<RectTransform> transforms)
        {
            foreach (Transform t in parent) { Destroy(t.gameObject); }
            var anchorX = new Vector2(0, 1);
            var space = 0.01f;
            var h = Mathf.Min(0.1f, (1 - space * (transforms.Count - 1)) / transforms.Count);

            var yMax = 1f;
            foreach (var rectT in transforms)
            {
                rectT.SetParent(parent, false);
                var anchorY = new Vector2(yMax - h, yMax);
                rectT.anchorMin = new Vector2(anchorX.x, anchorY.x);
                rectT.anchorMax = new Vector2(anchorX.y, anchorY.y);
                rectT.offsetMin = Vector2.zero;
                rectT.offsetMax = Vector2.zero;
                yMax = anchorY.x - space;
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
        #endregion

        #region UI Update
        private void UpdateUIInteractable(ARSession.SessionState state)
        {
            AssembleButton.interactable = state < ARSession.SessionState.Assembling && EasyARController.IsReady;
            FrameSourceDropdown.interactable = state < ARSession.SessionState.Assembling && EasyARController.IsReady;
            StartButton.interactable = state < ARSession.SessionState.Ready && EasyARController.IsReady;
            StopButton.interactable = state >= ARSession.SessionState.Assembling;
            KeepLastFrameToggle.interactable = state >= ARSession.SessionState.Ready;
            RecordEIFToggle.interactable = state >= ARSession.SessionState.Ready;

            aFrameSource.interactable = AssembleButton.interactable;
            aCustomFrameSource.interactable = AssembleButton.interactable;
            foreach (var toggle in aFrameSourceToggleList)
            {
                toggle.Item1.interactable = AssembleButton.interactable;
            }
            aFrameFilter.interactable = AssembleButton.interactable;
            foreach (var toggle in aFrameFilterToggleList)
            {
                toggle.Item1.interactable = AssembleButton.interactable;
            }

            rSessionToggle.interactable = state >= ARSession.SessionState.Ready;
            rCenterModeDropdown.interactable = state >= ARSession.SessionState.Ready;
            rCameraImageToggle.interactable = state >= ARSession.SessionState.Ready && Session.Assembly.CameraImageRenderer.OnSome;
            rFrameSourceToggle.interactable = state >= ARSession.SessionState.Ready;
            foreach (var toggle in rFrameFilterToggleList)
            {
                toggle.Item1.interactable = state >= ARSession.SessionState.Ready && Session.Assembly.FrameFilters.Contains(toggle.Item2);
            }
            if (state == ARSession.SessionState.Ready)
            {
                rSessionToggle.SetIsOnWithoutNotify(Session.enabled);
                rCameraImageToggle.SetIsOnWithoutNotify(Session.Assembly.CameraImageRenderer.OnSome ? Session.Assembly.CameraImageRenderer.Value.enabled : false);
                rFrameSourceToggle.SetIsOnWithoutNotify(Session.Assembly.FrameSource.enabled);
                foreach (var toggle in rFrameFilterToggleList)
                {
                    toggle.Item1.SetIsOnWithoutNotify(toggle.Item2.enabled);
                }
                rCenterModeDropdown.SetValueWithoutNotify((int)Session.CenterMode);
            }
            if (state < ARSession.SessionState.Assembling)
            {
                PrepareAssembleOptions();
            }
        }

        private void PrepareAssembleOptions()
        {
            Session.AssembleOptions.FrameSource = (AssembleOptions.FrameSourceSelection)aFrameSource.value;
            Session.AssembleOptions.FrameFilter = (AssembleOptions.FrameFilterSelection)aFrameFilter.value;
            Session.AssembleOptions.EnableCustomCamera = aCustomFrameSource.isOn;

            foreach (var toggle in aFrameSourceToggleList)
            {
                toggle.Item2.gameObject.SetActive(true);
            }
            if (Session.AssembleOptions.FrameSource == AssembleOptions.FrameSourceSelection.Auto)
            {
                foreach (var toggle in aFrameSourceToggleList)
                {
                    toggle.Item2.gameObject.SetActive(toggle.Item1.isOn);
                }
            }
            else if (Session.AssembleOptions.FrameSource == AssembleOptions.FrameSourceSelection.Manual)
            {
                var sources = aFrameSourceToggleList.Where(t => t.Item1.isOn).Select(t => t.Item2);
                Session.AssembleOptions.SpecifiedFrameSource = sources.Where(t => t != Session.AssembleOptions.SpecifiedFrameSource).FirstOrDefault();
                if (!Session.AssembleOptions.SpecifiedFrameSource)
                {
                    Session.AssembleOptions.SpecifiedFrameSource = sources.FirstOrDefault();
                }
                foreach (var toggle in aFrameSourceToggleList)
                {
                    toggle.Item1.SetIsOnWithoutNotify(toggle.Item2 == Session.AssembleOptions.SpecifiedFrameSource);
                }
            }

            foreach (var toggle in aFrameFilterToggleList)
            {
                toggle.Item2.gameObject.SetActive(toggle.Item1.isOn);
            }
            if (Session.AssembleOptions.FrameFilter == AssembleOptions.FrameFilterSelection.Manual)
            {
                Session.AssembleOptions.SpecifiedFrameFilters = aFrameFilterToggleList.Where(t => t.Item2.gameObject.activeSelf).Select(t => t.Item2).ToList();
            }
        }

        private void ShowAvailabilityUpdate(SessionReport.AvailabilityReport report)
        {
            if(report == null) { return; }

            lastReport = report;
            foreach (var text in aFrameSourceTextList)
            {
                var data = report.FrameSources.Where(r => r.Component == text.Item2).SingleOrDefault();
                text.Item1.text = data?.Availability.ToString() ?? "-";
            }
            foreach (var text in aFrameFilterTextList)
            {
                var data = report.FrameFilters.Where(r => r.Component == text.Item2).SingleOrDefault();
                text.Item1.text = data?.Availability.ToString() ?? "-";
            }
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
        #endregion
    }
}
