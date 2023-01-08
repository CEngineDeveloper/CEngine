//------------------------------------------------------------------------------
// PluginDBStrcut.cs
// Copyright 2022 2022/11/8 
// Created by CYM on 2022/11/8
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using System;

namespace CYM.Plot
{
    [Serializable]
    public class DBBaseNarration : DBBase
    {
        public HashList<string> Showed = new HashList<string>();
    }
}