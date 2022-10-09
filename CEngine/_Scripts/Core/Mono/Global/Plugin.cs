//------------------------------------------------------------------------------
// Install.cs
// Copyright 2022 2022/9/25 
// Created by CYM on 2022/9/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
namespace CYM
{
    public class Plugin
    {
        public Callback<BaseGlobal> OnInstall { get; set; }
        public Plugin()
        {
            BaseGlobal.Plugins.Add(this);
        }
    }
}