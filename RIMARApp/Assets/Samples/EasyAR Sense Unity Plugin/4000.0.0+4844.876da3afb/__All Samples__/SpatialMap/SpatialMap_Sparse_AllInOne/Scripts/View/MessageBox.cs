//================================================================================================================================
//
//  Copyright (c) 2020-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class MessageBox : MonoBehaviour
    {
        public RectTransform Box;

        private static MessageBox messageBox;
        private Queue<Tuple<string, float>> messageQueue = new Queue<Tuple<string, float>>();

        private void Awake()
        {
            messageBox = this;
            StartCoroutine(ShowMessage());
        }

        public static void EnqueueMessage(string message, float time)
        {
            if (!messageBox) { return; }
            messageBox.messageQueue.Enqueue(Tuple.Create(message, time));
        }

        public static void ClearMessages()
        {
            if (!messageBox) { return; }
            messageBox.messageQueue.Clear();
            messageBox.StopAllCoroutines();
            messageBox.Box.gameObject.SetActive(false);
            messageBox.StartCoroutine(messageBox.ShowMessage());
        }

        private IEnumerator ShowMessage()
        {
            while (true)
            {
                if (messageQueue.Count <= 0)
                {
                    yield return null;
                    continue;
                }

                var val = messageQueue.Peek();
                Box.gameObject.SetActive(true);
                var text = Box.GetComponentInChildren<UnityEngine.UI.Text>();
                text.text = val.Item1;
                yield return new WaitForSeconds(val.Item2);
                Box.gameObject.SetActive(false);
                messageQueue.Dequeue();
            }
        }
    }
}