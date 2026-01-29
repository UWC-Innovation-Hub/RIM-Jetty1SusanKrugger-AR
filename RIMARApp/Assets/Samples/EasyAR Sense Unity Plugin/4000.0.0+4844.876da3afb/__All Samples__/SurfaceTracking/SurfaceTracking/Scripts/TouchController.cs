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

namespace Sample
{
    public class TouchController
    {
        readonly Transform target;
        readonly Camera camera;
        readonly bool movable;
        readonly bool scalable;
        readonly bool rotatable;
        readonly Dictionary<int, (Vector2, Vector2)> points = new();
        Action step;
        Matrix4x4 rawTargetTransform;

        public TouchController(Transform target, Camera camera, bool movable, bool rotatable, bool scalable)
        {
            this.target = target;
            this.camera = camera;
            this.movable = movable;
            this.scalable = scalable;
            this.rotatable = rotatable;
        }

        public void Update(Dictionary<int, Vector2> touches)
        {
            if (!target || !camera) { return; }

            var oldKeys = points.Keys.ToList();
            foreach (var touch in touches)
            {
                points[touch.Key] = points.ContainsKey(touch.Key) ? (points[touch.Key].Item1, touch.Value) : (touch.Value, touch.Value);
            }
            foreach (var key in oldKeys.Where(k => !touches.Where(t => t.Key == k).Any()))
            {
                points.Remove(key);
            }

            if (points.Count == 0 || points.Count >= 3 || oldKeys.Where(key => !points.ContainsKey(key)).Any())
            {
                step = null;
            }
            else if (points.Count == 1)
            {
                if (step != Move1) { step = null; }
                if (step == null) { TryStart1(); }
            }
            else if (points.Count == 2)
            {
                if (step == Move1) { step = null; }
                if (step == null) { TryStart2(); }
            }
            step?.Invoke();
        }

        private void TryStart1()
        {
            if (!movable) { return; }
            rawTargetTransform = Matrix4x4.TRS(target.localPosition, target.localRotation, target.localScale);
            step = Move1;
        }

        private void TryStart2()
        {
            if (!movable && !rotatable && !scalable) { return; }
            const float gestureEnableDistanceThreshold = 10;
            var deltas = points.Values.Select(p => p.Item2 - p.Item1).ToList();
            if (deltas[0].magnitude <= gestureEnableDistanceThreshold || deltas[1].magnitude <= gestureEnableDistanceThreshold) { return; }
            rawTargetTransform = Matrix4x4.TRS(target.localPosition, target.localRotation, target.localScale);
            if (Vector2.Dot(deltas[0], deltas[1]) > 0)
            {
                (var xMov, var yMov) = Decompose(deltas[0] + deltas[1]);
                if (xMov.sqrMagnitude > yMov.sqrMagnitude)
                {
                    if (rotatable) { step = Rotate; }
                }
                else
                {
                    if (movable) { step = Move2; }
                }
            }
            else
            {
                if (scalable) { step = Scale; }
            }
        }

        private void Move1()
        {
            var deltas = points.Values.Select(p => p.Item2 - p.Item1).ToList();
            var delta = camera.transform.localToWorldMatrix.MultiplyVector(deltas[0] / new Vector2(Screen.width, Screen.height));
            var targetCamDistance = (camera.transform.position - target.position).magnitude;
            target.position = rawTargetTransform.GetPosition() + delta * targetCamDistance;
        }

        private void Move2()
        {
            var deltas = points.Values.Select(p => p.Item2 - p.Item1).ToList();
            (_, var yMovement) = Decompose(deltas[0] + deltas[1]);
            if (yMovement == Vector3.zero) { return; }

            var camForward = camera.transform.forward;
            var scale = Vector3.Dot(yMovement, camForward) > 0 ? 1 : -1;
            var targetCamDistance = (camera.transform.position - target.position).magnitude;
            target.position = rawTargetTransform.GetPosition() + scale * Vector3.ProjectOnPlane(camForward, Vector3.up) * yMovement.magnitude * targetCamDistance * 2 / 1000;
        }

        private void Rotate()
        {
            const float rotateSpeed = 270;
            var deltas = points.Values.Select(p => p.Item2 - p.Item1).ToList();
            (var xMovement, _) = Decompose((deltas[0] + deltas[1]) * 0.5f / Screen.width * rotateSpeed);
            if (xMovement == Vector3.zero) { return; }

            var scale = Vector3.Dot(Vector3.Cross(xMovement.normalized, Vector3.up), camera.transform.forward) < 0f ? 1 : -1;
            target.rotation = rawTargetTransform.rotation * Quaternion.Euler(0f, xMovement.sqrMagnitude * scale / Mathf.PI, 0f);
        }

        private void Scale()
        {
            var rawPoints = points.Values.Select(p => p.Item1).ToList();
            var curPoints = points.Values.Select(p => p.Item2).ToList();
            float scaleFactor = Vector2.Distance(rawPoints[0], rawPoints[1]) / Vector2.Distance(curPoints[0], curPoints[1]);
            target.localScale = rawTargetTransform.lossyScale / scaleFactor;
        }

        private (Vector3 x, Vector3 y) Decompose(Vector2 delta)
        {
            if (delta == Vector2.zero) { return (Vector3.zero, Vector3.zero); }

            var start = camera.ScreenToWorldPoint(new Vector3(0f, 0f, 300f));
            var end = camera.ScreenToWorldPoint(new Vector3(delta.x, delta.y, 300f));
            var direction = end - start;
            var rForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            if (rForward == Vector3.zero) { return (direction, Vector3.zero); }

            var rRight = Vector3.Cross(Vector3.up, rForward);
            var xMovement = Vector3.Project(direction, rRight);
            var temp = direction - xMovement;
            if (temp == Vector3.zero) { return (xMovement, Vector3.zero); }

            var yMovement = temp.magnitude * ((Vector3.Dot((Vector3.Dot(Vector3.up, temp.normalized) == 0 ? rForward : Vector3.up), temp.normalized) > 0 ? rForward : -rForward));
            return (xMovement, yMovement);
        }
    }
}
