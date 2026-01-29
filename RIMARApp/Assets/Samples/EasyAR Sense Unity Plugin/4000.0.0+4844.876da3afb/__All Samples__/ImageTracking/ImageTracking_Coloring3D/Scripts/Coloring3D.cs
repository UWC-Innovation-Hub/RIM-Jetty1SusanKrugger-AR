//================================================================================================================================
//
//  Copyright (c) 2015-2025 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using easyar;
using UnityEngine;
using UnityEngine.UI;

public class Coloring3D : MonoBehaviour
{
    public ARSession Session;
    public Button ButtonChange;
    public ImageTargetController ImageTarget;
    public MeshRenderer MeshRenderer;

    private Text buttonText;
    private RenderTexture renderTexture;
    private bool? freezed;
    private RenderTexture freezedTexture;

    private void Awake()
    {
        AdaptInputSystem();
        buttonText = ButtonChange.GetComponentInChildren<Text>();
        Session.StateChanged += (state) =>
        {
            if (state == ARSession.SessionState.Ready)
            {
                Session.Assembly.CameraImageRenderer.Value.RequestTargetTexture((_, texture) => renderTexture = texture);
            }
        };
        ImageTarget.TargetFound += () =>
        {
            if (!freezed.HasValue)
            {
                buttonText.text = "Freeze";
                freezed = false;
            }
            ButtonChange.interactable = true;
        };
        ImageTarget.TargetLost += () =>
        {
            if (!this || !ButtonChange) { return; }
            ButtonChange.interactable = false;
        };
        ButtonChange.onClick.AddListener(() =>
        {
            if (freezed.Value)
            {
                freezed = false;
                buttonText.text = "Freeze";
                if (freezedTexture) { Destroy(freezedTexture); }
            }
            else
            {
                freezed = true;
                buttonText.text = "Thaw";
                if (freezedTexture) { Destroy(freezedTexture); }
                if (renderTexture)
                {
                    freezedTexture = new RenderTexture(renderTexture.width, renderTexture.height, 0);
                    Graphics.Blit(renderTexture, freezedTexture);
                }
                MeshRenderer.material.mainTexture = freezedTexture;
            }
        });
    }

    private void Update()
    {
        if (!freezed.HasValue || freezed.Value || ImageTarget.Target == null) { return; }

        var halfWidth = 0.5f;
        var halfHeight = 0.5f / ImageTarget.Target.aspectRatio();
        Matrix4x4 points = Matrix4x4.identity;
        Vector3 targetVertex1 = ImageTarget.transform.TransformPoint(new Vector3(-halfWidth, halfHeight, 0));
        Vector3 targetVertex2 = ImageTarget.transform.TransformPoint(new Vector3(-halfWidth, -halfHeight, 0));
        Vector3 targetVertex3 = ImageTarget.transform.TransformPoint(new Vector3(halfWidth, halfHeight, 0));
        Vector3 targetVertex4 = ImageTarget.transform.TransformPoint(new Vector3(halfWidth, -halfHeight, 0));
        points.SetRow(0, new Vector4(targetVertex1.x, targetVertex1.y, targetVertex1.z, 1f));
        points.SetRow(1, new Vector4(targetVertex2.x, targetVertex2.y, targetVertex2.z, 1f));
        points.SetRow(2, new Vector4(targetVertex3.x, targetVertex3.y, targetVertex3.z, 1f));
        points.SetRow(3, new Vector4(targetVertex4.x, targetVertex4.y, targetVertex4.z, 1f));
        MeshRenderer.material.SetMatrix("_UvPints", points);
        MeshRenderer.material.SetMatrix("_RenderingViewMatrix", Camera.main.worldToCameraMatrix);
        MeshRenderer.material.SetMatrix("_RenderingProjectMatrix", GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false));
        MeshRenderer.material.SetTexture("_MainTex", renderTexture);
    }

    private void OnDestroy()
    {
        if (freezedTexture) { Destroy(freezedTexture); }
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
