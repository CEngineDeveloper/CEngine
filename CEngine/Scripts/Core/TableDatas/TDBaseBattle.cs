
using System;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    [Serializable]
    public class TDBaseBattleData : TDBaseData
    {
        #region mgr
        DBBaseSettings SettingData => BaseGlobal.SettingsMgr.Settings;
        #endregion

        public string Plot { get; set; } = SysConst.STR_Inv;
        public string SceneName { get; set; } = SysConst.STR_Inv;
        public bool UseAssetBundle { get; set; } = true;//Unity有bug，有些场景使用AssetBundle加载会显示不正确,必须使用内置加载
        public virtual string GetSceneName()
        {
            var ret = SceneName;
            if (ret.IsInv())
                ret = TDID.Replace(SysConst.Prefix_Battle, "");
            return ret;
        }
        public string GetRawSceneName()
        {
            var ret = SceneName;
            if (ret.IsInv())
                ret = TDID.Replace(SysConst.Prefix_Battle, "");
            return ret;
        }

    }
}
