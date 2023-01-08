using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
//------------------------------------------------------------------------------
// BuildConfig.cs
// Created by CYM on 2018/9/5
// 填写类的描述...
//------------------------------------------------------------------------------
namespace CYM
{
    public enum ResBuildType
    { 
        Config,
        Bundle,
    }
    public sealed class BuildConfig : ScriptableObjectConfig<BuildConfig> 
    {
        #region prop
        public override bool IsHideInBuildWindow => true;
        #endregion

        #region Inspector
        [HideInInspector]
        public string CompanyName = "CYM";
        [HideInInspector]
        public string NameSpace = "NationWar";
        [HideInInspector]
        public string MainUIView = "MainUIView";
        public int Major;
        public int Minor;
        public int Data;
        public int Suffix = 1;
        public int Prefs = 0;
        public VersionTag Tag = VersionTag.Preview;
        public bool IsUnityDevelopmentBuild;
        public bool IsShowWinClose = true;
        public bool IsObfuse = true;
        public bool IsShowConsoleBnt = false;
        public bool IgnoreChecker;
        public BuildType BuildType = BuildType.Develop;
        public string LastBuildTime;
        public Platform Platform = Platform.Windows64;
        public string Name = "MainTitle";
        public int Distribution;
        public int TouchDPI = 1;
        public int DragDPI = 800;

        //将所有的配置资源打成AssetBundle来读取，适合移动平台
        [SerializeField]
        public ResBuildType ResBuildType =  ResBuildType.Config;
        [SerializeField,ShowInInspector]
        public HashSet<LanguageType> Language = new HashSet<LanguageType>();
        [SerializeField]
        public bool IsDiscreteShared = true; //是否为离散的共享包
        [SerializeField]
        public bool IsForceBuild = false;
        [SerializeField]
        public bool IsCompresse = true; //是否为压缩格式
        [SerializeField]
        public bool IsSimulationEditor = true;
        //是否初始化的时候加载所有的Directroy Bundle
        [SerializeField]
        public bool IsInitLoadDirBundle = true;
        //是否初始化的时候加载所有Shared Bundle
        [SerializeField]
        public bool IsInitLoadSharedBundle = true;
        [SerializeField]
        public bool IsWarmupAllShaders = true;
        //是否将_Bundle和Resource里共享依赖的资源打包到Shared包里面,如果要开发热更新游戏，此选项需要保持为true
        [SerializeField]
        public bool IsSafeBuild = true;

        public bool IsHotFix = false;
        public string PublicIP = "127.0.0.1:8001";
        public string TestIP = "127.0.0.1:8001";
        #endregion

        #region prop
        public string GameVersion => ToString();
        public string FullVersion => string.Format("{0} {1} {2}", FullName, ToString(), Platform);
        public string DirPath
        {
            get
            {
                if (IsPublic)
                {
                    if (IsTrial)
                    {
                        return Path.Combine(SysConst.Path_Build, Platform.ToString());//xxxx/Windows_x64 Trail
                    }
                    else
                        return Path.Combine(SysConst.Path_Build, Platform.ToString());//xxxx/Windows_x64
                }
                else
                {
                    return Path.Combine(SysConst.Path_Build, FullVersion);//xxxx/BloodyMary v0.0 Preview1 Windows_x64 Steam
                }
            }
        }
        public string ExePath
        {
            get
            {
                if(Platform == Platform.Windows64)
                    return Path.Combine(DirPath, FullName + ".exe");
                else if(Platform == Platform.Android)
                    return Path.Combine(DirPath, FullName + ".apk");
                else if(Platform == Platform.IOS)
                    return Path.Combine(DirPath, FullName + ".ipa");
                throw new Exception();
            }
        }
        public string FullName => Name;
        public override string ToString()
        {
            string str = string.Format("v{0}.{1} {2}{3} {4}", Major, Minor, Tag, Suffix,BaseGlobal.GetDistributionName());
            if (IsDevelop)
                str += " Dev";
            return str;
        }
        #endregion

        #region is
        public bool IsDevelop => BuildType == BuildType.Develop;
        public bool IsPublic => BuildType == BuildType.Public;
        public bool IsTrial => BaseGlobal.GetDistributionName() == nameof(BaseGlobal.Trial);
        public static bool IsWindows => 
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor;
        // 数据库版本是否兼容
        public bool IsInData(int data) => Data == data;
        //是否为编辑器模式
        public bool IsEditorMode
        {
            get
            {
                if (!Application.isEditor) return false;
                if (Application.isEditor && IsSimulationEditor) return true;
                return false;
            }
        }
        //编辑器模式或者纯配置模式
        public bool IsEditorOrConfigMode => IsEditorMode || ResBuildType == ResBuildType.Config;
        //编辑器模式或者AB配置模式
        public bool IsEditorOrAssetBundleMode => IsEditorMode || ResBuildType == ResBuildType.Bundle;
        #endregion
    }
}