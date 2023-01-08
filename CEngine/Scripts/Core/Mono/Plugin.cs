//------------------------------------------------------------------------------
// Install.cs
// Copyright 2022 2022/9/25 
// Created by CYM on 2022/9/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CYM
{
    public class PluginGlobal
    {
        public static List<PluginGlobal> All { get; private set; } = new List<PluginGlobal>();

        public Callback<BaseGlobal> OnInstall { get; set; }
        public Callback<BaseMgr> OnPostAddComponet { get; set; }
        public PluginGlobal()
        {
            All.Add(this);
        }
    }

    public class PluginUnit
    {
        public static List<PluginUnit> All { get; private set; } = new List<PluginUnit>();
        public PluginUnit()
        {
            All.Add(this);
        }
        public Callback<BaseUnit> OnInstall { get; set; }
        public Callback<BaseUnit,BaseMgr> OnPostAddComponet { get; set; }
    }
}