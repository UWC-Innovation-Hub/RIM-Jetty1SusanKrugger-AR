//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class SessionRestarter : MonoBehaviour
    {
        private ARSession session;

        private void Awake()
        {
            var dropdown = GetComponentInChildren<Dropdown>();
            if (!dropdown) { return; }
            dropdown.onValueChanged.AddListener(RestartSessionWithOption);
            session =
#if UNITY_2022_3_OR_NEWER
                FindAnyObjectByType<ARSession>();
#else
                FindObjectOfType<ARSession>();
#endif
            if (!session) { return; }

            session.StateChanged += (state) =>
            {
                if (state != ARSession.SessionState.Ready) { return; }
                var text = GetComponentsInChildren<Text>().Where(t => t.name == "CurrentFrameSource").FirstOrDefault();
                if (!text) { return; }
                text.text = ARSessionFactory.DefaultName(session.Assembly.FrameSource.GetType());
            };
        }

        public void RestartSessionWithOption(int idx)
        {
            if (!session) { return; }
            session.StopSession(true);
            ARSessionFactory.SortFrameSource(session.gameObject, idx switch
            {
                0 => new ARSessionFactory.FrameSourceSortMethod { ARCore = ARSessionFactory.FrameSourceSortMethod.ARCoreSortMethod.PreferEasyAR, ARKit = ARSessionFactory.FrameSourceSortMethod.ARKitSortMethod.PreferEasyAR, MotionTracker = ARSessionFactory.FrameSourceSortMethod.MotionTrackerSortMethod.PreferSystem },
                1 => new ARSessionFactory.FrameSourceSortMethod { ARCore = ARSessionFactory.FrameSourceSortMethod.ARCoreSortMethod.PreferARFoundation, ARKit = ARSessionFactory.FrameSourceSortMethod.ARKitSortMethod.PreferARFoundation, MotionTracker = ARSessionFactory.FrameSourceSortMethod.MotionTrackerSortMethod.PreferSystem },
                2 => new ARSessionFactory.FrameSourceSortMethod { ARCore = ARSessionFactory.FrameSourceSortMethod.ARCoreSortMethod.PreferEasyAR, ARKit = ARSessionFactory.FrameSourceSortMethod.ARKitSortMethod.PreferEasyAR, MotionTracker = ARSessionFactory.FrameSourceSortMethod.MotionTrackerSortMethod.PreferEasyAR },
                _ => throw new ArgumentOutOfRangeException(),
            });
            session.StartSession();
        }
    }
}
