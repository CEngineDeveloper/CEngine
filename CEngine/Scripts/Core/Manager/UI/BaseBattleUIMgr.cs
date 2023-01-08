//------------------------------------------------------------------------------
// BaseBattleUIMgr.cs
// Copyright 2021 2021/9/5 
// Created by CYM on 2021/9/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using UnityEngine;

namespace CYM
{
    public class BaseBattleUIMgr : BaseUIMgr
    {
        #region life
        protected override string ViewName => "BattleUI";
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UBattleMenuView.Default?.Show();
            }
        }
        #endregion

        #region Callback
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            DoCreateView();
        }
        protected override void OnBattleReadDataEnd()
        {
            base.OnBattleReadDataEnd();

        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            DoDestroyView();
        }
        #endregion
    }
}