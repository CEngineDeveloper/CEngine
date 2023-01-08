//------------------------------------------------------------------------------
// PluginConsole.cs
// Copyright 2022 2022/10/3 
// Created by CYM on 2022/10/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using Sirenix.OdinInspector;

namespace CYM
{
    public partial class SysConsole : BaseCoreMono
    {
        [FoldoutGroup("Unit")]
        public bool IsAllAlert = false;
        [FoldoutGroup("Unit")]
        public bool IsNoEvent = false;
        [FoldoutGroup("Unit")]
        public bool IsMustEvent = false;
    }
}