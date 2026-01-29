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
    public class ViewManager : MonoBehaviour
    {
        public static ViewManager Instance;
        public ARSession Session;
        public Material SparseMaterial;
        public MainViewController MainView;
        public CreateViewController CreateView;
        public EditViewController EditView;
        public GameObject PreviewView;
        public Toggle EditPointCloudUI;
        public Toggle PreviewPointCloudUI;
        public Toggle EIFToggle;
        public Text RecycleBinText;
        public bool MainViewRecycleBinClearMapCacheOnly;

        private List<MapMeta> selectedMaps = new List<MapMeta>();
        private TrackMapSession trackMapSession;

        private void Awake()
        {
            AdaptInputSystem();
            Session.AutoStart = false;
            Instance = this;
            MainView.gameObject.SetActive(false);
            CreateView.gameObject.SetActive(false);
            EditView.gameObject.SetActive(false);
            PreviewView.SetActive(false);
            if (MainViewRecycleBinClearMapCacheOnly)
            {
                RecycleBinText.text = "Delete only maps caches";
            }
            else
            {
                RecycleBinText.text = "Delete all maps and caches";
            }
        }

        private void Start()
        {
            EIFToggle.gameObject.SetActive(false);
            EIFToggle.onValueChanged.AddListener((v) => Session.GetComponent<FrameRecorder>().enabled = v);
            Session.GetComponent<FrameRecorder>().OnReady.AddListener(() => { EIFToggle.gameObject.SetActive(true); });

            MessageBox.EnqueueMessage("Please wait while checking session availability...", 3);
            Session.AssembleUpdate += OnAssembleUpdate;
            StartCoroutine(Session.Assemble());
        }

        private void OnDestroy()
        {
            StopSession();
        }

        public void SelectMaps(List<MapMeta> metas)
        {
            selectedMaps = metas;
            MainView.EnablePreview(selectedMaps.Count > 0);
            MainView.EnableEdit(selectedMaps.Count == 1);
        }

        public void LoadMainView()
        {
            StopAllCoroutines();
            StopSession();
            SelectMaps(new List<MapMeta>());
            MainView.gameObject.SetActive(true);
        }

        public void LoadCreateView()
        {
            var buildSession = StartBuildSession();
            CreateView.SetMapSession(buildSession);
            CreateView.gameObject.SetActive(true);
        }

        public void LoadEditView()
        {
            var trackSession = StartTrackSession();
            trackSession.LoadMapMeta(SparseMaterial, true);
            EditView.SetMapSession(trackSession);
            EditView.gameObject.SetActive(true);
            EditPointCloudUI.isOn = true;
            StartCoroutine(HandlePointCloud(EditPointCloudUI));
        }

        public void LoadPreviewView()
        {
            var trackSession = StartTrackSession();
            trackSession.LoadMapMeta(SparseMaterial, false);
            PreviewView.SetActive(true);
            PreviewPointCloudUI.isOn = false;
            StartCoroutine(HandlePointCloud(PreviewPointCloudUI));
        }

        public void ShowParticle(bool show)
        {
            if (trackMapSession == null)
            {
                return;
            }
            foreach (var map in trackMapSession.Maps)
            {
                if (map.Controller) { map.Controller.PointCloudRenderer.Show = show; }
            }
        }

        public void SwitchCenterMode()
        {
            if (Session.AvailableCenterMode.Count == 0) { return; }
            while (true)
            {
                Session.CenterMode = (ARSession.ARCenterMode)(((int)Session.CenterMode + 1) % Enum.GetValues(typeof(ARSession.ARCenterMode)).Length);
                if (Session.AvailableCenterMode.Contains(Session.CenterMode)) { break; }
            }
        }

        private void OnAssembleUpdate(SessionReport.AvailabilityReport report)
        {
            if (report.FrameSources.Where(f => f.Availability == SessionReport.AvailabilityReport.AvailabilityStatus.Available).Any())
            {
                MessageBox.ClearMessages();
                LoadMainView();
                Session.AssembleUpdate -= OnAssembleUpdate;
            }
            else
            {
                MainView.gameObject.SetActive(false);
                CreateView.gameObject.SetActive(false);
                EditView.gameObject.SetActive(false);
                PreviewView.SetActive(false);
                MessageBox.ClearMessages();
                var message = string.Empty;
                foreach (var fs in report.FrameSources.Select(f => f.Component))
                {
                    message += (string.IsNullOrEmpty(message) ? "" : ", ") + $"{ARSessionFactory.DefaultName(fs.GetType())}";
                }
                MessageBox.EnqueueMessage($"This device is not supported by any active frame source in session '{Session}':\n{message}", 100);
            }
            if (report.PendingDeviceList.Count <= 0)
            {
                Session.AssembleUpdate -= OnAssembleUpdate;
            }
        }

        private BuildMapSession StartBuildSession()
        {
            var mapSession = new BuildMapSession(Session);
            Session.AssembleOptions.FrameFilter = AssembleOptions.FrameFilterSelection.Manual;
            Session.AssembleOptions.SpecifiedFrameFilters = new List<FrameFilter> { Session.GetComponentInChildren<SparseSpatialMapBuilderFrameFilter>() };
            Session.StartSession();
            return mapSession;
        }

        private TrackMapSession StartTrackSession()
        {
            var mapSession = new TrackMapSession(Session, selectedMaps);
            trackMapSession = mapSession;
            Session.AssembleOptions.FrameFilter = AssembleOptions.FrameFilterSelection.Manual;
            Session.AssembleOptions.SpecifiedFrameFilters = new List<FrameFilter> { Session.GetComponentInChildren<SparseSpatialMapTrackerFrameFilter>() };
            Session.StartSession();
            return mapSession;
        }

        private void StopSession()
        {
            if (trackMapSession != null)
            {
                trackMapSession.Dispose();
                trackMapSession = null;
            }
            if (Session)
            {
                Session.GetComponent<FrameRecorder>().enabled = false;
                Session.StopSession(false);
            }
            EIFToggle.SetIsOnWithoutNotify(false);
            EIFToggle.gameObject.SetActive(false);
        }

        private IEnumerator HandlePointCloud(Toggle toggle)
        {
            Dictionary<SparseSpatialMapController, bool> lastStates = new();
            while (true)
            {
                var trackedMaps = trackMapSession?.Maps.Select(m => m.Controller).Where(c => c && c.IsTracked).ToList() ?? new();
                toggle.gameObject.SetActive(trackedMaps.Any());
                foreach (var map in trackedMaps)
                {
                    var isDirectlyTracked = Optional<bool>.Empty;
                    if (lastStates.TryGetValue(map, out var state))
                    {
                        isDirectlyTracked = state;
                    }

                    if (isDirectlyTracked != map.IsDirectlyTracked)
                    {
                        lastStates[map] = map.IsDirectlyTracked;
                        var parameter = map.PointCloudRenderer.ParticleParameter;
                        parameter.StartColor = map.IsDirectlyTracked ? new Color32(11, 205, 255, 255) : new Color32(163, 236, 255, 255);
                        map.PointCloudRenderer.ParticleParameter = parameter;
                    }
                }
                yield return null;
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
    }
}
