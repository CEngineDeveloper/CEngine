using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_STANDALONE_WIN
using Steamworks;
using Steamworks.Data;
#endif

namespace CYM.Steam
{
    public class BaseSteamSDKMgr : BasePlatSDKMgr
    {
#if UNITY_STANDALONE_WIN  

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            uint appId = GetAppId();
            // 使用try防止崩溃
            try
            {
                SteamClient.Init(appId);
            }
            catch (Exception e)
            {
                CLog.Info("Error starting steam client: {0}", e);
                SteamClient.Shutdown();
            }
            if (SteamClient.IsValid)
            {
                IsSDKInited = true;
            }
            else
            {
                SteamClient.Shutdown();
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();

        }

        public override void OnStart()
        {
            base.OnStart();
        }
        public override void OnDisable()
        {
            SteamClient.Shutdown();
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            SteamClient.RunCallbacks();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }
        public override void OnDestroy()
        {
            SteamClient.Shutdown();
            base.OnDestroy();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否支持这类语言
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected override bool IsSupportLanguage(string lang)
        {
            if (SteamApps.AvailableLanguages == null)
                return false;
            if (SteamApps.AvailableLanguages.Length == 0)
                return false;
            return SteamApps.AvailableLanguages[0].Contains(lang);
        }
        /// <summary>
        /// 是否为正版
        /// </summary>
        public override bool IsLegimit
        {
            get
            {
                if (BuildConfig.Ins.IsDevelop)return true;
                if (!SteamClient.IsValid)return false;
                if (IsDifferentAppId) return false;
                return true;
            }
        }
        /// <summary>
        /// 是否支持云存档
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportCloudArchive()
        {
            return IsLegimit;
        }
        /// <summary>
        /// 通过id判断此人是否为自己
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool IsSelf(ulong id)
        {
            return SteamClient.SteamId == id;
        }
        /// <summary>
        /// 是否支持平台UI
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportPlatformUI()
        {
            return IsLegimit;
        }
        /// <summary>
        /// 是否支持平台语言
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportPlatformLanguage()
        {
            return true;
        }
        #endregion

        #region Get
        public override string GetName()
        {
            if (!SteamClient.IsValid)
                return "";
            return SteamClient.Name;
        }
        public override ulong GetUserID()
        {
            if (!SteamClient.IsValid)
                return 0;
            return SteamClient.SteamId;
        }
        protected override uint GetAppId()
        {
            if (!SteamClient.IsValid)
                return 0;
            return GameConfig.Ins.SteamAppID;
        }
        /// <summary>
        /// 本地APP文件路径
        /// </summary>
        /// <returns></returns>
        protected override string LocalAppIDFilePath()
        {
            return "steam_appid.txt";
        }
        /// <summary>
        /// 得到错误信息
        /// </summary>
        /// <returns></returns>
        public override string GetErrorInfo()
        {
            if (IsDifferentAppId) return "The game is not activated, app found the different app id,do you changed any thing?";
            if (!IsLegimit) return "Unable to connect to Steam\n" + Util.GetStr("Msg_需要连接Steam");
            if (!SteamClient.IsValid) return "The game is not activated";
            return "Error";
        }
        /// <summary>
        /// 得到云存档路径
        /// </summary>
        /// <returns></returns>
        public override string GetCloudArchivePath()=> Application.persistentDataPath;
        public override string GetCurLanguageStr()
        {
            if(IsSDKInited)
                return SteamApps.GameLanguage;
            return "";
        }
        #endregion

        #region Set
        public override void Purchase()
        {
            string orderID = IDUtil.GenOrderNumber();
            string key = GameConfig.Ins.SteamWebAPI;
            //void SendUserInfo()
            //{
            //    CLog.Info("开始发送");
            //    string str = "key="+ key + "&steamid=" + GetUserID().ToString();
            //    var result = HTTPUtil.SendGetHttp("https://partner.steam-api.com/ISteamMicroTxn/GetUserInfo/v2/?" + str,null);
            //    CLog.Info(result.ToString());
            //}

            //void GetUserInfo()
            //{
            //    CLog.Info("开始发送");
            //    string str = "key=" + key + "&steamid=" + GetUserID().ToString();
            //    var result = HTTPUtil.SendGetHttp("https://partner.steam-api.com/ISteamMicroTxn/GetUserInfo/v2/" + str,null);
            //    CLog.Info(result.ToString());
            //}

            //void SendInitTxn()
            //{
            //    Dictionary<string, string> dic = new Dictionary<string, string>();
            //    dic.Add("key", key);
            //    dic.Add("orderid", orderID.ToString());
            //    dic.Add("steamid", GetUserID().ToString());
            //    dic.Add("appid", GetAppId().ToString());
            //    dic.Add("itemcount", "1");
            //    dic.Add("language", GetCurLanguageStr());
            //    dic.Add("currency", "CNY");
            //    dic.Add("itemid[0]", IDUtil.GenString());
            //    dic.Add("qty[0]", "1");
            //    dic.Add("amount[0]", "100");
            //    dic.Add("description[0]", "此物品非常凶残");
            //    var result = HTTPUtil.SendPostHttp("https://partner.steam-api.com/ISteamMicroTxnSandbox/InitTxn/v3/", dic);
            //    CLog.Info(result.ToString());

            //}

            //void SendFinalizeTxn()
            //{
            //    Dictionary<string, string> dic = new Dictionary<string, string>();
            //    dic.Add("key", key);
            //    dic.Add("orderid", orderID);
            //    dic.Add("appid", GetAppId().ToString());
            //    var result = HTTPUtil.SendPostHttp("https://partner.steam-api.com/ISteamMicroTxnSandbox/FinalizeTxn/v2/", dic);
            //    CLog.Info(result.ToString());
            //}
        }
        #endregion

        #region Callback
        #endregion

        #region overlay
        public override void OpenAchievement(ulong id)
        {
            //SteamFriends.OpenOverlay(,id);
        }
        public override void OpenChat(ulong id)
        {
            //Client?.Overlay.OpenChat(id);
        }
        public override void OpenProfile(ulong id)
        {
            //Client?.Overlay.OpenProfile(id);
        }
        public override void OpenStats(ulong id)
        {
            //Client?.Overlay.OpenStats(id);
        }
        public override void OpenTrade(ulong id)
        {
            //Client?.Overlay.OpenTrade(id);
        }
        public override void OpenAddFriend(ulong id)
        {
            //Client?.Overlay.AddFriend(id);
        }
        public override void OpenURL(string URL)
        {
            if (Application.isEditor)
            {
                base.OpenURL(URL);
            }
            else
            {
                SteamFriends.OpenWebOverlay(URL,true);
            }
        }
        #endregion

        #region shop
        public override void GoToShop()
        {
        }
        #endregion

        #region test
        protected override void Test()
        {
        }
        #endregion

        public override void PostBuilder()
        {
            base.PostBuilder();
            var dir = BuildConfig.Ins.DirPath;
            if (!FileUtil.ExistsDir(dir))
                Directory.CreateDirectory(dir);
            var path = Path.Combine(BuildConfig.Ins.DirPath, SysConst.File_SteamAppID);
            if (!FileUtil.ExistsFile(path))
            {
                using (File.Create(path))
                { 
                
                }
            }
            FileUtil.WriteFile(path, GameConfig.Ins.SteamAppID.ToString());
        }
#endif

    }

}