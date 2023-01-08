//------------------------------------------------------------------------------
// PluginSysConsole.cs
// Copyright 2022 2022/12/26 
// Created by CYM on 2022/12/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using Sirenix.OdinInspector;

namespace CYM
{
    public partial class SysConsole : BaseCoreMono
    {
        [FoldoutGroup("Person")]
        public bool IsFastPersonDeath = false;
    }
}