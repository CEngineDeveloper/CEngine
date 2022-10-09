//------------------------------------------------------------------------------
// BaseSLGCamera.cs
// Copyright 2018 2018/11/13 
// Created by CYM on 2018/11/13
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Cam
{
    public class BaseSLGCamera3DMgr : BaseSLGCameraMgr
    {
        #region prop
        protected RTSCamera3D RTSCamera { get; private set; }
        public override float ScrollVal
        {
            get
            {
                if (RTSCamera == null)
                    return 0;
                return RTSCamera.ScrollValue;
            }
        }
        DBBaseSettings DBSettings => BaseGlobal.Settings;
        #endregion

        #region life
        public override bool IsMoving => RTSCamera.IsMoving;
        public override float HightPercent => Mathf.Clamp01((CameraHight - RTSCamera.MinHight) / (RTSCamera.MaxHight - RTSCamera.MinHight));
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (RTSCamera == null) return;
            RTSCamera.DragSpeed(DragSpeed * DBSettings.CameraMoveSpeed);
            RTSCamera.ScreenEdgeSpeed(ScreenEdgeSpeed * DBSettings.CameraMoveSpeed);
            RTSCamera.ScrollSpeed(ScrollSpeed * DBSettings.CameraScrollSpeed);
            RTSCamera.RotateSpeed(RotateSpeed);
            RTSCamera.ScreenEdgeMoveControl(IsScreenEdgeControl);
            RTSCamera.ScrollControl(IsScrollControl);
            RTSCamera.DragControl(IsDragControl);
            RTSCamera.RotateControl(IsRotateControl);
            RTSCamera.AllControl(IsControl);
        }
        public override void FetchCamera()
        {
            base.FetchCamera();
            if (MainCamera != null)
            {
                RTSCamera = MainCamera.GetComponentInChildren<RTSCamera3D>();
                if(RTSCamera==null)
                    RTSCamera = MainCamera.gameObject.AddComponent<RTSCamera3D>();
            }
        }
        #endregion

        #region jump
        public override void Jump(Vector3 pos, float? heightPercent = 0.05f)
        {
            RTSCamera.JumpTo(pos);
            if (heightPercent != null)
                RTSCamera.SetScroll(heightPercent.Value);
        }
        #endregion

        #region set
        public override void Enable(bool b)
        {
            base.Enable(b);
            if (RTSCamera)
            {
                RTSCamera.enabled = b;
            }
        }
        public void SetMinMaxHeight(float min, float max)
        {
            RTSCamera?.SetMinMaxHeight(min, max);
        }
        public void SetGroundTest(bool isTest)
        {
            RTSCamera?.SetGroundTest(isTest);
        }
        public void Move(Vector3 dir)
        {
            RTSCamera?.Move(dir);
        }
        public void Follow(BaseUnit target)
        {
            RTSCamera?.Follow(target.Trans);
        }
        public void CancleFollow()
        {
            RTSCamera?.CancelFollow();
        }
        public void SetBound(float minX,float maxX,float minY,float maxY)
        {
            RTSCamera?.SetBound(new List<float>() { minX, minY, maxX, maxY });
        }
        public void SetBound(List<float> data)
        {
            RTSCamera?.SetBound(data);
        }
        #endregion
    }
}