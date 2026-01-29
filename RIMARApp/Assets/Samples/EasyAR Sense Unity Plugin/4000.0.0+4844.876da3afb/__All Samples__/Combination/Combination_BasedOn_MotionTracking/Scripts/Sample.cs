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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Combination_BasedOn_MotionTracking
{
    public class Sample : MonoBehaviour
    {
        public ARSession Session;
        public GameObject MsgBox;
        public Toggle EnableARFoundation;
        public Toggle EnableSpatialMap;
        public Toggle EnableMega;

        private void Awake()
        {
            AdaptInputSystem();
        }

        private void OnDestroy()
        {
            if (Session)
            {
                Session.StateChanged -= HandleSessionStateChange;
            }
        }

        public void StartSample()
        {
            if (!EnableMega.isOn)
            {
                Session.GetComponentInChildren<MegaTrackerFrameFilter>().gameObject.SetActive(false);
            }
            if (!EnableSpatialMap.isOn)
            {
                Session.GetComponentInChildren<SparseSpatialMapBuilderFrameFilter>().gameObject.SetActive(false);
                Session.GetComponentInChildren<DenseSpatialMapBuilderFrameFilter>().gameObject.SetActive(false);
            }

            if (EnableARFoundation.isOn)
            {
#if EASYAR_DISABLE_ARFOUNDATION
                ShowMessage(10000, "EasyAR AR Foundation support is disabled");
                return;
#elif !ENABLE_ARFOUNDATION
                ShowMessage(10000, "AR Foundation >= 5 package not exist");
                return;
#elif UNITY_ANDROID && !UNITY_EDITOR && !ENABLE_ARFOUNDATION_ARCORE
                ShowMessage(10000, "Google ARCore XR Plugin package not exist");
                return;
#elif UNITY_IOS && !UNITY_EDITOR && !ENABLE_ARFOUNDATION_ARKIT
                ShowMessage(10000, "Apple ARKit XR Plugin package not exist");
                return;
#else
                ARSessionFactory.SortFrameSource(Session.gameObject, new ARSessionFactory.FrameSourceSortMethod { ARCore = ARSessionFactory.FrameSourceSortMethod.ARCoreSortMethod.PreferARFoundation, ARKit = ARSessionFactory.FrameSourceSortMethod.ARKitSortMethod.PreferARFoundation });
#endif
            }
            else
            {
                Session.GetComponentsInChildren<ARFoundationFrameSource>().ToList().ForEach(f => f.gameObject.SetActive(false));
            }
            if (EnableMega.isOn)
            {
#if !EASYAR_ENABLE_MEGA
                ShowMessage(10000, "package com.easyar.mega is required to use EasyAR Mega");
                return;
#endif
            }

            Session.CenterMode = ARSession.ARCenterMode.SessionOrigin;
            Session.StateChanged += HandleSessionStateChange;
            Session.StartSession();
        }

        private void HandleSessionStateChange(ARSession.SessionState status)
        {
            if (status == ARSession.SessionState.Broken)
            {
                if (Session.Report.BrokenReason == SessionReport.SessionBrokenReason.NoAvailabileFrameSource || Session.Report.BrokenReason == SessionReport.SessionBrokenReason.FrameFilterNotAvailabile)
                {
                    var report = Session.Report.Availability;
                    var message = $"Availability:" + Environment.NewLine;
                    if (report.DeviceList.Count > 0)
                    {
                        message += $"- Device list update status:" + Environment.NewLine;
                        foreach (var item in report.DeviceList)
                        {
                            message += $"  - {item.Type}: {item.Status} {item.Error}" + Environment.NewLine;
                        }
                    }
                    if (report.FrameSources.Count > 0)
                    {
                        message += $"- Frame Source:" + Environment.NewLine;
                        foreach (var item in report.FrameSources)
                        {
                            message += $"  - {ARSessionFactory.DefaultName(item.Component.GetType())}: {item.Availability}" + Environment.NewLine;
                        }
                    }
                    if (report.FrameFilters.Count > 0)
                    {
                        message += $"- Frame Filter:" + Environment.NewLine;
                        foreach (var item in report.FrameFilters)
                        {
                            message += $"  - {ARSessionFactory.DefaultName(item.Component.GetType())}: {item.Availability}" + Environment.NewLine;
                        }
                    }
                    ShowMessage(10000,
                        Session.Report.Exception + Environment.NewLine +
                        Environment.NewLine +
                        $"EasyAR Session Broken with reason {Session.Report.BrokenReason}." + Environment.NewLine +
                        "Strictly, it means the device is not supported by EasyAR with (and only with) your configurations (features and settings you choose in the session object)." + Environment.NewLine +
                        $"Configurations you need to examine include but not limited to if there are missing/extra frame source/filter objects under the session, options like {nameof(ARSession)}.{nameof(ARSession.AssembleOptions)}, etc.." + Environment.NewLine +
                        $"You can get detailed availability report from {nameof(ARSession)}.{nameof(ARSession.Report)}." + Environment.NewLine +
                        $"The session may recover automatically if the device list updated from online data during session start and found device supported." + Environment.NewLine +
                        Environment.NewLine +
                        $"EasyAR Session 损坏（原因：{Session.Report.BrokenReason}）。" + Environment.NewLine +
                        "严格来说，这意味着你配置（且仅该配置）下的EasyAR无法在该设备上运行。配置指你在session物体中所选择的功能和设置。" + Environment.NewLine +
                        $"你需要检查的配置包括但不限于，session下是否有缺失或多余的frame source/filter物体，一些选项比如{nameof(ARSession)}.{nameof(ARSession.AssembleOptions)}等。" + Environment.NewLine +
                        $"你可以从{nameof(ARSession)}.{nameof(ARSession.Report)}获得详细的可用性报告。" + Environment.NewLine +
                        $"如果在启动session时联网更新设备列表时发现设备已被支持，session有可能自动恢复。" + Environment.NewLine +
                        Environment.NewLine +
                        message
                    );
                }
                else
                {
                    ShowMessage(10000,
                        Session.Report.Exception + Environment.NewLine +
                        $"EasyAR Session Broken with reason {Session.Report.BrokenReason}." + Environment.NewLine +
                        "This is usually device-irrelevant, Please debug your project using exception details." + Environment.NewLine +
                        $"EasyAR Session 损坏（原因：{Session.Report.BrokenReason}）。" + Environment.NewLine +
                        "这通常是设备无关的，请使用详细异常信息调试你的工程。"
                    );
                }
            }
            else
            {
                StartCoroutine(ClearMessage(5));
            }

            if (status == ARSession.SessionState.Ready)
            {
                ShowMessage(10,
                    "Image must NOT move in real world when motion fusion is on." + Environment.NewLine +
                    Environment.NewLine +
                    "    Image target scale must be set to physical image width." + Environment.NewLine +
                    "    Scale is preset to match long edge of A4 paper." + Environment.NewLine +
                    "    Suggest to print out images for test."
                );
            }
        }

        private void ShowMessage(int time, string message)
        {
            StopAllCoroutines();
            MsgBox.GetComponentInChildren<Text>().text = message;
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
    }
}
