//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sample
{
    public class Dragger : MonoBehaviour
    {
        public GameObject OutlinePrefab;
        public GameObject FreeMove;
        public UnityEngine.UI.Toggle VideoPlayable;
        public GameObject TouchRoot;

        private RectTransform rectTransform;
        private UnityEngine.UI.Image dummy;
        private TouchController touchControl;
        private TrackMapSession mapSession;
        private GameObject candidate;
        private GameObject selection;
        private bool isOnMap;
        private bool isMoveFree = true;

        public event Action<GameObject> CreateObject;
        public event Action<GameObject> DeleteObject;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            dummy = GetComponentInChildren<UnityEngine.UI.Image>(true);
            OutlinePrefab = Instantiate(OutlinePrefab);
            OutlinePrefab.SetActive(false);
        }

        private void Update()
        {
            if (mapSession == null || mapSession.ARSession.State < easyar.ARSession.SessionState.Ready) { return; }
            bool hasOneInput = false;
            bool hasOneInputTouchBegan = false;
            Vector3 inputPosition = new Vector3();
            bool isEditorOrStandalone = Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer;
            bool isPointerOverGameObject = (isEditorOrStandalone && EventSystem.current.IsPointerOverGameObject());
#if ENABLE_LEGACY_INPUT_MANAGER
            touchControl?.Update(Input.touches.ToDictionary(t => t.fingerId, t => t.position));
            inputPosition = Input.mousePosition;
            hasOneInput = Input.GetMouseButton(0) || Input.touchCount == 1;
            hasOneInputTouchBegan = Input.GetMouseButtonDown(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began);
            isPointerOverGameObject = isPointerOverGameObject || (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId));
#elif INPUTSYSTEM_PACKAGE_INSTALLED
            if (UnityEngine.InputSystem.Touchscreen.current != null)
            {
                var activeTouches = UnityEngine.InputSystem.Touchscreen.current.touches.Where(t => t.isInProgress).ToList();
                touchControl?.Update(activeTouches.ToDictionary(t => t.touchId.ReadValue(), t => t.position.ReadValue()));
                hasOneInput = activeTouches.Count == 1;
                hasOneInputTouchBegan = activeTouches.Count == 1 && activeTouches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began;
                inputPosition = activeTouches[0].position.ReadValue();
                isPointerOverGameObject = isPointerOverGameObject || (UnityEngine.InputSystem.Touchscreen.current.touches.Count > 0 && EventSystem.current.IsPointerOverGameObject(UnityEngine.InputSystem.Touchscreen.current.primaryTouch.touchId.ReadValue()));
            }
            else if (UnityEngine.InputSystem.Mouse.current != null)
            {
                hasOneInput = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
                hasOneInputTouchBegan = UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
                inputPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            }
            else
            {
                return;
            }
#endif
            transform.position = inputPosition;
            if (candidate)
            {
                if (!isPointerOverGameObject && hasOneInput)
                {
                    var point = mapSession.HitTestOne(new Vector2(inputPosition.x / Screen.width, inputPosition.y / Screen.height));
                    if (point.OnSome)
                    {
                        candidate.transform.position = point.Value + Vector3.up * candidate.transform.localScale.y / 2;
                        isOnMap = true;
                    }
                }

                if (isPointerOverGameObject || !isOnMap)
                {
                    HideCandidate();
                }
                else
                {
                    ShowCandidate();
                }
            }
            else
            {
                if (!isPointerOverGameObject && hasOneInputTouchBegan)
                {
                    var ray = mapSession.ARSession.Assembly.Camera.ScreenPointToRay(inputPosition);
                    if (Physics.Raycast(ray, out var hitInfo))
                    {
                        StopEdit();
                        StartEdit(hitInfo.collider.gameObject);
                    }
                }
            }

            if (selection && !isMoveFree)
            {
                if (!isPointerOverGameObject && hasOneInput)
                {
                    var point = mapSession.HitTestOne(new Vector2(inputPosition.x / Screen.width, inputPosition.y / Screen.height));
                    if (point.OnSome)
                    {
                        selection.transform.position = point.Value + Vector3.up * selection.transform.localScale.y / 2;
                    }
                }
            }
        }

        private void OnDisable()
        {
            mapSession = null;
            StopEdit();
        }

        public void SetMapSession(TrackMapSession session)
        {
            mapSession = session;
        }

        public void SetFreeMove(bool free)
        {
            isMoveFree = free;
            if (selection)
            {
                touchControl = new TouchController(selection.transform, mapSession.ARSession.Assembly.Camera, free, true, true);
            }
        }

        public void StartCreate(PropCellController controller)
        {
            StopEdit();
            isOnMap = false;
            rectTransform.sizeDelta = controller.GetComponent<RectTransform>().sizeDelta;
            dummy.sprite = controller.Templet.Icon;
            dummy.color = Color.white;
            candidate = Instantiate(controller.Templet.Object);
            candidate.name = controller.Templet.Object.name;
            if (candidate)
            {
                var video = candidate.GetComponentInChildren<VideoPlayerAgent>(true);
                if (video) { video.Playable = false; }
            }
            FreeMove.SetActive(false);
            HideCandidate();
        }

        public void StopCreate()
        {
            if (candidate.activeSelf)
            {
                CreateObject?.Invoke(candidate);
                StartEdit(candidate);
            }
            else
            {
                Destroy(candidate);
            }

            dummy.gameObject.SetActive(false);
            FreeMove.SetActive(true);
            isOnMap = false;
            candidate = null;
        }

        public void StartEdit(GameObject obj)
        {
            selection = obj;
            if (VideoPlayable.isOn)
            {
                ToggleVideoPlayable(true);
            }

            var meshFilter = selection.GetComponentInChildren<MeshFilter>();
            OutlinePrefab.SetActive(true);
            OutlinePrefab.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
            OutlinePrefab.transform.parent = meshFilter.transform;
            OutlinePrefab.transform.localPosition = Vector3.zero;
            OutlinePrefab.transform.localRotation = Quaternion.identity;
            OutlinePrefab.transform.localScale = Vector3.one;

            SetFreeMove(isMoveFree);
        }

        public void StopEdit()
        {
            ToggleVideoPlayable(false);
            selection = null;
            if (OutlinePrefab)
            {
                OutlinePrefab.transform.parent = null;
                OutlinePrefab.SetActive(false);
            }
            touchControl = null;
        }

        public void DeleteSelection()
        {
            if (!selection) { return; }
            DeleteObject?.Invoke(selection);
            Destroy(selection);
            StopEdit();
        }

        public void ToggleVideoPlayable(bool playable)
        {
            if (selection)
            {
                var video = selection.GetComponentInChildren<VideoPlayerAgent>(true);
                if (video) { video.Playable = playable; }
            }
        }

        private void ShowCandidate()
        {
            dummy.gameObject.SetActive(false);
            candidate.SetActive(true);
        }

        private void HideCandidate()
        {
            candidate.SetActive(false);
            dummy.gameObject.SetActive(true);
        }
    }
}
