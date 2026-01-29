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

namespace Sample
{
    public class Sample : MonoBehaviour
    {
        public ARSession Session;
        public GameObject MsgBox;
        private int wakingUpCount;

        private void Awake()
        {
            AdaptInputSystem();
#if !EASYAR_ENABLE_MEGA
            ShowMessage(1000, "package com.easyar.mega is required to use EasyAR Mega", "");
            Session.AutoStart = false;
#else
            Session.StateChanged += HandleSessionStateChange;
            var megaTracker = Session.GetComponentInChildren<MegaTrackerFrameFilter>();
            if (megaTracker)
            {
                megaTracker.LocalizationRespond += HandleLocalizationStatusChange;
            }
            MsgBox.SetActive(false);
#endif
        }

        private void OnDestroy()
        {
            if (Session)
            {
                Session.StateChanged -= HandleSessionStateChange;
                var megaTracker = Session.GetComponentInChildren<MegaTrackerFrameFilter>();
                if (megaTracker)
                {
                    megaTracker.LocalizationRespond -= HandleLocalizationStatusChange;
                }
            }
        }

        private void HandleSessionStateChange(ARSession.SessionState status)
        {
            if (status == ARSession.SessionState.Broken)
            {
                if (Session.Report.BrokenReason == SessionReport.SessionBrokenReason.NoAvailabileFrameSource || Session.Report.BrokenReason == SessionReport.SessionBrokenReason.FrameFilterNotAvailabile)
                {
                    ShowMessage(100,
                        Session.Report.Exception + Environment.NewLine +
                        $"EasyAR Session Broken with reason {Session.Report.BrokenReason}." + Environment.NewLine +
                        "Strictly, it means the device is not supported by EasyAR with (and only with) your configurations (features and settings you choose in the session object)." + Environment.NewLine +
                        $"Configurations you need to examine include but not limited to if there are missing/extra frame source/filter objects under the session, options like {nameof(ARSession)}.{nameof(ARSession.AssembleOptions)} and {nameof(MegaTrackerFrameFilter)}.{nameof(MegaTrackerFrameFilter.MinInputFrameLevel)}, etc.." + Environment.NewLine +
                        $"You can get detailed availability report from {nameof(ARSession)}.{nameof(ARSession.Report)}." + Environment.NewLine +
                        $"The session may recover automatically if the device list updated from online data during session start and found device supported." + Environment.NewLine +
                        $"EasyAR Session 损坏（原因：{Session.Report.BrokenReason}）。" + Environment.NewLine +
                        "严格来说，这意味着你配置（且仅该配置）下的EasyAR无法在该设备上运行。配置指你在session物体中所选择的功能和设置。" + Environment.NewLine +
                        $"你需要检查的配置包括但不限于，session下是否有缺失或多余的frame source/filter物体，一些选项比如{nameof(ARSession)}.{nameof(ARSession.AssembleOptions)}，{nameof(MegaTrackerFrameFilter)}.{nameof(MegaTrackerFrameFilter.MinInputFrameLevel)}等。" + Environment.NewLine +
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
                    ShowMessage(100,
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

        private void HandleLocalizationStatusChange(MegaLocalizationResponse response)
        {
            var status = response.Status;
            wakingUpCount = status == MegaTrackerLocalizationStatus.WakingUp ? wakingUpCount + 1 : 0;
            if (wakingUpCount >= 5)
            {
                ShowMessage(10,
                    "Service is waking up, you need to let user wait." + Environment.NewLine +
                    "服务正在唤醒中，你需要让用户等待。"
                    ,
                    "Service is waking up, please wait patiently." + Environment.NewLine +
                    "服务正在唤醒中，请耐心等待。"
                );
            }

            if (status == MegaTrackerLocalizationStatus.QpsLimitExceeded)
            {
                ShowMessage(10,
                    "QPS limit exceeded, you can keep random user fail (overall worse tracking quality) or pay for more QPS." + Environment.NewLine +
                    "QPS超限，你可以保持随机用户失败（总体跟踪质量下降）或付费提升QPS上限。"
                    ,
                    "Too many users, please wait patiently." + Environment.NewLine +
                    "用户过多，请耐心等待。"
                );
            }

            if (status == MegaTrackerLocalizationStatus.ApiTokenExpired)
            {
                ShowMessage(10,
                    "Token expired (may only occurs when you access the service using a Token)." + Environment.NewLine +
                    "You need to request a Token from your own backend and call MegaTrackerFrameFilter.UpdateToken to update it. Your backend should generate the Token using EasyAR's Token generation method." + Environment.NewLine +
                    "Token过期（仅在你使用Token访问服务时可能出现）。" + Environment.NewLine +
                    $"你需要请求你自己的后台获取Token，并调用MegaTrackerFrameFilter.UpdateToken进行更新。你的后台应使用EasyAR的Token生成方式生成Token。"
                    ,
                    "<Instructions from App developer to user.>" + Environment.NewLine +
                    "<应用开发者对用户的说明。>"
                );
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
