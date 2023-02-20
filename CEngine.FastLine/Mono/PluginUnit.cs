//------------------------------------------------------------------------------
// BaseUnit.cs
// Copyright 2022 2022/9/25 
// Created by CYM on 2022/9/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.Line;
namespace CYM
{
    public partial class BaseUnit : BaseCoreMono
    {
        static PluginUnit PluginFastline = new PluginUnit
        {
            OnPostAddComponet = (u, x) => {
                if (x is BaseULineMgr)
                {
                    u.LineRenderMgr = x as BaseULineMgr;
                }
            }
        };
        public BaseULineMgr LineRenderMgr { get; protected set; }
    }
}