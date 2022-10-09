//------------------------------------------------------------------------------
// BaseFOWRevealerMgr.cs
// Copyright 2018 2018/11/12 
// Created by CYM on 2018/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using FoW;

namespace CYM.FOW
{
    public class BaseFOWRevealerMgr : BaseMgr
    {
        #region prop
        protected FogOfWarUnit FOWRevealer { get; set; }
        protected BaseCameraMgr CameraMgr => FOWMgr.CameraMgr;
        public bool IsPreVisible { get; protected set; } = true;
        public bool IsVisible { get; protected set; } = true; //是否可见
        public bool IsInFog { get; private set; } //是否处于迷雾中
        public bool IsInView { get; private set; } = true; //是否为玩家的视野
        #endregion

        #region life
        protected virtual float Radius => throw new System.NotImplementedException();
        protected virtual BaseFOWMgr FOWMgr => BaseGlobal.FOWMgr;
        public sealed override MgrType MgrType => MgrType.Unit;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            FOWMgr?.FOWRevealerList.Add(this);
            SelfBaseUnit.Callback_OnSetOwner += OnSetOwner;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            FOWMgr?.FOWRevealerList.Remove(this);
            SelfBaseUnit.Callback_OnSetOwner += OnSetOwner;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            FOWRevealer = SelfMono.SetupMonoBehaviour<FogOfWarUnit>();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (FOWRevealer != null)
            {
                if (CameraMgr == null || !CameraMgr.IsEnable)
                {
                    FOWRevealer.enabled = false;
                    return;
                }
                else
                {
                    FOWRevealer.enabled = true;
                }
                FOWRevealer.team = IsInView ? 0 : 1;
                FOWRevealer.circleRadius = IsInView ? Radius : 0;
                IsInFog = IsInFogRange();
                IsVisible = !IsInFog;

                if (IsPreVisible != IsVisible)
                {
                    OnVisibleChange(IsVisible);
                    IsPreVisible = IsVisible;
                }
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            IsInView = IsEnableView();
        }
        protected virtual bool IsEnableView()
        {
            return true;
        }
        #endregion

        #region is
        bool IsInFogRange()
        {
            if (FOWMgr == null)
                return false;
            if (FOWMgr.IsInFog(SelfBaseUnit.Pos, 70))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Callback
        protected virtual void OnVisibleChange(bool isVisible)
        {

        }
        protected virtual void OnSetOwner(BaseUnit unit)
        {
            SetDirty();
        }
        #endregion
    }
}