using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.SceneManagement;
using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Commands;
using System.Linq;
using CYM.DLC;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Reflection;
using UnityEditor.Compilation;

namespace CYM
{
    public partial class BuildWindow : EditorWindow
    {
        public static EditorWindow Ins { get; private set; }

        [MenuItem("Tools/Build  &`")]
        public static void ShowBuildWindow()
        {
            Ins = ShowWindow<BuildWindow>();
            Ins.minSize = new Vector2(300, 500);
            RefreshData();
            Save();
        }

        #region prop
        static GUIStyle TitleStyle = new GUIStyle();
        static BuildConfig BuildConfig => BuildConfig.Ins;
        static LocalConfig LocalConfig => LocalConfig.Ins;
        static DLCConfig DLCConfig => DLCConfig.Ins;
        static UIConfig UIConfig => UIConfig.Ins;
        static LogConfig LogConfig => LogConfig.Ins;
        protected static Dictionary<string, string> SceneNames { get; private set; } = new Dictionary<string, string>();
        protected static string VerticalStyle = "HelpBox";
        protected static string ButtonStyle = "minibutton";
        protected static string FoldStyle = "AnimItemBackground";
        protected static string SceneButtonStyle = "ButtonMid;";
        static GUIStyle TitleStyleData;
        static HybridCLRGlobalSettings HybridCLRGlobalSettings;
        static InstallerController HybridCLRInstallerController;
        #endregion

        #region life
        void OnEnable()
        {
            Ins = this;
            RefreshData();
            AssetDatabase.DisallowAutoRefresh();
        }
        #endregion

        #region set
        public static void RefreshData()
        {
            if (Application.isPlaying)
                return;
            TitleStyle.fixedWidth = 100;
            HybridCLRInstallerController = new InstallerController();
            EnsureLanguge();
            EnsureProjectFiles();
            RefreshSceneNames();
            HybridCLRGlobalSettings = Resources.Load<HybridCLRGlobalSettings>(nameof(HybridCLRGlobalSettings));
            DLCConfig.RefreshDLC();
            Ins.titleContent = new GUIContent("Build");
            Ins.Repaint();
            RefreshPlugin();
            ScriptableObjectConfigMgr.RefreshInternalConfig();
            BaseGlobal.RefreshDistribution();
            CLog.Info("打开开发者界面");
        }
        public static void RefreshPlugin()
        {
            Plugins.Clear();
            var data = typeof(BuildWindow).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var item in data)
            {
                if (item.FieldType.Name ==  typeof(PluginEditor).Name)
                {
                    var val = item.GetValue(item);
                    Plugins.Add(val);
                }
            }
        }
        public void DrawGUI()
        {
            //GUI.enabled = !Application.isPlaying;
            Present_Info();
            Present_Version();
            Present_Settings();
            Present_Languge();
            Present_DLC();
            Present_HotFix();
            Present_Build();
            Present_Explorer();
            Present_SubWindow();
            Present_ConfigWindow();
            Present_PluginWindow();
            Present_LevelList();
            Present_Ccustom();
            Present_Other();
            //GUI.enabled = Application.isPlaying;
        }
        #endregion

