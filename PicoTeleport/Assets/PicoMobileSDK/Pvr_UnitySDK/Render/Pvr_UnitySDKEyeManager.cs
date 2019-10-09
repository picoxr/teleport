///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights 
// File: Pvr_UnitySDKEyeManager
// Author: AiLi.Shang
// Date:  2017/01/18
// Discription:  Controller of cameras . Be fully careful of  Code modification
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

public class Pvr_UnitySDKEyeManager : MonoBehaviour
{
    public bool isfirst = true;
	private int framenum = 0;

    private int RenderLayersMax = 4;
    /************************************    Properties  *************************************/
    #region Properties
    private Pvr_UnitySDKEye[] eyes = null;
    public Pvr_UnitySDKEye[] Eyes
    {
        get
        {
            if (eyes == null)
            {
                eyes = GetComponentsInChildren<Pvr_UnitySDKEye>(true).ToArray();
            }
            return eyes;
        }
    }

    // StandTexture Overlay
    private Pvr_UnitySDKEyeOverlay[] overlays = null;
    public Pvr_UnitySDKEyeOverlay[] Overlays
    {
        get
        {
            if (overlays == null)
            {
                //overlays = GetComponentsInChildren<Pvr_UnitySDKEyeOverlay>(true).ToArray();
                overlays = Pvr_UnitySDKEyeOverlay.Instances.ToArray();
            }
            return overlays;
        }
    }



    public Camera ControllerCamera
    {
        get
        {
            return GetComponent<Camera>();
        }
    }

    private bool renderedStereo = false;

    private int ScreenHeight
    {
        get
        {
            return Screen.height - (Application.isEditor ? 36 : 0);
        }
    }

    #endregion

    /************************************ Process Interface  *********************************/
    #region  Process Interface
    public void AddStereoRig()
    {
        if (Eyes.Length > 0)
        {
            return;
        }
        CreateEye(Pvr_UnitySDKAPI.Eye.LeftEye);
        CreateEye(Pvr_UnitySDKAPI.Eye.RightEye);
    }

    private void CreateEye(Pvr_UnitySDKAPI.Eye eyeSide)
    {
        string nm = name + (eyeSide == Pvr_UnitySDKAPI.Eye.LeftEye ? " LeftEye" : " RightEye");
        GameObject go = new GameObject(nm);
        go.transform.parent = transform;
        go.AddComponent<Camera>().enabled = true;
#if !UNITY_5
    if (GetComponent<GUILayer>() != null) {
      go.AddComponent<GUILayer>();
    }
    if (GetComponent("FlareLayer") != null) {
      go.AddComponent<FlareLayer>();
    }
#endif
        var picovrEye = go.AddComponent<Pvr_UnitySDKEye>();
        picovrEye.eyeSide = eyeSide;
    }

    private void FillScreenRect(int width, int height, Color color)
    {
        int x = Screen.width / 2;
        int y = Screen.height / 2 - 15;
        width /= 2;
        height /= 2;
        Pvr_UnitySDKManager.SDK.Middlematerial.color = color;
        Pvr_UnitySDKManager.SDK.Middlematerial.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix();
        GL.Color(Color.white);
        GL.Begin(GL.QUADS);
        GL.Vertex3(x - width, y - height, 0);
        GL.Vertex3(x - width, y + height, 0);
        GL.Vertex3(x + width, y + height, 0);
        GL.Vertex3(x + width, y - height, 0);
        GL.End();
        GL.PopMatrix();

    }
    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API
    void Awake()
    {
        AddStereoRig();
    }

    void OnEnable()
    {
        StartCoroutine("EndOfFrame");
    }

