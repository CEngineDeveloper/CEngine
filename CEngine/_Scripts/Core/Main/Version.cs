//**********************************************
// Class Name	: GlobalComponet
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public sealed class Version
    {
        public static string BuildTime => Config.LastBuildTime;
        public static string GameVersion => Config.ToString();
        public static string FullGameVersion => Config.FullVersion;
        public static BuildConfig Config => BuildConfig.Ins;
        public static bool IsTrial => Config.IsTrial;
        public static bool IsDevelop => Config.IsDevelop;
        public static bool IsPublic => Config.IsPublic;

        #region isCan
        /// <summary>
        /// 数据库版本是否兼容
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsInData(int data)=> Config.Data == data;
        #endregion
    }
}