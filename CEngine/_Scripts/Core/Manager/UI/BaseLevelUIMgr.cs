//------------------------------------------------------------------------------
// BaseLevelUIMgr.cs
// Copyright 2021 2021/9/5 
// Created by CYM on 2021/9/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.UI;
namespace CYM
{
    public class BaseLevelUIMgr : BaseUIMgr 
    {
        protected override string ViewName => "LevelUI";

        #region Callback
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            DoCreateView();
        }
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            Show(false);
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            DoDestroyView();
        }
        #endregion
    }
}