    void Update()
    {

        ControllerCamera.enabled = !Pvr_UnitySDKManager.SDK.VRModeEnabled;
#if UNITY_EDITOR
        for (int i = 0; i < Eyes.Length; i++)
        {
            Eyes[i].eyecamera.enabled = Pvr_UnitySDKManager.SDK.VRModeEnabled;
        } 
#endif

        if (!Pvr_UnitySDKManager.SDK.IsViewerLogicFlow)
        {
            for (int i = 0; i < Eyes.Length; i++)
            {
                Eyes[i].EyeRender();
            }
        }
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }


#if UNITY_EDITOR
    private void OnGUI()
    {
        Pvr_UnitySDKEyeOverlay.Instances.Sort();
        foreach (var eyeOverlay in Pvr_UnitySDKEyeOverlay.Instances)
        {
            if (!eyeOverlay.isActiveAndEnabled) continue;
            if (eyeOverlay.imageTexture == null) continue;
            if (eyeOverlay.imageTransform != null && !eyeOverlay.imageTransform.gameObject.activeSelf) continue;
            if (eyeOverlay.imageTransform != null && !eyeOverlay.imageTransform.IsChildOf(this.transform.parent)) continue;

            Rect textureRect = new Rect(0, 0, 1, 1);

            Vector2 leftCenter = new Vector2(Screen.width * 0.25f, Screen.height * 0.5f);
            Vector2 rightCenter = new Vector2(Screen.width * 0.75f, Screen.height * 0.5f);
            Vector2 eyeExtent = new Vector3(Screen.width * 0.25f, Screen.height * 0.5f);
            eyeExtent.x -= 100.0f;
            eyeExtent.y -= 100.0f;

            Rect leftScreen = Rect.MinMaxRect(
                leftCenter.x - eyeExtent.x,
                leftCenter.y - eyeExtent.y,
                leftCenter.x + eyeExtent.x,
                leftCenter.y + eyeExtent.y);
            Rect rightScreen = Rect.MinMaxRect(
                rightCenter.x - eyeExtent.x,
                rightCenter.y - eyeExtent.y,
                rightCenter.x + eyeExtent.x,
                rightCenter.y + eyeExtent.y);

            var eyeRectMin = eyeOverlay.clipLowerLeft; eyeRectMin /= eyeRectMin.w;
            var eyeRectMax = eyeOverlay.clipUpperRight; eyeRectMax /= eyeRectMax.w;

            if (eyeOverlay.eyeSide == Pvr_UnitySDKAPI.Eye.LeftEye)
            {
                leftScreen = Rect.MinMaxRect(
                        leftCenter.x + eyeExtent.x * eyeRectMin.x,
                        leftCenter.y + eyeExtent.y * eyeRectMin.y,
                        leftCenter.x + eyeExtent.x * eyeRectMax.x,
                        leftCenter.y + eyeExtent.y * eyeRectMax.y);

                Graphics.DrawTexture(leftScreen, eyeOverlay.imageTexture, textureRect, 0, 0, 0, 0);
            }
            else if (eyeOverlay.eyeSide == Pvr_UnitySDKAPI.Eye.RightEye)
            {
                rightScreen = Rect.MinMaxRect(
                       rightCenter.x + eyeExtent.x * eyeRectMin.x,
                       rightCenter.y + eyeExtent.y * eyeRectMin.y,
                       rightCenter.x + eyeExtent.x * eyeRectMax.x,
                       rightCenter.y + eyeExtent.y * eyeRectMax.y);

                Graphics.DrawTexture(rightScreen, eyeOverlay.imageTexture, textureRect, 0, 0, 0, 0);
            }
        }
    }
#endif
    #endregion

