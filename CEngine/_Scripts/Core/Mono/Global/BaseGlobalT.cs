//------------------------------------------------------------------------------
// BaseGlobalT.cs
// Copyright 2020 2020/7/17 
// Created by CYM on 2020/7/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;

namespace CYM
{
    public class BaseGlobalT<TPlayer, TCofnig,TSetting, TGlobal> : BaseGlobal
        where TPlayer : BaseUnit
        where TCofnig : SysConfig, new()
        where TSetting: DBBaseSettings, new()
        where TGlobal : BaseGlobal
    {
        public new static TGlobal Ins { get; protected set; }
        public new static TPlayer Player => ScreenMgr?.Player as TPlayer;
        public static TDBaseConfig<TCofnig> TDConfig = new TDBaseConfig<TCofnig>();
        public static TSetting Setting => SettingsMgr.Settings as TSetting;

        public override void Awake()
        {
            Ins = this as TGlobal;
            base.Awake();
        }

    }
}