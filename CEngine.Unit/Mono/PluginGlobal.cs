//------------------------------------------------------------------------------
// PluginGlobal.cs
// Copyright 2022 2022/9/26 
// Created by CYM on 2022/9/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.Unit;

namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        static PluginGlobal PluginUnit = new PluginGlobal
        {
            OnInstall = (g) =>
            {
                PerformMgr = g.AddComponent<BasePerformMgr>();
            }
        };

        public static IAttrMgr AttrMgr { get; protected set; }
        public static IBuffMgr BuffMgr { get; protected set; }
        public static BasePerformMgr PerformMgr { get; protected set; }
    }
}