//------------------------------------------------------------------------------
// Distribution.cs
// Copyright 2022 2022/9/25 
// Created by CYM on 2022/9/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.Steam;
using System;

namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        public static Type Steam { get; private set; } = typeof(BaseSteamSDKMgr);
    }
}