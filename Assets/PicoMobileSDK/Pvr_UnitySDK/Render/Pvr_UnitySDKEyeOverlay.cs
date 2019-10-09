///////////////////////////////////////////////////////////////////////////////
// Copyright 2019-2020  Pico Technology Co., Ltd. All Rights 
// File:			Pvr_Overlay.cs
// Author: 			PICO
// Create Date:  	#CREATETIME#
// Change Date:		#CHANGETIME#
// Discription: The API Core funcation.Be fully careful of  Code modification
///////////////////////////////////////////////////////////////////////////////
#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IOS_DEVICE
#elif UNITY_STANDALONE_WIN
#define WIN_DEVICE
#endif
#endif
using Pvr_UnitySDKAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Pvr_UnitySDKEyeOverlay : MonoBehaviour, IComparable<Pvr_UnitySDKEyeOverlay>
{
    public static List<Pvr_UnitySDKEyeOverlay> Instances = new List<Pvr_UnitySDKEyeOverlay>();

    public Eye eyeSide;
    public int layerIndex = 0;
    public ImageType imageType = ImageType.StandardTexture;
    public Texture2D imageTexture;
    // Donn't modify at Runtime
    public Transform imageTransform;

    // camera clip space
    public Vector4 clipLowerLeft = new Vector4(-1, -1, 0, 1);
    public Vector4 clipUpperLeft = new Vector4(-1, 1, 0, 1);
    public Vector4 clipUpperRight = new Vector4(1, 1, 0, 1);
    public Vector4 clipLowerRight = new Vector4(1, -1, 0, 1);
    // texture uv space
    public Vector2 uvLowerLeft = new Vector2(0, 0);
    public Vector2 uvUpperLeft = new Vector2(0, 1);
    public Vector2 uvUpperRight = new Vector2(1, 1);
    public Vector2 uvLowerRight = new Vector2(1, 0);

    public int ImageTextureId { get; set; }

    private Camera eyeCamera = null;

    public int CompareTo(Pvr_UnitySDKEyeOverlay other)
    {
        return this.layerIndex.CompareTo(other.layerIndex);
    }


    #region Unity Methods
    private void Awake()
    {
        Instances.Add(this);
        this.eyeCamera = this.GetComponent<Camera>();
        this.InitializeBuffer();
        this.InitializeCoords();
    }

    private void LateUpdate()
    {
        this.UpdateCoords();
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }
    #endregion


    private void InitializeBuffer()
    {
        switch (this.imageType)
        {
            case ImageType.StandardTexture:
                if (this.imageTexture)
                {
                    this.ImageTextureId = this.imageTexture.GetNativeTexturePtr().ToInt32();
                }
                break;
            case ImageType.EglTexture:
                this.ImageTextureId = 0;
                break;
            case ImageType.EquirectangularTexture:
                if (this.imageTexture)
                {
                    this.ImageTextureId = this.imageTexture.GetNativeTexturePtr().ToInt32();
                }
                break;
            default:
                break;
        }
    }

    private void InitializeCoords()
    {
        clipLowerLeft.Set(-1, -1, 0, 1);
        clipUpperLeft.Set(-1, 1, 0, 1);
        clipUpperRight.Set(1, 1, 0, 1);
        clipLowerRight.Set(1, -1, 0, 1);
    }

    private void UpdateCoords()
    {
        if (this.imageTransform == null || !this.imageTransform.gameObject.activeSelf)
        {
            return;
        }

        if (this.eyeCamera == null)
        {
            return;
        }

        var extents = 0.5f * Vector3.one;
        var center = Vector3.zero;

        var worldLowerLeft = new Vector4(center.x - extents.x, center.y - extents.y, 0, 1);
        var worldUpperLeft = new Vector4(center.x - extents.x, center.y + extents.y, 0, 1);
        var worldUpperRight = new Vector4(center.x + extents.x, center.y + extents.y, 0, 1);
        var worldLowerRight = new Vector4(center.x + extents.x, center.y - extents.y, 0, 1);

        Matrix4x4 MVP = eyeCamera.projectionMatrix * eyeCamera.worldToCameraMatrix * imageTransform.localToWorldMatrix;

        clipLowerLeft = MVP * worldLowerLeft;
        clipUpperLeft = MVP * worldUpperLeft;
        clipUpperRight = MVP * worldUpperRight;
        clipLowerRight = MVP * worldLowerRight;
    }




    #region Public Method

    public void SetTexture(Texture2D texture)
    {
        this.imageTexture = texture;
        this.InitializeBuffer();
    }

    #endregion




    public enum ImageType
    {
        //RenderTexture = 0,
        StandardTexture,
        EglTexture,
        EquirectangularTexture
    }
}
