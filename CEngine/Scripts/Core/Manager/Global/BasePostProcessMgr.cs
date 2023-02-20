//------------------------------------------------------------------------------
// BasePostProcessMgr.cs
// Copyright 2023 2023/1/9 
// Created by CYM on 2023/1/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace CYM
{
    public class BasePostProcessMgr : BaseGFlowMgr
    {
        #region prop
        protected Camera Camera { get; private set; }
        protected PostProcessVolume Volume { get; private set; }
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();

        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            if (CameraObj.Obj)
            {
                Camera = CameraObj.Obj;
            }
            Volume = Camera.GetComponentInChildren<PostProcessVolume>();

        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (BattleMgr == null || !BattleMgr.IsLoadBattleEnd)
                return;
            OnPostProcessUpdate();
        }
        protected virtual void OnPostProcessUpdate()
        {

        }
        #endregion

        #region get
        public T GetSetting<T>() where T : PostProcessEffectSettings
        {
            T ret = default;
            if (Volume && Volume.profile)
                Volume.profile.TryGetSettings(out ret);
            return ret;
        }
        #endregion

        #region set
        public virtual void EnablePostProcess(bool b)
        {
            BaseGlobal.Settings.EnablePostProcess = b;
            if (Volume)
                Volume.enabled = b;
        }
        #endregion
    }
}