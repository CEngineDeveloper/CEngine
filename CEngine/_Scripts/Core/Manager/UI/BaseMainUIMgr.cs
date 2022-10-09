//------------------------------------------------------------------------------
// BaseMainUIMgr.cs
// Copyright 2021 2021/9/5 
// Created by CYM on 2021/9/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.UI;
namespace CYM
{
    public class BaseMainUIMgr : BaseUIMgr
    {
        #region life
        protected override string ViewName => "MainUI";
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            Fade.Show(1.0f);
        }
        protected override void OnCreateUIView1()
        {
            base.OnCreateUIView1();
            UseUIParticleSystem();
        }
        protected override void OnCreateUIView2()
        {
            base.OnCreateUIView2();
            ChangeToCameraSpace();
        }
        #endregion

        #region Callback
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
            if (BaseGlobal.BattleMgr != null && 
                BaseGlobal.BattleMgr.IsInReloading)
                return;
            UICameraObj.GO.SetActive(false);
            DoDestroyView();
        }
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            if (BaseGlobal.BattleMgr != null &&
                BaseGlobal.BattleMgr.IsInReloading)
                return;
            UICameraObj.GO.SetActive(true);
            DoCreateView();
        }
        protected override void OnAllLoadEnd1()
        {
            base.OnAllLoadEnd1();
            DoCreateView();
        }
        #endregion
    }
}