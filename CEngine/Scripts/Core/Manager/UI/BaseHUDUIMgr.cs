//------------------------------------------------------------------------------
// BaseHUDUIMgr.cs
// Copyright 2021 2021/3/27 
// Created by CYM on 2021/3/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM.UI;
namespace CYM
{
    public class BaseHUDUIMgr : BaseUIMgr
    {
        public static UHUDView CommonHUDView { get; protected set; }
      

        #region life
        protected override string ViewName => "HUDUI";
        protected override string RootViewPrefab => "URootView";
        protected override void OnCreateUIView1()
        {
            base.OnCreateUIView1();
            CommonHUDView = CreateView<UHUDView>("UHUDView", "Common");
        }
        protected UHUDView CreateHUDView(string customName)
        {
            return CreateView<UHUDView>("UHUDView", customName);
        }
        #endregion

        #region Callback
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
            DoCreateView();
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            DoDestroyView();
        }
        #endregion

        #region set
        //创建一个全局的HUDitem
        public static THUD Spawn<THUD>(string prefabName,BaseUnit followUnit=null) where THUD : UHUDBar
        {
            if (prefabName.IsInv()) return null;
            GameObject tempGO = BaseGlobal.RsUI.Get(prefabName);
            if (tempGO != null)
            {
                var temp = CommonHUDView.Jump(tempGO, followUnit);
                if (temp == null)
                {
                    CLog.Error("错误！Spawn HUD,没有添加HUDitem组件：" + prefabName);
                    return null;
                }
                temp.SetFollowObj(followUnit.GetNode(temp.NodeType));
                return (temp as THUD);
            }
            else
            {
                CLog.Error("错误！Spawn：" + prefabName);
            }
            return null;
        }
        #endregion
    }
}