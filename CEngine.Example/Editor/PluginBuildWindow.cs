//------------------------------------------------------------------------------
// PluginBuildWindow.cs
// Copyright 2022 2022/11/6 
// Created by CYM on 2022/11/6
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using UnityEditor;

namespace CYM
{
    public partial class BuildWindow : EditorWindow
    {
        static PluginEditor PluginExample = new PluginEditor
        {
            OnGUI = () => {

            }
        };
    }
}