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

namespace VideoRecording
{
    public class RecordingSample : MonoBehaviour
    {
        public GameObject MsgBox;
        public VideoRecorder VideoRecorder;
        public Material WatermarkMaterial;
        public bool RecordWatermark;

        private CameraRecorder cameraRecorder;
        private GameObject videoBoard;

        private void Awake()
        {
            AdaptInputSystem();
            MsgBox.SetActive(false);
            if (!VideoRecorder.IsAvailable)
            {
                ShowMessage($"VideoRecorder not available: {VideoRecorder.NotAvailableReason}", 10000);
            }
        }

        public void SampleStart()
        {
            if (!VideoRecorder || !VideoRecorder.IsAvailable) { return; }

            if (videoBoard) { Destroy(videoBoard); }

            VideoRecorder.FilePathType = WritablePathType.PersistentDataPath;
            VideoRecorder.FilePath = "Video_Recording_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".mp4";
            VideoRecorder.StartRecording((success, permission, error) => ShowMessage(success ? "Recording start" : $"Recording start fail: {permission}, {error} ", 10), (error) => ShowMessage($"Recording error: {error}", 5));

            cameraRecorder = Camera.main.gameObject.AddComponent<CameraRecorder>();
            cameraRecorder.Setup(VideoRecorder, RecordWatermark ? WatermarkMaterial : null);
        }

        public void SampleStop()
        {
            if (!VideoRecorder || !VideoRecorder.IsAvailable) { return; }

            if (VideoRecorder.StopRecording())
            {
                ShowMessage("Recording finished, video saved to Unity Application.persistentDataPath" + Environment.NewLine +
                    "Filename: " + VideoRecorder.FilePath + Environment.NewLine +
                    "PersistentDataPath: " + Application.persistentDataPath + Environment.NewLine +
                    "You can change sample code if you prefer to record videos into system album", 8);
                AdjustVideoAndPlay();
            }
            else
            {
                ShowMessage("Recording failed", 5);
            }
            if (cameraRecorder) { Destroy(cameraRecorder); }
        }

        private void AdjustVideoAndPlay()
        {
            videoBoard = GameObject.CreatePrimitive(PrimitiveType.Quad);
            videoBoard.transform.SetParent(Camera.main.transform, false);
            videoBoard.transform.localPosition = new Vector3(0, 0, 3);
            videoBoard.transform.localScale = new Vector3(1, (float)Screen.height / Screen.width, 1);
            var audio = videoBoard.AddComponent<AudioSource>();
            var player = videoBoard.AddComponent<UnityEngine.Video.VideoPlayer>();
            player.source = UnityEngine.Video.VideoSource.Url;
            player.url = Application.persistentDataPath + "/" + VideoRecorder.FilePath;
            player.isLooping = true;
            player.renderMode = UnityEngine.Video.VideoRenderMode.MaterialOverride;
            player.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.AudioSource;
            player.SetTargetAudioSource(0, audio);
        }

        private void ShowMessage(string message, int time)
        {
            StopAllCoroutines();
            Debug.Log(message);
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