    /************************************  End Of Per Frame  *************************************/
    IEnumerator EndOfFrame()
    {
        float[] lowerLeft = new float[6];
        float[] upperLeft = new float[6];
        float[] upperRight = new float[6];
        float[] lowerRight = new float[6];

        List<int> leftEyeEnableLayers = new List<int>();
        List<int> rightEyeEnableLayers = new List<int>();

        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (isfirst && framenum == 3)
            {
                Pvr_UnitySDKAPI.System.UPvr_RemovePlatformLogo();
                Pvr_UnitySDKAPI.System.UPvr_StartVRModel();
                isfirst = false;
            }
            else if (isfirst && framenum < 3)
            {
                Debug.Log("+++++++++++++++++++++++++++++++" + framenum);
                framenum++;
            }

            // if find Overlay then Open Composition Layers feature
            if (Pvr_UnitySDKEyeOverlay.Instances.Count > 0)
            {
                #region Composition Layers
                // clera
                leftEyeEnableLayers.Clear();
                rightEyeEnableLayers.Clear();

                // for eyebuffer
                int eyeTextureId = 0;
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (!Eyes[i].isActiveAndEnabled) continue;

                    #region LL UL UR LR
                    // LL
                    lowerLeft[0] = Eyes[i].clipLowerLeft.x;
                    lowerLeft[1] = Eyes[i].clipLowerLeft.y;
                    lowerLeft[2] = Eyes[i].clipLowerLeft.z;
                    lowerLeft[3] = Eyes[i].clipLowerLeft.w;
                    lowerLeft[4] = Eyes[i].uvLowerLeft.x;
                    lowerLeft[5] = Eyes[i].uvLowerLeft.y;
                    // UL
                    upperLeft[0] = Eyes[i].clipUpperLeft.x;
                    upperLeft[1] = Eyes[i].clipUpperLeft.y;
                    upperLeft[2] = Eyes[i].clipUpperLeft.z;
                    upperLeft[3] = Eyes[i].clipUpperLeft.w;
                    upperLeft[4] = Eyes[i].uvUpperLeft.x;
                    upperLeft[5] = Eyes[i].uvUpperLeft.y;
                    // UR
                    upperRight[0] = Eyes[i].clipUpperRight.x;
                    upperRight[1] = Eyes[i].clipUpperRight.y;
                    upperRight[2] = Eyes[i].clipUpperRight.z;
                    upperRight[3] = Eyes[i].clipUpperRight.w;
                    upperRight[4] = Eyes[i].uvUpperRight.x;
                    upperRight[5] = Eyes[i].uvUpperRight.y;
                    // LR
                    lowerRight[0] = Eyes[i].clipLowerRight.x;
                    lowerRight[1] = Eyes[i].clipLowerRight.y;
                    lowerRight[2] = Eyes[i].clipLowerRight.z;
                    lowerRight[3] = Eyes[i].clipLowerRight.w;
                    lowerRight[4] = Eyes[i].uvLowerRight.x;
                    lowerRight[5] = Eyes[i].uvLowerRight.y;
                    #endregion

                    switch (Eyes[i].eyeSide)
                    {
                        case Pvr_UnitySDKAPI.Eye.LeftEye:
                            eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            break;
                        case Pvr_UnitySDKAPI.Eye.RightEye:
                            eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx + 3];
                            break;
                        default:
                            break;
                    }


                    Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(Eyes[i].layerIndex, (int)Eyes[i].eyeSide, eyeTextureId, 0, 0);
                    Pvr_UnitySDKAPI.Render.UPvr_SetupLayerCoords(Eyes[i].layerIndex, (int)Eyes[i].eyeSide, lowerLeft, lowerRight, upperLeft, upperRight);

                    this.RecordEnableLayers(Eyes[i].eyeSide, Eyes[i].layerIndex, ref leftEyeEnableLayers, ref rightEyeEnableLayers);
                }

