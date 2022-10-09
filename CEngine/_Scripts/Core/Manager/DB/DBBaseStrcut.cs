//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    #region settings
    [Serializable]
    public partial class DBBaseSettings
    {
        /// <summary>
        /// 语言类型
        /// </summary>
        public LanguageType LanguageType = LanguageType.Chinese;
        /// <summary>
        /// 禁止背景音乐
        /// </summary>
        public bool MuteMusic = false;
        /// <summary>
        /// 禁止音效
        /// </summary>
        public bool MuteSFX = false;
        /// <summary>
        /// 静止所有音乐
        /// </summary>
        public bool Mute = false;
        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float VolumeMusic = 0.2f;
        /// <summary>
        /// 音效音量
        /// </summary>
        public float VolumeSFX = 1.0f;
        /// <summary>
        /// 主音量
        /// </summary>
        public float Volume = 1.0f;
        /// <summary>
        /// 语音音量
        /// </summary>
        public float VolumeVoice = 0.5f;
        /// <summary>
        /// 自动存储类型
        /// </summary>
        public bool IsAutoSave = true;
        /// <summary>
        /// 是否开启后期特效
        /// </summary>
        public bool EnablePostProcess = true;
        /// <summary>
        /// 游戏画质
        /// </summary>
        public GamePropType Quality = GamePropType.Hight;
        /// <summary>
        /// 游戏分辨率,通常选择小一号的窗口模式
        /// </summary>
        public int Resolution = 1;
        /// <summary>
        /// 全屏
        /// </summary>
        public WindowType WindowType = WindowType.Windowed;
    }
    [Serializable]
    public partial class DBBaseGame
    {
        public string PlayerID = SysConst.STR_Inv;
        public string BattleID = SysConst.STR_Inv;
        public int PlayTime = 0;
        public int LoadBattleCount = 0;
        public int TurnCount = 0;
        public DateTime CurDateTime = new DateTime(1, 1, 1);
        public GameDateTimeType CurDateTimeAgeType = GameDateTimeType.BC;
        public GameNetMode GameNetMode = GameNetMode.PVE;
        public GamePlayStateType GamePlayStateType = GamePlayStateType.NewGame;
        public bool IsNewGame() => GamePlayStateType == GamePlayStateType.NewGame;
        public bool IsLoadGame() => GamePlayStateType == GamePlayStateType.LoadGame;
        public bool IsFirstLoadBattle() => LoadBattleCount == 1;
    }
    #endregion

    #region base
    [Serializable]
    public class DBBase
    {
        public long ID = SysConst.LONG_Inv;
        public string TDID = SysConst.STR_Inv;
        public string CustomName = SysConst.STR_Inv;

        public bool IsInv()
        {
            return TDID.IsInv() && ID.IsInv();
        }
    }
    [Serializable]
    public class DBBaseUnit : DBBase
    {
        public Vec3 Position = new Vec3(new Vector3(99999.0f, 99999.0f, 99999.0f));
        public Qua Rotation = new Qua(Quaternion.identity);
        public bool IsNewAdd = false;
    }
    [Serializable]
    public class DBBaseBuff : DBBase
    {
        public float CD = 0;
        public float Input;
        public bool Valid = true;
    }
    #endregion
}