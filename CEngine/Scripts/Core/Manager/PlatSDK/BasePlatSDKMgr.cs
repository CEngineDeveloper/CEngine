using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CYM
{
    public class BasePlatSDKMgr : BaseGFlowMgr
    {
        #region prop
        protected uint fileAppId;
        protected Dictionary<string, LanguageType> LanguageDic { get; set; } = new Dictionary<string, LanguageType>();
        #endregion

        #region 生命周期
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            AddLanguageConvert("schinese", LanguageType.Chinese);
            AddLanguageConvert("tchinese", LanguageType.Traditional);
            AddLanguageConvert("english", LanguageType.English);
        }
        public override void OnBeAdded(IMono mono)
        {
            fileAppId = ReadFileAppId();
            base.OnBeAdded(mono);
        }
        public override void OnAffterStart()
        {
            base.OnAffterStart();

        }
        protected override void OnAllLoadEnd2()
        {
            base.OnAllLoadEnd2();
            if (!IsLegimit)
            {
                var errorMsg = GetErrorInfo();
                WinUtil.MessageBox(errorMsg, "OK");
            }
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否已经初始化
        /// </summary>
        /// <returns></returns>
        public bool IsSDKInited
        {
            get; protected set;
        }
        /// <summary>
        /// 是否支持这类语言
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected virtual bool IsSupportLanguage(string lang)
        {
            return false;
        }
        /// <summary>
        /// 是否为正版
        /// </summary>
        public virtual bool IsLegimit
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 文件APPid不一致
        /// </summary>
        public virtual bool IsDifferentAppId
        {
            get
            {
                if (Application.isEditor)
                    return false;
                return GetAppId() != fileAppId;
            }
        }
        /// <summary>
        /// 是否支持云存档
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuportCloudArchive()
        {
            return IsLegimit;
        }
        /// <summary>
        /// 通过id判断此人是否为自己
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool IsSelf(ulong id)
        {
            return false;
        }
        /// <summary>
        /// 是否支持平台语言
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuportPlatformLanguage()
        {
            return false;
        }
        /// <summary>
        /// 是否支持平台界面
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuportPlatformUI()
        {
            return false;
        }
        /// <summary>
        /// 是否支持menu prop UI
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuportMenuPropUI()
        {
            return true;
        }
        #endregion

        #region Set
        /// <summary>
        /// 添加langue转换
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        protected void AddLanguageConvert(string key, LanguageType type)
        {
            if (LanguageDic.ContainsKey(key))
            {
                LanguageDic[key] = type;
            }
            else
            {
                LanguageDic.Add(key, type);
            }
        }
        //支付接口
        public virtual void Purchase()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Get
        public virtual string GetName()
        {
            return "None";
        }
        public virtual ulong GetUserID()
        {
            return 0;
        }
        /// <summary>
        /// 获得标题描述
        /// </summary>
        /// <returns></returns>
        public virtual string GetMainMenuTitle()
        {
            return "";
        }
        /// <summary>
        /// 本地APP文件路径
        /// </summary>
        /// <returns></returns>
        protected virtual string LocalAppIDFilePath()
        {
            return string.Empty;
        }
        /// <summary>
        /// 得到APPid
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetAppId()
        {
            return 0;
        }
        /// <summary>
        /// 读取文件中的APPid
        /// </summary>
        /// <returns></returns>
        private uint ReadFileAppId()
        {
            if (Application.isEditor)
                return 0;
            if (LocalAppIDFilePath().IsInv())
                return 0;
            uint r = 0;
            string id = null;
            var file = LocalAppIDFilePath();
            if (!File.Exists(file))
                return 0;
            try
            {
                id = File.ReadAllText(file);
            }
            catch (Exception e)
            {
                CLog.Error("无法打开appid文件, {0}", e);
                return 0;
            }

            if (!uint.TryParse(id, out r))
            {
                CLog.Error("无法读取AppID {0}", id);
                return 0;
            }
            else
            {
                return r;
            }
        }
        /// <summary>
        /// 得到错误信息
        /// </summary>
        /// <returns></returns>
        public virtual string GetErrorInfo()
        {
            return "Error";
        }
        /// <summary>
        /// 得到云存档路径
        /// </summary>
        /// <returns></returns>
        public virtual string GetCloudArchivePath()
        {
            return UnityEngine.Application.persistentDataPath;
        }
        /// <summary>
        /// 获得语言
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public virtual LanguageType GetLanguageType()
        {
            string str = GetCurLanguageStr();
            if (LanguageDic.ContainsKey(str))
                return LanguageDic[str];
            return LanguageType.English;
        }
        public virtual string GetCurLanguageStr()
        {
            return "";
        }
        #endregion

        #region Callback
        protected virtual void OnInitSetting()
        {

        }
        #endregion

        #region overlay
        /// <summary>
        /// 打开成就overlay
        /// </summary>
        public virtual void OpenAchievement(ulong id)
        {

        }
        /// <summary>
        /// 打开聊天
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenChat(ulong id)
        {
        }
        /// <summary>
        /// 打开简介
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenProfile(ulong id)
        {
        }
        /// <summary>
        /// 打开统计
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenStats(ulong id)
        {
        }
        /// <summary>
        /// 打开贸易
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenTrade(ulong id)
        {
        }
        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenAddFriend(ulong id)
        {

        }
        /// <summary>
        /// 打开URL
        /// </summary>
        public virtual void OpenURL(string URL)
        {
            UnityEngine.Application.OpenURL(URL);
        }
        #endregion

        #region shop
        public virtual void GoToShop()
        {

        }
        #endregion

        #region test
        protected virtual void Test()
        {

        }
        #endregion

        public virtual void PostBuilder()
        {

        }

    }

}