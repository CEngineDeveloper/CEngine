//------------------------------------------------------------------------------
// BaseSLGCamera.cs
// Copyright 2021 2021/6/18 
// Created by CYM on 2021/6/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.UI;
namespace CYM.Cam
{
    public abstract class BaseSLGCameraMgr : BaseCameraMgr
    {
        bool isScrollControl = true;
        bool isDragControl = true;
        bool isScreenEdgeControl = true;
        bool isRotateControl = true;
        bool isControl = true;

        #region param
        public virtual float ScrollSpeed => 1.0f;
        public virtual float DragSpeed => 1.0f;
        public virtual float ScreenEdgeSpeed => 1.0f;
        public virtual float RotateSpeed => 1.0f;
        public virtual bool IsScrollControl =>
                BaseInputMgr.IsEnablePlayerInput &&
                !BaseInputMgr.IsStayInUIWithoutHUD &&
                isScrollControl;
        public virtual bool IsDragControl =>
                BaseInputMgr.IsEnablePlayerInput &&
                !BaseInputMgr.IsStayInUIWithoutHUD &&
                isDragControl;
        public virtual bool IsScreenEdgeControl => isScreenEdgeControl;
        public virtual bool IsRotateControl => isRotateControl;
        public virtual bool IsControl =>
                 !SysConsole.Ins.IsLockCamera &&
                 !BaseInputMgr.IsFullScreen &&
                 !ULoadingView.IsInLoading &&
                 BaseInputMgr.IsEnablePlayerInput &&
                 isControl &&
                 IsEnable;
        #endregion

        #region set
        public void SetScrollControl(bool b)
        {
            isScrollControl = b;
        }
        public void SetDragControl(bool b)
        {
            isDragControl = b;
        }
        public void SetScreenEdgeControl(bool b)
        {
            isScreenEdgeControl = b;
        }
        public void Lock(bool b)
        {
            isControl = !b;
        }
        public virtual void SetScroll(float val) => throw new System.NotImplementedException();
        #endregion

        #region Callback
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            isScrollControl = true;
            isDragControl = true;
            isScreenEdgeControl = true;
            isControl = true;
        }
        #endregion
    }
}