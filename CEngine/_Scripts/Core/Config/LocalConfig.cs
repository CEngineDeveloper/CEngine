//------------------------------------------------------------------------------
// BuildLocalConfig.cs
// Copyright 2018 2018/5/3 
// Created by CYM on 2018/5/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;

namespace CYM
{
    public sealed class LocalConfig : ScriptableObjectConfig<LocalConfig>
    {
        public override bool IsHideInBuildWindow => true;
        #region editor setting
        public bool FoldInfo = false;
        public bool FoldVersion = false;
        public bool FoldSetting = false;
        public bool FoldSettingSteam = false;
        public bool FoldSettingBuild = false;
        public bool FoldDLC = false;
        public bool FoldBuild = false;
        public bool FoldHotFix = false;
        public bool FoldHotFixGenerate = false;
        public bool FoldExplorer = false;
        public bool FoldSceneList = false;
        public bool FoldOther = false;
        public bool FoldSubWindow = false;
        public bool FoldConfigWindow = false;

        public string ABDownloadPath = "";
        #endregion
    }
}