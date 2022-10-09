//------------------------------------------------------------------------------
// BaseGlobal.cs
// Copyright 2022 2022/9/25 
// Created by CYM on 2022/9/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.Line;
namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        public static BaseGLineMgr PathRenderMgr { get; protected set; }
    }
}