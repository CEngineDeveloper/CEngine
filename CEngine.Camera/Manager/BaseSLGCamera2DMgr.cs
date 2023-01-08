//------------------------------------------------------------------------------
// BaseSLGCamera2DMgr.cs
// Copyright 2019 2019/12/11 
// Created by CYM on 2019/12/11
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;

namespace CYM.Cam
{
    public class BaseSLGCamera2DMgr : BaseSLGCameraMgr
    {
        #region prop
        public override float ScrollVal
        {
            get
            {
                if (RTSCamera == null)
                    return .0f;
                return RTSCamera.ScrollValue; 
            }
        }
        protected RTSCamera2D RTSCamera { get; private set; }
        DBBaseSettings DBSettings => BaseGlobal.Settings;
        #endregion

        #region life
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
                RTSCamera = MainCamera.GetComponentInChildren<RTSCamera2D>();
                if (RTSCamera == null)
                    RTSCamera = MainCamera.gameObject.AddComponent<RTSCamera2D>();
            }
        }
        #endregion

        #region set
        public override void SetPos(Vector3 pos)
        {
            pos = pos.SetZ(-2);
            base.SetPos(pos);
        }
        public override void Enable(bool b)
        {
            base.Enable(b);
            if(RTSCamera!=null)
                RTSCamera.enabled = b;
        }
        public override void Jump(Vector3 pos, float? heightPercent = null)
        {
            RTSCamera?.JumpTo(pos);
            if(heightPercent.HasValue)
                RTSCamera?.SetScroll(heightPercent.Value);
        }
        public override void SetScroll(float val)
        {
            RTSCamera?.SetScroll(val);
        }
        #endregion

        #region is
        public override bool IsMoving => RTSCamera.IsMoving;
        #endregion
    }
}