        #region info
        public void Present_Info()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldInfo = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldInfo, "信息"))
            {
                if (!BuildConfig.LastBuildTime.IsInv())
                    EditorGUILayout.LabelField("BuildTime:" + BuildConfig.LastBuildTime);
                EditorGUILayout.LabelField(string.Format("版本:{0}", BuildConfig));
                EditorGUILayout.LabelField(string.Format("完整:{0}", BuildConfig.FullVersion));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Version
        public void Present_Version()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldVersion = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldVersion, "版本"))
            {
                BuildConfig.Platform = (Platform)EditorGUILayout.Popup("目标", (int)BuildConfig.Platform, Enum.GetNames(typeof(Platform)));
                if (BaseGlobal.DistributionOptions != null)
                {
                    BuildConfig.Distribution = EditorGUILayout.Popup("发布渠道", BuildConfig.Distribution, BaseGlobal.DistributionOptions);
                }
                BuildConfig.BuildType = (BuildType)EditorGUILayout.EnumPopup("打包版本", BuildConfig.BuildType);

                BuildConfig.Name = EditorGUILayout.TextField("名称", BuildConfig.Name);
                if (PlayerSettings.productName != BuildConfig.Name)
                {
                    PlayerSettings.productName = BuildConfig.Name;
                    RefreshAppIdentifier();
                }
                if (PlayerSettings.companyName != BuildConfig.CompanyName)
                {
                    PlayerSettings.companyName = BuildConfig.Name;
                    RefreshAppIdentifier();
                }

                BuildConfig.Major = EditorGUILayout.IntField("主版本", BuildConfig.Major);
                BuildConfig.Minor = EditorGUILayout.IntField("副版本", BuildConfig.Minor);
                BuildConfig.Data = EditorGUILayout.IntField("存档标", BuildConfig.Data);
                BuildConfig.Prefs = EditorGUILayout.IntField("Prefs", BuildConfig.Prefs);

                EditorGUILayout.BeginHorizontal();
                BuildConfig.Tag = (VersionTag)EditorGUILayout.EnumPopup("后缀", BuildConfig.Tag);
                BuildConfig.Suffix = EditorGUILayout.IntField(BuildConfig.Suffix);
                EditorGUILayout.EndHorizontal();


                if (PlayerSettings.bundleVersion != BuildConfig.ToString())
                    PlayerSettings.bundleVersion = BuildConfig.ToString();

                if (PlayerSettings.productName != BuildConfig.Name)
                    PlayerSettings.productName = BuildConfig.Name;

                if (PlayerSettings.companyName != BuildConfig.CompanyName)
                    PlayerSettings.companyName = BuildConfig.CompanyName;

                EditorGUILayout.BeginVertical();
                OnDrawSettings();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();

            void RefreshAppIdentifier()
            {
                PlayerSettings.applicationIdentifier = "com." + BuildConfig.CompanyName + "." + BuildConfig.Name;
            }
        }
        #endregion

        #region Setting
        public void Present_Settings()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.FoldSetting = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.FoldSetting, "设置"))
            {
                EditorUserBuildSettings.development = BuildConfig.IsUnityDevelopmentBuild = EditorGUILayout.BeginToggleGroup("Debug Build", BuildConfig.IsUnityDevelopmentBuild);
                EditorGUILayout.EndToggleGroup();

                bool preSimu = BuildConfig.IsSimulationEditor;
                BuildConfig.IsSimulationEditor = EditorGUILayout.BeginToggleGroup("Simulation Editor", BuildConfig.IsSimulationEditor);
                EditorGUILayout.EndToggleGroup();
                if (preSimu != BuildConfig.IsSimulationEditor)
                {
                    DLCConfig.RefreshDLC();
                }

                BuildConfig.IsHotFix = HybridCLRGlobalSettings.enable = EditorGUILayout.BeginToggleGroup("Enable HotFix", BuildConfig.IsHotFix);
                BuildConfig.TestIP = EditorGUILayout.TextField("Hotfix Test IP", BuildConfig.TestIP);
                BuildConfig.PublicIP = EditorGUILayout.TextField("Hotfix Public IP", BuildConfig.PublicIP);
                EditorGUILayout.EndToggleGroup();

                ResBuildType preResBuildType = BuildConfig.ResBuildType;
                if (BuildConfig.IsHotFix)
                {
                    BuildConfig.ResBuildType = ResBuildType.Bundle;
                    EditorGUILayout.LabelField("ResBuildType", BuildConfig.ResBuildType.ToString());
                }
                else
                {
                    BuildConfig.ResBuildType = (ResBuildType)EditorGUILayout.EnumPopup("ResBuildType", BuildConfig.ResBuildType);
                }
                if (preResBuildType != BuildConfig.ResBuildType)
                {
                    DLCConfig.RefreshDLC();
                }

                EditorGUILayout.LabelField("分辨率", TitleStyleData);
                BuildConfig.TouchDPI = EditorGUILayout.IntField("Touch DPI", BuildConfig.TouchDPI);
                BuildConfig.DragDPI = EditorGUILayout.IntField("Drag DPI", BuildConfig.DragDPI);
                UIConfig.Width = EditorGUILayout.IntField("Width", UIConfig.Width);
                UIConfig.Height = EditorGUILayout.IntField("Height", UIConfig.Height);

                EditorGUILayout.LabelField("杂项", TitleStyleData);
                LogConfig.Enable = EditorGUILayout.Toggle("Is Log", LogConfig.Enable);
                UIConfig.IsShowLogo = EditorGUILayout.Toggle("Is Show Logo", UIConfig.IsShowLogo);
                BuildConfig.IgnoreChecker = EditorGUILayout.Toggle("Is Ignore Checker", BuildConfig.IgnoreChecker);
                BuildConfig.IsShowWinClose = EditorGUILayout.Toggle("Is Show WinClose", BuildConfig.IsShowWinClose);
                BuildConfig.IsShowConsoleBnt = EditorGUILayout.Toggle("Is Show ConsoleBnt", BuildConfig.IsShowConsoleBnt);
                BuildConfig.IsInitLoadDirBundle = EditorGUILayout.Toggle("Is Init Load Dir Bundle", BuildConfig.IsInitLoadDirBundle);
                BuildConfig.IsInitLoadSharedBundle = EditorGUILayout.Toggle("Is Init Load Shared Bundle", BuildConfig.IsInitLoadSharedBundle);
                BuildConfig.IsWarmupAllShaders = EditorGUILayout.Toggle("Is Warmup All Shaders", BuildConfig.IsWarmupAllShaders);

                EditorGUILayout.LabelField("打包", TitleStyleData);
                BuildConfig.IsDiscreteShared = EditorGUILayout.Toggle("Is Discrete Shared", BuildConfig.IsDiscreteShared);
                BuildConfig.IsForceBuild = EditorGUILayout.Toggle("Is Force Build", BuildConfig.IsForceBuild);
                BuildConfig.IsCompresse = EditorGUILayout.Toggle("Is Compresse", BuildConfig.IsCompresse);
                BuildConfig.IsSafeBuild = EditorGUILayout.Toggle("Is Safe Build", BuildConfig.IsSafeBuild);
                OnDrawSettings();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 语言
        static LanguageType languageType = LanguageType.Chinese;
        static LanguageType? lastDeleteLanguge = null;
        public void Present_Languge()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.FoldLang = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.FoldLang, "语言"))
            {
                foreach (var item in BuildConfig.Language)
                {
                    DrawItem(item);
                }
                GUILayout.BeginHorizontal();
                languageType = (LanguageType)EditorGUILayout.EnumPopup("",languageType, (GUIStyle)"DropDownToggleButton");
                if (GUILayout.Button("添加", GUILayout.Width(50)))
                {
                    BuildConfig.Language.Add(languageType);
                    Repaint();
                }
                GUILayout.EndHorizontal();

                if (lastDeleteLanguge != null)
                {
                    BuildConfig.Language.Remove(lastDeleteLanguge.Value);
                    EnsureLanguge();
                    Repaint();
                    lastDeleteLanguge = null;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();

            void DrawItem(LanguageType language)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(language.ToString(), (GUIStyle)"HelpBox");
                if (GUILayout.Button("X",GUILayout.Width(25)))
                {
                    lastDeleteLanguge = language;
                }
                GUILayout.EndHorizontal();
            }
        }
        static void EnsureLanguge()
        {
            if (BuildConfig.Language.Count == 0)
            {
                BuildConfig.Language.Add(LanguageType.Chinese);
            }
        }
        #endregion

        #region DLC
        static string newDlcName = "NewDlc";
        public void Present_DLC()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.FoldDLC = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.FoldDLC, "DLC"))
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                newDlcName = EditorGUILayout.TextField("名称", newDlcName);
                if (GUILayout.Button("Add"))
                {
                    if (!newDlcName.IsInv() && EditorUtility.DisplayDialog("警告!", "您确定要添加此DLC吗？", "确定要添加", "取消"))
                    {
                        DLCConfig.AddDLC(newDlcName);
                        Save();
                    }
                }
                EditorGUILayout.EndHorizontal();

                foreach (var item in DLCConfig.EditorExtend)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(item.Name, GUILayout.MaxWidth(100));

                    if (GUILayout.Button("Build Config"))
                    {
                        Builder.BuildDLCConfig(item);
                        AssetDatabase.Refresh();
                    }
                    if (GUILayout.Button("Build Bundle"))
                    {
                        Builder.BuildBundle(item);
                        EditorUtility.DisplayDialog("提示!", $"恭喜! {item.Name} 已经打包完成!!", "确定");
                        CLog.Green($"恭喜! {item.Name} 已经打包完成!!");
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        if (EditorUtility.DisplayDialog("警告!", "您确定要删除此DLC吗？", "确定要删除", "取消"))
                        {
                            DLCConfig.RemoveDLC(item.Name);
                            FileUtil.DeleteDir(item.AbsRootPath);
                            Save();
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("RrfreshDLC"))
                {
                    DLCConfig.RefreshDLC();
                }
                if (GUILayout.Button("RecreateDLC"))
                {
                    DLCConfig.RecreateDLC();
                    DLCConfig.RefreshDLC();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 构建
        public void Present_Build()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.FoldBuild = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.FoldBuild, "构建"))
            {
                EditorGUILayout.BeginVertical();
                foreach (var item in DLCConfig.EditorInner)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(item.Name, GUILayout.MaxWidth(100));

                    if (GUILayout.Button("Build Config"))
                    {
                        Builder.BuildDLCConfig(item);
                        AssetDatabase.Refresh();
                    }
                    if (GUILayout.Button("Build Bundle"))
                    {
                        if (!CheckKey()) return;
                        Builder.BuildBundle(item);
                        EditorUtility.DisplayDialog("提示!", $"恭喜! {item.Name} 已经打包完成!!", "确定");
                        CLog.Green($"恭喜! {item.Name} 已经打包完成!!");
                    }

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Path:" + LocalConfig.ABDownloadPath, (GUIStyle)"HelpBox"))
                {
                    LocalConfig.ABDownloadPath = EditorUtility.OpenFolderPanel("Path", Application.dataPath, "AssetBundle");
                }
                if (GUILayout.Button("Copy"))
                {
                    if (!Directory.Exists(LocalConfig.ABDownloadPath))
                    {
                        CLog.Error("路径不存在:" + LocalConfig.ABDownloadPath);
                        return;
                    }
                    FileUtil.CopyDir(Application.streamingAssetsPath, LocalConfig.ABDownloadPath, true, true);
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Build EXE"))
                {
                    if (CheckEorr()) return;
                    if (!CheckDevBuildWarring()) return;
                    if (!CheckAuthority()) return;
                    if (!CheckKey()) return;
                    RefreshData();
                    Builder.BuildEXE();
                    EditorUtility.DisplayDialog("提示!", $"恭喜! 程序构建已经打包完成!!", "确定");
                    CLog.Green($"恭喜! 程序构建已经打包完成!!");
                }
                if (GUILayout.Button("Build ABEXE"))
                {
                    if (CheckEorr()) return;
                    if (!CheckDevBuildWarring()) return;
                    if (!CheckAuthority()) return;
                    if (!CheckKey()) return;
                    RefreshData();
                    foreach (var item in DLCConfig.EditorAll)
                    {
                        Builder.BuildBundle(item);
                    }
                    Builder.BuildEXE();
                    EditorUtility.DisplayDialog("提示!", $"恭喜! 一键打包已经打包完成!!", "确定");
                    CLog.Green($"恭喜! 一键打包已经打包完成!!");
                    EditorApplication.Beep();
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Build All"))
                {
                    if (CheckEorr()) return;
                    if (!CheckDevBuildWarring()) return;
                    if (!CheckAuthority()) return;
                    if (!CheckKey()) return;
                    RefreshData();

                    BuildHotFix();

                    foreach (var item in DLCConfig.EditorAll)
                    {
                        Builder.BuildBundle(item);
                    }
                    Builder.BuildEXE();
                    SaveDownloadFile();
                    EditorUtility.DisplayDialog("提示!", $"恭喜! 一键打包已经打包完成!!", "确定");
                    CLog.Green($"恭喜! 一键打包已经打包完成!!");
                    EditorApplication.Beep();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 热更
        public void Present_HotFix()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.FoldHotFix = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.FoldHotFix, "热更"))
            {
                EditorGUILayout.BeginVertical();
                if (HybridCLRInstallerController.HasInstalledHybridCLR())
                {
                    EditorGUILayout.Space();
                    LocalConfig.FoldHotFixGenerate = EditorGUILayout.Toggle("Show Generate Detail", LocalConfig.FoldHotFixGenerate, (GUIStyle)"SoloToggle");
                    EditorGUILayout.Space();
                    if (GUILayout.Button("CompileDll"))
                    {
                        CompileDllCommand.CompileDllActiveBuildTarget();
                    }

                    if (!LocalConfig.FoldHotFixGenerate)
                    {
                        if (GUILayout.Button("GenerateAll"))
                        {
                            PrebuildCommand.GenerateAll();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("LinkXml"))
                        {
                            LinkGeneratorCommand.GenerateLinkXml();
                        }
                        if (GUILayout.Button("MethodBridge"))
                        {
                            MethodBridgeGeneratorCommand.GenerateMethodBridge();
                        }
                        if (GUILayout.Button("AOTGenericReference"))
                        {
                            AOTReferenceGeneratorCommand.GenerateAOTGenericReference();
                        }
                        if (GUILayout.Button("ReversePInvokeWrapper"))
                        {
                            ReversePInvokeWrapperGeneratorCommand.GenerateReversePInvokeWrapper();
                        }
                    }

                    if (GUILayout.Button("Build Bundle"))
                    {
                        HybridCLRBuilder.BuildSceneAssetBundleActiveBuildTarget();
                    }
                    if (GUILayout.Button("Build All"))
                    {
                        BuildHotFix();
                    }
                    if (GUILayout.Button("Generate Download List"))
                    {
                        SaveDownloadFile();
                    }
                }
                else
                {
                    if (GUILayout.Button("Install IL2CPP_Plus(huatuo)..."))
                    {
                        InstallerWindow window = EditorWindow.GetWindow<InstallerWindow>("HybridCLR Installer", true);
                        window.minSize = new Vector2(800f, 500f);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        static void BuildHotFix()
        {
            if (!HybridCLRGlobalSettings.enable)
                return;
            CompileDllCommand.CompileDllActiveBuildTarget();
            PrebuildCommand.GenerateAll();
            HybridCLRBuilder.BuildSceneAssetBundleActiveBuildTarget();
        }
        #endregion

        #region 资源管理器
        public void Present_Explorer()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldExplorer = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldExplorer, "链接"))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Persistent"))
                {
                    FileUtil.OpenExplorer(Application.persistentDataPath);
                }
                else if (GUILayout.Button("删除 Persistent"))
                {
                    FileUtil.DeleteDir(Application.persistentDataPath);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Project"))
                {
                    FileUtil.OpenExplorer(SysConst.Path_Project);
                }
                else if (GUILayout.Button("Bin"))
                {
                    FileUtil.OpenExplorer(SysConst.Path_Build);
                }
                else if (GUILayout.Button("Language"))
                {
                    OpenLanguage();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("打开VS"))
                {
                    LaunchVS();
                }
                else if (GUILayout.Button("打开Lua"))
                {
                    LaunchLua();
                }
                else if (GUILayout.Button("运行"))
                {
                    FileUtil.StartEXE(BuildConfig.ExePath);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                OnDrawPresentExplorer();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 关卡列表
        [HideInInspector]
        static Vector2 scrollSceneList = Vector2.zero;
        public void Present_LevelList()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldSceneList = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldSceneList, "场景"))
            {
                if (SceneNames.Count > 5)
                    scrollSceneList = EditorGUILayout.BeginScrollView(scrollSceneList, GUILayout.ExpandHeight(false), GUILayout.MinHeight(300));

                EditorGUILayout.BeginHorizontal();
                DrawGoToBundleSystemSceneButton(SysConst.SCE_Start);
                DrawGoToBundleSystemSceneButton(SysConst.SCE_Preview);
                DrawGoToBundleSystemSceneButton(SysConst.SCE_Test);
                EditorGUILayout.EndHorizontal();
                if (SceneNames != null)
                {
                    foreach (var item in SceneNames)
                    {
                        if (item.Key == SysConst.SCE_Preview ||
                            item.Key == SysConst.SCE_Start ||
                            item.Key == SysConst.SCE_Test)
                            continue;
                        DrawGoToBundleSceneButton(item.Key, item.Value);
                    }
                }
                if (SceneNames.Count > 5)
                    EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 自定义,插件拓展自定义的UI
        public static HashList<PluginEditor> Plugins { get; private set; } = new HashList<PluginEditor>();
        public void Present_Ccustom()
        {
            foreach (var item in Plugins)
            {
                item.OnGUI?.Invoke();
            }
        }
        #endregion

        #region 其他
        public void Present_Other()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldOther = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldOther, "其他"))
            {
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("创建启动器"))
                {
                    GameObject Starter = new GameObject(nameof(Starter));
                    Starter.AddComponent<Starter>();
                }
                else if (GUILayout.Button("运行游戏"))
                {
                    FileUtil.OpenExplorer(BuildConfig.ExePath);
                    CLog.Info("Run:{0}", BuildConfig.ExePath);
                }
                else if (GUILayout.Button("保存"))
                {
                    Save();
                }
                else if (GUILayout.Button("拍照"))
                {
                    ScreenCapture.CaptureScreenshot(SysConst.Path_Screenshot + "/Screenshot.png", ScreenCapture.StereoScreenCaptureMode.BothEyes);
                    FileUtil.OpenFile(SysConst.Path_Screenshot);
                }
                else if (GUILayout.Button("编译"))
                {
                    Compilation();
                }
                else if (GUILayout.Button("刷新"))
                {
                    RefreshData();
                }
                else if (GUILayout.Button("运行"))
                {
                    AssetDatabase.Refresh();
                    GoToScene(GetSystemScenesPath(SysConst.SCE_Start), OpenSceneMode.Single);
                    EditorApplication.isPlaying = true;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                OnDrawPresentScriptTemplate();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region sub Window
        public void Present_SubWindow()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldSubWindow = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldSubWindow, "窗口"))
            {
                if (GUILayout.Button("GUIStyle")) GUIStyleWindow.ShowWindow();
                else if (GUILayout.Button("Prefs")) PrefsWindow.ShowWindow();
                else if (GUILayout.Button("ColorPicker")) ColorPickerWindow.ShowWindow();
                else if (GUILayout.Button("Dependencies")) DependencyWindow.ShowWindow();
                else if (GUILayout.Button("ParticleScaler")) ParticleScalerWindow.ShowWindow();
                else if (GUILayout.Button("UnityTexture")) UnityTextureWindow.ShowWindow();
                else if (GUILayout.Button("RampTexture")) RampTexGenWindow.ShowWindow();
                else if (GUILayout.Button("Screenshot")) ScreenshotWindow.ShowWindow();
                else if (GUILayout.Button("TerrainHeight")) TerrainHeightWindow.ShowWindow();
                else if (GUILayout.Button("Console")) EditorUtility.OpenPropertyEditor(Starter.ConsoleObj);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region config windown
        public void Present_ConfigWindow()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldConfigWindow = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldConfigWindow, "配置"))
            {
                foreach (var item in ScriptableObjectConfigMgr.ConfigWindows)
                {
                    if (item.Key is IScriptableObjectConfig inter && !inter.IsHideInBuildWindow)
                    {
                        if (GUILayout.Button(item.Key.name))
                            ScriptableObjectConfigWindow.ShowConfigWindow(item.Key);
                    }
                }

                if (GUILayout.Button("刷新配置"))
                {
                    ScriptableObjectConfigMgr.RefreshInternalConfig();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Plugin window
        public void Present_PluginWindow()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldPluginWindow = EditorGUILayout.BeginFoldoutHeaderGroup(LocalConfig.Ins.FoldPluginWindow, "插件"))
            {
                foreach (var item in PluginConfig.Ins.Datas)
                {
                    GUILayout.Button(item.Key, (GUIStyle)"ObjectFieldThumb");
                }

                if (GUILayout.Button("刷新插件"))
                {
                    PluginConfig.Ins.Refresh();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region utile
        public static void Compilation()
        {
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();
        }
        public static void Save()
        {
            EditorUtility.SetDirty(HybridCLRGlobalSettings);
            EditorUtility.SetDirty(LogConfig);
            EditorUtility.SetDirty(UIConfig);
            EditorUtility.SetDirty(DLCConfig);
            EditorUtility.SetDirty(LocalConfig);
            EditorUtility.SetDirty(BuildConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        public bool CheckDevBuildWarring()
        {
            if (BuildConfig.IsDevelop)
            {
                return EditorUtility.DisplayDialog("警告!", "您确定要构建吗?因为当前是Dev版本", "确定要构建Dev版本", "取消");
            }
            return true;
        }
        protected bool CheckAuthority()
        {
            CLog.Info("打包:" + SystemInfo.deviceName);
            return true;
        }
        protected bool CheckEorr()
        {
            if (BuildConfig.IgnoreChecker)
                return false;

            if (CheckIsHaveError())
                return true;
            return false;
        }
        protected bool CheckKey()
        {
            return true;
        }
        protected bool DoCheckWindow<T>() where T : CheckerWindow
        {
            T window = GetWindow<T>();
            window.CheckAll();
            window.Close();
            return window.IsHaveError();
        }
        protected static EditorWindow ShowWindow<T>() where T : EditorWindow
        {
            var ret = GetWindow<T>();
            ret.ShowPopup();
            return ret;
        }
        protected static string GetSystemScenesPath(string name)
        {
            return string.Format(SysConst.Format_BundleSystemScenesPath, name);
        }
        protected void DrawGoToBundleSystemSceneButton(string name)
        {
            if (GUILayout.Button(name))
            {
                GoToScene(GetSystemScenesPath(name));
                if (name == SysConst.SCE_Start)
                {
                }
            }
        }
        protected void DrawGoToBundleSceneButton(string name, string fullPath)
        {
            if (GUILayout.Button(name))
            {
                GoToSceneByFullPath(fullPath);
            }
        }
        protected static void GoToScene(string path, OpenSceneMode mode = OpenSceneMode.Single)
        {
            GoToSceneByFullPath(Application.dataPath + path, mode);
        }
        protected static void GoToSceneByFullPath(string path, OpenSceneMode mode = OpenSceneMode.Single)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(path, mode);
        }
        protected static void RefreshSceneNames()
        {
            var paths = Directory.GetFiles(Path.Combine(Application.dataPath, SysConst.Dir_Art, "Scene"), "*.unity", SearchOption.AllDirectories);
            SceneNames.Clear();
            foreach (var item in paths)
            {
                string tempName = Path.GetFileNameWithoutExtension(item);
                if (tempName == SysConst.SCE_Preview ||
                    tempName == SysConst.SCE_Start ||
                    tempName == SysConst.SCE_Test)
                    continue;
                if (!SceneNames.ContainsKey(tempName))
                    SceneNames.Add(tempName, item);
            }
        }
        public static void EnsureProjectFiles()
        {
            FileUtil.EnsureDirectory(SysConst.Path_Arts);
            FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Arts, "Scene"));

            FileUtil.EnsureDirectory(SysConst.Path_Bundles);

            FileUtil.EnsureDirectory(SysConst.Path_Resources);
            FileUtil.EnsureDirectory(SysConst.Path_ResourcesConfig);
            FileUtil.EnsureDirectory(SysConst.Path_ResourcesTemp);
            FileUtil.EnsureDirectory(SysConst.Path_ResourcesConst);

            FileUtil.EnsureDirectory(SysConst.Path_Funcs);
            FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Funcs, "GlobalMgr"));
            FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Funcs, "Main"));
            FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Funcs, "Table"));
            FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Funcs, "UI"));
            FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Funcs, "UIMgr"));
            FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Funcs, "UnitMgr"));
            FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Funcs, "UnitMono"));
            FileUtil.EnsureDirectory(SysConst.Path_StreamingAssets);
        }
        public static void SaveDownloadFile()
        {
            string findPath = Application.streamingAssetsPath;
            List<string> pathes = Directory.GetFiles(findPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => !file.ToLower().EndsWith("meta"))
                    .Where(file => !file.ToLower().EndsWith("bytes"))
                    .Where(file => !file.ToLower().EndsWith("json"))
                    .ToList();

            Dictionary<string, DownloadData> predonwload = new Dictionary<string, DownloadData>();
            foreach (var item in pathes)
            {
                string md5Hash = "";
                using (FileStream fs = File.OpenRead(item))
                {
                    using (var crypto = MD5.Create())
                    {
                        md5Hash = BitConverter.ToString(crypto.ComputeHash(fs));
                    }
                }

                var filename = item.Replace(findPath + "\\", "");
                predonwload.Add(filename, new DownloadData { IsPredownload = true, HashCode = md5Hash });
            }
            var serdata = JsonConvert.SerializeObject(predonwload, Formatting.Indented);
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, SysConst.File_Download), serdata);
            AssetDatabase.Refresh();
        }
        static void LaunchVS()
        {
            string assetsDirName = Path.Combine(SysConst.RPath_CEngine, "Scripts/Core/Mono/Global/BaseGlobal.cs");

            if (!File.Exists(assetsDirName))
            {
                Debug.Log("BaseGlobal.cs doesn't exist? Aborting...");
                return;
            }
            AssetDatabase.ImportAsset(assetsDirName, ImportAssetOptions.ForceUpdate);
            var asset = AssetDatabase.LoadAssetAtPath(assetsDirName, typeof(TextAsset)) as TextAsset;
            AssetDatabase.OpenAsset(asset);
        }
        static void LaunchLua()
        {
            string assetsDirName = Path.Combine(SysConst.RPath_Bundles, "Lua/Lua.code-workspace");

            if (!File.Exists(assetsDirName))
            {
                Debug.Log("Lua.code-workspace doesn't exist? Aborting...");
                return;
            }
            FileUtil.OpenExplorer(assetsDirName);
        }
        static void OpenLanguage()
        {
            string assetsDirName = Path.Combine(SysConst.RPath_Bundles, "Language");

            if (!Directory.Exists(assetsDirName))
            {
                Debug.Log("Language doesn't exist? Aborting...");
                return;
            }
            FileUtil.OpenExplorer(assetsDirName);
        }
        public static void GoToStart() => GoToScene(GetSystemScenesPath(SysConst.SCE_Start));
        #endregion

        #region Override
        [HideInInspector]
        public Vector2 scrollPosition = Vector2.zero;
        protected void OnGUI()
        {
            if (BuildConfig == null)
                return;
            if (TitleStyleData == null)
            {
                TitleStyleData = (GUIStyle)"flow varPin tooltip";
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawGUI();
            EditorGUILayout.EndScrollView();
        }
        protected virtual void OnDrawPresentScriptTemplate() { }
        protected virtual void OnDrawPresentExplorer() { }
        protected virtual void OnDrawSubwindow() { }
        protected virtual void OnDrawSettings() { }
        protected virtual bool CheckIsHaveError() => DoCheckWindow<CheckerWindow>();
        #endregion
    }
}