                // for overlay ：current only support one layer
                for (int i = 0; i < Overlays.Length; i++)
                {
                    if (!Overlays[i].isActiveAndEnabled) continue;
                    if (Overlays[i].imageTexture == null) continue;
                    if (Overlays[i].imageTransform != null && !Overlays[i].imageTransform.gameObject.activeSelf) continue;

                    #region LL UL UR LR
                    // LL
                    lowerLeft[0] = Overlays[i].clipLowerLeft.x;
                    lowerLeft[1] = Overlays[i].clipLowerLeft.y;
                    lowerLeft[2] = Overlays[i].clipLowerLeft.z;
                    lowerLeft[3] = Overlays[i].clipLowerLeft.w;
                    lowerLeft[4] = Overlays[i].uvLowerLeft.x;
                    lowerLeft[5] = Overlays[i].uvLowerLeft.y;
                    // UL
                    upperLeft[0] = Overlays[i].clipUpperLeft.x;
                    upperLeft[1] = Overlays[i].clipUpperLeft.y;
                    upperLeft[2] = Overlays[i].clipUpperLeft.z;
                    upperLeft[3] = Overlays[i].clipUpperLeft.w;
                    upperLeft[4] = Overlays[i].uvUpperLeft.x;
                    upperLeft[5] = Overlays[i].uvUpperLeft.y;
                    // UR
                    upperRight[0] = Overlays[i].clipUpperRight.x;
                    upperRight[1] = Overlays[i].clipUpperRight.y;
                    upperRight[2] = Overlays[i].clipUpperRight.z;
                    upperRight[3] = Overlays[i].clipUpperRight.w;
                    upperRight[4] = Overlays[i].uvUpperRight.x;
                    upperRight[5] = Overlays[i].uvUpperRight.y;
                    // LR
                    lowerRight[0] = Overlays[i].clipLowerRight.x;
                    lowerRight[1] = Overlays[i].clipLowerRight.y;
                    lowerRight[2] = Overlays[i].clipLowerRight.z;
                    lowerRight[3] = Overlays[i].clipLowerRight.w;
                    lowerRight[4] = Overlays[i].uvLowerRight.x;
                    lowerRight[5] = Overlays[i].uvLowerRight.y;
                    #endregion

                    Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(Overlays[i].layerIndex, (int)Overlays[i].eyeSide, Overlays[i].ImageTextureId, 0, 0);
                    Pvr_UnitySDKAPI.Render.UPvr_SetupLayerCoords(Overlays[i].layerIndex, (int)Overlays[i].eyeSide, lowerLeft, lowerRight, upperLeft, upperRight);

                    this.RecordEnableLayers(Overlays[i].eyeSide, Overlays[i].layerIndex, ref leftEyeEnableLayers, ref rightEyeEnableLayers);
                }

                for (int index = 0; index < this.RenderLayersMax; index++)
                {
                    // Left Layers
                    if (!leftEyeEnableLayers.Contains(index))
                    {
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(index, (int)Pvr_UnitySDKAPI.Eye.LeftEye, 0, 0, 0);
                    }

                    // Right Layers
                    if (!rightEyeEnableLayers.Contains(index))
                    {
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(index, (int)Pvr_UnitySDKAPI.Eye.RightEye, 0, 0, 0);
                    }
                }
                #endregion
            }

            Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.TimeWarp, Pvr_UnitySDKManager.SDK.RenderviewNumber);
            Pvr_UnitySDKManager.SDK.currEyeTextureIdx = Pvr_UnitySDKManager.SDK.nextEyeTextureIdx;
            Pvr_UnitySDKManager.SDK.nextEyeTextureIdx = (Pvr_UnitySDKManager.SDK.nextEyeTextureIdx + 1) % 3;
        }
    }

    private void RecordEnableLayers(Pvr_UnitySDKAPI.Eye eyeSide, int layerIndex, ref List<int> lEnableLayers, ref List<int> rEnableLayers)
    {
        switch (eyeSide)
        {
            case Pvr_UnitySDKAPI.Eye.LeftEye:
                if (lEnableLayers.Contains(layerIndex))
                {
                    Debug.LogError(string.Format("LeftEye layerIndex:{0} is already exist! Don't add the same layerIndex more than once!", layerIndex));
                    return;
                }
                lEnableLayers.Add(layerIndex);
                break;
            case Pvr_UnitySDKAPI.Eye.RightEye:
                if (rEnableLayers.Contains(layerIndex))
                {
                    Debug.LogError(string.Format("RightEye layerIndex:{0} is already exist!  Don't add the same layerIndex more than once!", layerIndex));
                    return;
                }
                rEnableLayers.Add(layerIndex);
                break;
        }
    }
}