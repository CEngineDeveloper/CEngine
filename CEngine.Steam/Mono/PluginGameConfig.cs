//------------------------------------------------------------------------------
// SteamConfig.cs
// Copyright 2022 2022/9/25 
// Created by CYM on 2022/9/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using Sirenix.OdinInspector;

namespace CYM
{
    public partial class GameConfig : CustomScriptableObjectConfig<GameConfig>
    {
        #region Steam
        [FoldoutGroup("Steam")]
        public uint SteamAppID;
        [FoldoutGroup("Steam")]
        public string SteamWebAPI;
        #endregion
    }
}