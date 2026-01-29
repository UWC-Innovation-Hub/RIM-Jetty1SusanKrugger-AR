//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace Sample
{
    [RequireComponent(typeof(MeshRenderer), typeof(UnityEngine.Video.VideoPlayer))]
    public class VideoPlayerAgent : MonoBehaviour
    {
        private UnityEngine.Video.VideoPlayer player;
        private bool playable = true;

        public bool Playable
        {
            get { return playable; }
            set
            {
                playable = value;
                StatusChanged();
            }
        }

        private void OnEnable()
        {
            StatusChanged();
        }

        private void Start()
        {
            player = GetComponent<UnityEngine.Video.VideoPlayer>();
            StatusChanged();
        }

        private void StatusChanged()
        {
            if (!player)
            {
                return;
            }
            if (playable)
            {
                player.Play();
            }
            else
            {
                player.Pause();
            }
        }
    }
}
