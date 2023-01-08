//**********************************************
// Class Name	: CYMBaseSettingsManager
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CYM
{
    public class BaseSettingsMgr<TDBSetting> : BaseGFlowMgr, ISettingsMgr<TDBSetting>
        where TDBSetting : DBBaseSettings, new()
    {
        #region prop
        public TDBSetting Settings { get; protected set; } = new TDBSetting();
        public bool IsFirstCreateSettings { get; set; } = false;
        #endregion

        #region Callback Val
        /// <summary>
        /// 还原设置
        /// </summary>
        public event Callback<TDBSetting> Callback_OnRevert;
        /// <summary>
        /// 设置初始化
        /// </summary>
        public event Callback Callback_OnInitSettings;
        /// <summary>
        /// 第一次创建设置文件回调
        /// </summary>
        public event Callback<TDBSetting> Callback_OnFirstCreateSetting;
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            InitAllResolutions();
            Callback_OnRevert += OnRevert;
            Callback_OnInitSettings += OnInitSetting;
            Callback_OnFirstCreateSetting += OnFirstCreateSetting;
        }
        public override void OnAffterStart()
        {
            base.OnAffterStart();
        }
        protected override void OnAllLoadEnd2()
        {
            base.OnAllLoadEnd2();
            RefreshScreenSettings();
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            string fullpath = SysConst.Path_Settings;
            TDBSetting settings = default;
            if (File.Exists(fullpath))
            {
                using (Stream stream = File.OpenRead(fullpath))
                {
                    if (stream != null)
                    {
                        try
                        {
                            settings = FileUtil.LoadJson<TDBSetting>(stream);
                        }
                        catch (Exception e)
                        {
                            settings = default;
                            CLog.Error("载入settings出错{0}", e);
                        }
                    }

                }
            }
            if (settings == null)
            {
                IsFirstCreateSettings = true;
                settings = new TDBSetting();
                Save();
            }
            SetSettings(settings);
        }
        public override void OnStart()
        {
            base.OnStart();
            if (IsFirstCreateSettings)
                Callback_OnFirstCreateSetting?.Invoke(Settings);
            Callback_OnInitSettings?.Invoke();
        }
        /// <summary>
        /// mono的OnDisable
        /// </summary>
        public override void OnDisable()
        {
            Callback_OnRevert -= OnRevert;
            Callback_OnInitSettings -= OnInitSetting;
            Callback_OnFirstCreateSetting -= OnFirstCreateSetting;
            base.OnDisable();
        }
        #endregion

        #region set
        /// <summary>
        /// 还原设置
        /// </summary>
        public virtual void Revert()
        {
            Settings = new TDBSetting();
            Callback_OnRevert?.Invoke(Settings);
        }
        /// <summary>
        /// 设置设置
        /// </summary>
        /// <param name="data"></param>
        public void SetSettings(TDBSetting data)
        {
            Settings = data;
        }
        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            using (Stream stream = new FileStream(SysConst.Path_Settings, FileMode.Create))
            {
                FileUtil.SaveJson(stream, Settings);
                stream.Close();
            }
        }
        /// <summary>
        /// 设置分辨率
        /// </summary>
        public virtual void SetResolution(int index)
        {
            Settings.Resolution = index;
            RefreshScreenSettings();
        }
        /// <summary>
        /// 设置全屏
        /// </summary>
        public virtual void SetWindowType(WindowType type)
        {
            Settings.WindowType = type;
            if (type == WindowType.Fullscreen)
                Settings.Resolution = 0;
            RefreshScreenSettings();
        }
        /// <summary>
        /// 设置画质
        /// </summary>
        public virtual void SetQuality(int index)
        {
            QualitySettings.SetQualityLevel(index);
            Settings.Quality = (GamePropType)index;
        }
        public void SetTerrainAccuracy(bool b)
        {
            if (TerrainObj.Ins != null)
            {
                TerrainObj.Obj.heightmapPixelError = b ? 1 : 3;
            }
        }
        /// <summary>
        /// 刷新屏幕设置
        /// </summary>
        public void RefreshScreenSettings()
        {
            if (!BaseGlobal.LoaderMgr.IsLoadEnd) return;
            var index = Settings.Resolution;
            if (Resolutions.Count <= index) return;

            if (Settings.WindowType == WindowType.ExclusiveFullScreen)
            {
                Screen.SetResolution(Resolutions[index].width, Resolutions[index].height, FullScreenMode.ExclusiveFullScreen);
            }
            else if (Settings.WindowType == WindowType.Fullscreen)
            {
                Screen.SetResolution(Resolutions[index].width, Resolutions[index].height, FullScreenMode.FullScreenWindow);
            }
            else if (Settings.WindowType == WindowType.MaximizedWindow)
            {
                Screen.SetResolution(Resolutions[index].width, Resolutions[index].height, FullScreenMode.MaximizedWindow);
            }
            else if (Settings.WindowType == WindowType.Windowed)
            {
                Screen.SetResolution(Resolutions[index].width, Resolutions[index].height, FullScreenMode.Windowed);
            }
        }
        protected virtual void InitAllResolutions()
        {
            ResolutionsKey.Clear();
            Resolutions.Clear();
            foreach (var item in Screen.resolutions)
            {
                string customKey = string.Format($"{item.width}x{item.height}");
                if (!ResolutionsKey.Contains(customKey))
                {
                    ResolutionsKey.Add(customKey);
                    Resolutions.Add(item);
                }
            }

            Resolutions.Sort((x, y) =>
            {

                if (x.width > y.width)
                    return -1;
                else
                    return 1;
            });
        }
        #endregion

        #region get
        protected HashSet<string> ResolutionsKey = new HashSet<string>();
        protected List<Resolution> Resolutions = new List<Resolution>();
        public virtual string[] GetResolutionStrs() => Resolutions.Select(x => x.ToString()).ToArray();
        #endregion

        #region Callback
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
        }
        protected virtual void OnRevert(TDBSetting data)
        {
            BaseGlobal.LangMgr.Switch(data.LanguageType);

            BaseGlobal.AudioMgr.MuteMusic(data.MuteMusic);
            BaseGlobal.AudioMgr.MuteSFX(data.MuteSFX);
            BaseGlobal.AudioMgr.SetVolumeMusic(data.VolumeMusic);
            BaseGlobal.AudioMgr.SetVolumeSFX(data.VolumeSFX);
            BaseGlobal.AudioMgr.SetVolume(data.Volume);

            SetQuality((int)data.Quality);
            SetResolution(data.Resolution);
            SetWindowType(data.WindowType);
        }
        protected virtual void OnInitSetting()
        {
            OnRevert(Settings);
        }
        protected virtual void OnFirstCreateSetting(TDBSetting arg1)
        {

        }
        #endregion
    }
}