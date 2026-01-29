//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sample
{
    public class MapDockController : MonoBehaviour
    {
        public Button OpenButton;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            bool isClickBegan = false;
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                isClickBegan = Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject();
#elif INPUTSYSTEM_PACKAGE_INSTALLED
                isClickBegan = UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject();
#endif
            }
            else
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                isClickBegan = Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#elif INPUTSYSTEM_PACKAGE_INSTALLED
                if (UnityEngine.InputSystem.Touchscreen.current != null)
                {
                    var activeTouches = UnityEngine.InputSystem.Touchscreen.current.touches.Where(t => t.isInProgress).ToList();
                    isClickBegan = activeTouches.Count > 0 && activeTouches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(activeTouches[0].touchId.ReadValue());
                }
#endif
            }
            if (isClickBegan)
            {
                ShowAndHide(OpenButton.gameObject.activeSelf);
            }
        }

        public void ShowAndHide(bool isShow)
        {
            StopAllCoroutines();
            if (isShow)
            {
                StartCoroutine(Show());
            }
            else
            {
                StartCoroutine(Hide());
            }
        }

        private IEnumerator Show()
        {
            var offsetMax = rectTransform.offsetMax;
            var offsetMin = rectTransform.offsetMin;
            OpenButton.gameObject.SetActive(false);
            while (offsetMax.x < 0)
            {
                offsetMax.x += Screen.width * Time.deltaTime;
                offsetMin.x += Screen.width * Time.deltaTime;
                rectTransform.offsetMax = offsetMax;
                rectTransform.offsetMin = offsetMin;
                if (offsetMax.x > 0)
                {
                    offsetMax.x = 0;
                    offsetMin.x = 0;
                    rectTransform.offsetMax = offsetMax;
                    rectTransform.offsetMin = offsetMin;
                }
                yield return 0;
            }
        }

        private IEnumerator Hide()
        {
            var offsetMax = rectTransform.offsetMax;
            var offsetMin = rectTransform.offsetMin;
            var width = rectTransform.rect.width + Screen.width * 0.01f;
            while (offsetMax.x > -width && offsetMin.x > -width)
            {
                offsetMax.x -= Screen.width * Time.deltaTime;
                offsetMin.x -= Screen.width * Time.deltaTime;
                rectTransform.offsetMax = offsetMax;
                rectTransform.offsetMin = offsetMin;
                if (offsetMax.x < -width)
                {
                    offsetMax.x = -width;
                    offsetMin.x = -width;
                    rectTransform.offsetMax = offsetMax;
                    rectTransform.offsetMin = offsetMin;
                }
                yield return 0;
            }
            OpenButton.gameObject.SetActive(true);
        }
    }
}
