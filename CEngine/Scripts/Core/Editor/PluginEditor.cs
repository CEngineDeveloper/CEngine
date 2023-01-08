//------------------------------------------------------------------------------
// EditorPlugin.cs
// Copyright 2022 2022/11/6 
// Created by CYM on 2022/11/6
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using System;

namespace CYM
{
    public class PluginEditor
    {
        public Action OnGUI { get; set; }
        public PluginEditor()
        {
            //BuildWindow.Plugins.Add(this);
        }
    }
}