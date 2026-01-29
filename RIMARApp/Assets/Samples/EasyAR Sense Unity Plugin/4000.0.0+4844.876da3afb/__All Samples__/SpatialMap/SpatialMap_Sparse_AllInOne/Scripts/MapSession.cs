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
using System.Linq;
using UnityEngine;

namespace Sample
{
    public class TrackMapSession : IDisposable
    {
        public ARSession ARSession;
        public SparseSpatialMapTrackerFrameFilter Tracker;
        public List<MapData> Maps = new();
        private List<(SparseSpatialMapController, Action<bool, string>)> mapLoadHandler = new();

        public TrackMapSession(ARSession arSession, List<MapMeta> maps)
        {
            ARSession = arSession;
            Tracker = ARSession.GetComponentInChildren<SparseSpatialMapTrackerFrameFilter>();
            if (maps != null)
            {
                foreach (var meta in maps)
                {
                    Maps.Add(new MapData() { Meta = meta });
                }
            }
        }

        public void Dispose()
        {
            if (Tracker) { Tracker.TargetLoad -= OnMapLoad; }
            foreach (var map in Maps)
            {
                if (map.Controller) { UnityEngine.Object.Destroy(map.Controller.gameObject); }
                foreach (var prop in map.Props) { if (prop) { UnityEngine.Object.Destroy(prop); } }
            }
        }

        public void LoadMapMeta(Material sparseMaterial, bool isEdit)
        {
            if (!Tracker) { return; }

            Tracker.TargetLoad += OnMapLoad;
            foreach (var m in Maps)
            {
                var meta = m.Meta;
                var controller = ARSessionFactory.CreateController<SparseSpatialMapController>(new ARSessionFactory.Resources { SparseSpatialMapPointCloudMaterial = sparseMaterial }).GetComponent<SparseSpatialMapController>();
                controller.Source = meta.Map;
                controller.Tracker = Tracker;
                controller.PointCloudRenderer.Show = isEdit;
                mapLoadHandler.Add((controller, (status, error) =>
                {
                    MessageBox.EnqueueMessage("Load map {name = " + controller.Info.Name + ", id = " + controller.Info.ID + "} into " + Tracker.name + Environment.NewLine +
                        " => " + status + (string.IsNullOrEmpty(error) ? "" : " <" + error + ">"), status ? 3 : 5);
                    if (!status)
                    {
                        return;
                    }
                    MessageBox.EnqueueMessage("Notice: By default (MainViewRecycleBinClearMapCacheOnly == false)," + Environment.NewLine +
                        "load map will not trigger a download in this sample." + Environment.NewLine +
                        "Map cache is used (SparseSpatialMapManager.clear not called alone)." + Environment.NewLine +
                        "Statistical request count will not be increased (more details on EasyAR website).", 5);

                    foreach (var propInfo in meta.Props)
                    {
                        GameObject prop = null;
                        foreach (var templet in PropCollection.Instance.Templets)
                        {
                            if (templet.Object.name == propInfo.Name)
                            {
                                prop = UnityEngine.Object.Instantiate(templet.Object);
                                break;
                            }
                        }
                        if (!prop)
                        {
                            Debug.LogError("Missing prop templet: " + propInfo.Name);
                            continue;
                        }
                        prop.transform.parent = controller.transform;
                        prop.transform.localPosition = new Vector3(propInfo.Position[0], propInfo.Position[1], propInfo.Position[2]);
                        prop.transform.localRotation = new Quaternion(propInfo.Rotation[0], propInfo.Rotation[1], propInfo.Rotation[2], propInfo.Rotation[3]);
                        prop.transform.localScale = new Vector3(propInfo.Scale[0], propInfo.Scale[1], propInfo.Scale[2]);
                        prop.name = propInfo.Name;
                        if (isEdit)
                        {
                            var video = prop.GetComponentInChildren<VideoPlayerAgent>(true);
                            if (video) { video.Playable = false; }
                        }
                        m.Props.Add(prop);
                    }
                }
                ));

                controller.TargetFound += () =>
                {
                    MessageBox.EnqueueMessage($"Found target {{name = {controller.Info.Name}}}", 3);
                };
                controller.TargetLost += () =>
                {
                    MessageBox.EnqueueMessage($"Lost target {{name = {controller.Info.Name}}}", 3);
                };
                m.Controller = controller;
            }
        }

        public Optional<Vector3> HitTestOne(Vector2 pointInView)
        {
            var map = Maps.Select(m => m.Controller).Where(c => c && c.IsDirectlyTracked).FirstOrDefault();
            if (!map) { return Optional<Vector3>.Empty; }
            foreach (var point in map.HitTest(pointInView))
            {
                return map.transform.TransformPoint(point);
            }
            return Optional<Vector3>.Empty;
        }

        private void OnMapLoad(SparseSpatialMapController map, bool status, string error)
        {
            if (!map) { return; }
            mapLoadHandler.Where(h => h.Item1 == map).Select(h => h.Item2).SingleOrDefault()?.Invoke(status, error);
        }

        public class MapData
        {
            public MapMeta Meta;
            public SparseSpatialMapController Controller;
            public List<GameObject> Props = new List<GameObject>();
        }
    }

    public class BuildMapSession
    {
        public ARSession ARSession;
        public SparseSpatialMapBuilderFrameFilter Builder;

        public BuildMapSession(ARSession arSession)
        {
            ARSession = arSession;
            Builder = ARSession.GetComponentInChildren<SparseSpatialMapBuilderFrameFilter>();
            Builder.enabled = true;
        }

        public bool IsSaving { get; private set; }
        public bool Saved { get; private set; }

        public void Save(string name, Optional<Image> preview)
        {
            IsSaving = true;
            Builder.Host(name, preview, Optional<int>.Empty, (map, error) =>
            {
                if (map.OnSome)
                {
                    var mapMeta = new MapMeta(map.Value, new List<MapMeta.PropInfo>());
                    MapMetaManager.Save(mapMeta);
                    MessageBox.EnqueueMessage("Map Generated", 3);
                    MessageBox.EnqueueMessage("Notice: map name can be changed on website," + Environment.NewLine +
                        "while the SDK side will not get updated until map cache cleared and a re-download has been triggered.", 5);
                    Saved = true;
                }
                else
                {
                    MessageBox.EnqueueMessage("Fail to generate Map" + (error.OnNone ? "" : "\n error: " + error), 5);
                }
                IsSaving = false;
            });
        }
    }
}
