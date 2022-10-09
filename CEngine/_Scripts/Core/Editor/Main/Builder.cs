using CodegenCS;
using CYM.DLC;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace CYM
{
    public class Builder
    {
        #region prop
        /// <summary>
        /// 所有的Bundle映射
        /// string1:原始Bundle,例如: Internal_ui(真实的Bundle名称)
        /// string2:索引Bundle,例如: ui(用户定义的Bundle名称)
        /// </summary>
        static Dictionary<string, string> AllBundles { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 所有共享的Bundle
        /// </summary>
        static HashSet<string> AllSharedBundles { get; set; } = new HashSet<string>();
        /// <summary>
        /// 所有的资源
        /// string1:资源路径
        /// string2:bundle 名字(用户定义的Bundle名称)
        /// </summary>
        static Dictionary<string, string> AllAssets { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 所有的资源
        /// string1:资源路径
        /// string2:buildRule 名字(资源所处的顶层文件夹名称)
        /// </summary>
        static Dictionary<string, string> AllAssetsBuildRuleName { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 已经决定被打包的Internal资源,防止重复打包
        /// </summary>
        static HashSet<string> PackedAssets_Internal { get; set; } = new HashSet<string>();
        /// <summary>
        /// 已经决定被打包的Navite资源,防止重复打包
        /// </summary>
        static HashSet<string> PackedAssets_Native { get; set; } = new HashSet<string>();
        /// <summary>
        /// 已经决定被打包的DLC资源,防止重复打包
        /// </summary>
        static HashSet<string> PackedAssets_DLC { get; set; } = new HashSet<string>();
        /// <summary>
        /// 即将要buid的Bundle数据
        /// </summary>
        static List<AssetBundleBuild> Builds { get; set; } = new List<AssetBundleBuild>();
        /// <summary>
        /// 打包规则
        /// </summary>
        static List<BuildRule> Rules { get; set; } = new List<BuildRule>();
        /// <summary>
        /// 所有的共享文件
        /// string:被依赖的路径key 
        /// List<string>:依赖源的路径
        /// </summary>
        static Dictionary<string, HashSet<string>> AllDependencies { get; set; } = new Dictionary<string, HashSet<string>>();
        /// <summary>
        /// 上一次BuildRule的缓存
        /// </summary>
        static List<AssetBundleBuild> AssetBundleBuildsCache = new List<AssetBundleBuild>();
        static BuildConfig BuildConfig => BuildConfig.Ins;
        static DLCItem Native => DLCConfig.EditorNative;
        static DLCItem Internal => DLCConfig.EditorInternal;
        static DLCConfig DLCConfig => DLCConfig.Ins;
        #endregion

        #region Build utile
        internal static void BuildDLCConfig(DLCItem dlc)
        {
            //清除缓存
            PackedAssets_Internal.Clear();
            PackedAssets_Native.Clear();
            PackedAssets_DLC.Clear();
            Builds.Clear();
            Rules.Clear();
            AllDependencies.Clear();
            AllAssetsBuildRuleName.Clear();
            AllAssets.Clear();
            AllBundles.Clear();
            AllSharedBundles.Clear();
            //永远提前打包Internal
            if (!dlc.IsInternal) 
                BuildDLCConfigInternal(Internal);
            //如果不是NativeDLC则先BuildNativeDLC,防止资源被其他DLC重复打包
            if (!dlc.IsInternal && !dlc.IsNative) 
                BuildDLCConfigInternal(Native);
            BuildDLCConfigInternal(dlc);
        }
        internal static void BuildEXE()
        {
            OnPreBuild();
            _BuildEXE();
            OnPostBuild();
        }
        internal static void BuildBundle(DLCItem dlc)
        {
            OnPreBuild();
            _BuildBundle(dlc);
            OnPostBuild();
        }
        internal static void BuildBundleAndEXE(DLCItem dlc)
        {
            OnPreBuild();
            _BuildBundle(dlc);
            _BuildEXE();
            OnPostBuild();
        }
        #endregion

        #region get AssetBundle name
        /// <summary>
        /// ex. native_prefab,必须加native_前缀,因为需要加载到全局的Manifest,防止dlc之间的重复
        /// <returns></returns>

        // 返回完整的路径作为Bundle名称
        static Tuple<string, string> GetABNamePath(DLCItem item, BuildRuleConfig data, string filePath)
        {
            string source = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)).Replace('\\', '/');
            return new Tuple<string, string>((item.Name + "_" + source).ToLower(), source.ToLower());
        }
        // 根据文件夹以及文件名生成Bundle名称
        static Tuple<string, string> GetABNameFile(DLCItem item, BuildRuleConfig data, string filePath)
        {
            string source = (data.Name + "/" + Path.GetFileNameWithoutExtension(filePath)).ToLower();
            return new Tuple<string, string>((item.Name + "_" + source).ToLower(), source.ToLower());
        }
        // 根据文件夹获得名称
        static Tuple<string, string> GetABNameDirectory(DLCItem item, BuildRuleConfig data)
        {
            string source = data.Name.ToLower();
            return new Tuple<string, string>((item.Name + "_" + source).ToLower(), source.ToLower());
        }
        // 获得共享资源名称
        static Tuple<string, string> GetABNameSharedDiscrete(DLCItem item, string path)
        {
            return new Tuple<string, string>((item.Name + "_" + SysConst.BN_Shared.ToLower() + "/"+FileUtil.MD5(path)).ToLower(), SysConst.BN_Shared.ToLower());
        }
        static Tuple<string, string> GetABNameShared(DLCItem item)
        {
            return new Tuple<string, string>(item.Name + "_" + SysConst.BN_Shared.ToLower(), SysConst.BN_Shared.ToLower());
        }
        #endregion

        #region private utile
        static void BuildDLCConfigInternal(DLCItem dlcItem)
        {
            var builds = AssetBundleBuildsCache = GenerateAssetBundleBuildData(dlcItem);
            string constPath = dlcItem.GetConst();
            string manifestPath = dlcItem.GetManifest();
            string dlcItemPath = dlcItem.GetConfig();

            List<string> bundles = new List<string>();
            List<string> assets = new List<string>();

            if (builds.Count > 0)
            {
                foreach (var item in builds)
                {
                    bundles.Add(item.assetBundleName);
                    foreach (var assetPath in item.assetNames)
                    {
                        assets.Add(assetPath + ":" + (bundles.Count - 1));
                    }
                }
            } 

            #region 创建Manifest文件
            if (File.Exists(manifestPath)) File.Delete(manifestPath);
            DLCManifest dlcManifest = new DLCManifest();
            foreach (var item in builds)
            {
                BundleData tempData = new BundleData();
                tempData.DLCName = dlcItem.Name;
                if (AllBundles.ContainsKey(item.assetBundleName))
                {
                    tempData.BundleName = item.assetBundleName;
                    if (AllSharedBundles.Contains(item.assetBundleName))
                        tempData.IsShared = true;
                }
                else
                {
                    CLog.Error("没有包含:" + item.assetBundleName);
                }
                foreach (var asset in item.assetNames)
                {
                    AssetPathData pathData = new AssetPathData();
                    pathData.FullPath = asset;
                    pathData.FileName = Path.GetFileNameWithoutExtension(asset);
                    if (AllAssets.ContainsKey(asset))
                    {
                        pathData.SourceBundleName = AllAssets[asset];
                    }
                    if (AllAssetsBuildRuleName.ContainsKey(asset))
                    {
                        pathData.BuildRuleName = AllAssetsBuildRuleName[asset];
                    }
                    //获得分类名称
                    if (!pathData.SourceBundleName.IsInv())
                    {
                        pathData.Category = GetParentDir(pathData.FullPath.ToLower());
                        string GetParentDir(string path)
                        {
                            string[] paths = path.Split('/');
                            for (int i = 0; i < paths.Length-1; ++i)
                            {
                                if (paths[i] == pathData.SourceBundleName)
                                {
                                    if (i + 1 < paths.Length-1)
                                        return paths[i + 1];
                                    else return "";
                                }
                            }
                            return "";
                        }
                    }
                    tempData.AssetFullPaths.Add(pathData);
                }
                dlcManifest.Data.Add(tempData);
            }
            FileUtil.SaveJson(manifestPath, dlcManifest, true);
            #endregion

            #region dlcitem
            if (File.Exists(dlcItemPath)) File.Delete(dlcItemPath);
            FileUtil.SaveJson(dlcItemPath, dlcItem.Config, true);
            #endregion

            #region const
            SafeDic<string, Dictionary<string, string>> Consts = new SafeDic<string, Dictionary<string, string>>();
            foreach (var bundleData in dlcManifest.Data)
            {
                string buildRuleName = "";
                foreach (var pathData in bundleData.AssetFullPaths)
                {
                    if (pathData.SourceBundleName.IsInv())
                        continue;
                    if (pathData.BuildRuleName.IsInv())
                        continue;
                    //获得相应的BuildRule名称
                    buildRuleName = pathData.BuildRuleName;
                    //跳过指定的Bundle资源
                    if (pathData.BuildRuleName == SysConst.BN_System ||
                        pathData.BuildRuleName == SysConst.BN_Shared ||
                        pathData.BuildRuleName == SysConst.BN_Config ||
                        pathData.BuildRuleName == SysConst.BN_CSharp ||
                        pathData.BuildRuleName == SysConst.BN_Excel ||
                        pathData.BuildRuleName == SysConst.BN_Lua ||
                        pathData.BuildRuleName == SysConst.BN_Text ||
                        pathData.BuildRuleName == SysConst.BN_Language ||
                        pathData.BuildRuleName == SysConst.BN_Sprite)
                        continue;
                    if (pathData.SourceBundleName == SysConst.BN_System ||
                        pathData.SourceBundleName == SysConst.BN_Shared ||
                        pathData.SourceBundleName == SysConst.BN_Config ||
                        pathData.SourceBundleName == SysConst.BN_CSharp ||
                        pathData.SourceBundleName == SysConst.BN_Excel ||
                        pathData.SourceBundleName == SysConst.BN_Lua ||
                        pathData.SourceBundleName == SysConst.BN_Text ||
                        pathData.SourceBundleName == SysConst.BN_Language ||
                        pathData.SourceBundleName == SysConst.BN_Sprite)
                        continue;
                    //保证变量名称有效
                    var fileName = pathData.FileName.Replace(".", "_").Replace("(", "_").Replace(")", "").Trim();
                    //忽略不需要的Const
                    if (DLCConfig.IsInIgnoreConst(fileName))
                        continue;
                    if (!Consts.ContainsKey(buildRuleName))
                        Consts.Add(buildRuleName, new Dictionary<string, string>());
                    if (Consts[buildRuleName].ContainsKey(fileName))
                        continue;
                    Consts[buildRuleName].Add(fileName, pathData.FileName);
                }
            }

            if (File.Exists(constPath)) File.Delete(constPath);
            var cultureInfo = new System.Globalization.CultureInfo("en-us");
            var w = new CodegenTextWriter(constPath, System.Text.Encoding.UTF8);
            w.WithCurlyBraces("namespace CYM", () =>
            {
                foreach (var item in Consts)
                {
                    var className = cultureInfo.TextInfo.ToTitleCase(item.Key);
                    if (item.Key == SysConst.BN_UI)
                        className = className.ToUpper();
                    w.WithCurlyBraces($"public partial class C{className}", () =>
                    {
                        foreach (var line in item.Value)
                        {
                            w.WriteLine($"public const string {line.Key} = \"{line.Value}\";");
                        }
                    });
                }
            });
            w.Flush();
            w.Dispose();
            #endregion

            CLog.Info("[Builder][{0}] BuildManifest with " + assets.Count + " assets and " + bundles.Count + " bundels.", dlcItem.Name);
        }
        private static void _BuildBundle(DLCItem dlcItem)
        {
            FileUtil.EnsureDirectory(dlcItem.TargetPath);
            BuildDLCConfig(dlcItem);
            BuildAssetBundleOptions op = BuildAssetBundleOptions.None;
            if(BuildConfig.IsCompresse) op |= BuildAssetBundleOptions.ChunkBasedCompression;
            else op |= BuildAssetBundleOptions.UncompressedAssetBundle;
            if (BuildConfig.IsForceBuild)
                op |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            List<AssetBundleBuild> realBundles = new List<AssetBundleBuild>();
            foreach (var item in AssetBundleBuildsCache)
            {
                realBundles.Add(new AssetBundleBuild {
                    assetBundleName = item.assetBundleName+SysConst.Extention_AssetBundle,
                    assetBundleVariant = item.assetBundleVariant,
                    assetNames = item.assetNames,
                    addressableNames = item.addressableNames
                });
            }
            BuildPipeline.BuildAssetBundles(dlcItem.TargetPath, realBundles.ToArray(), op, EditorUserBuildSettings.activeBuildTarget);
            dlcItem.OnPostBuildBundle();
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 活的构建的数据
        /// </summary>
        /// <param name="manifestPath"></param>
        /// <returns></returns>
        private static List<AssetBundleBuild> GenerateAssetBundleBuildData(DLCItem dlc)
        {
            if (dlc.IsInternal) 
                PackedAssets_Internal.Clear();
            else if (dlc.IsNative) 
                PackedAssets_Native.Clear();
            PackedAssets_DLC.Clear();
            Builds.Clear();
            Rules.Clear();
            AllDependencies.Clear();
            AllAssetsBuildRuleName.Clear();
            AllAssets.Clear();
            AllBundles.Clear();
            AllSharedBundles.Clear();

            //建立其他文件
            List<BuildRuleConfig> tempBuildDatas = dlc.BuildRuleData;
            if (tempBuildDatas == null && tempBuildDatas.Count == 0)
            {
                CLog.Error("没有配置相关的AssetBundle信息");
                return null;
            }

            foreach (var item in tempBuildDatas)
            {
                BuildRule buildRule = null;
                if (item.BuildRuleType == BuildRuleType.Directroy)
                    buildRule = new BuildAssetsWithDirectroy(dlc, item);
                else if (item.BuildRuleType == BuildRuleType.FullPath)
                    buildRule = new BuildAssetsWithPath(dlc, item);
                else if (item.BuildRuleType == BuildRuleType.File)
                    buildRule = new BuildAssetsWithFile(dlc, item);
                Rules.Add(buildRule);
            }

            //搜集依赖的资源
            foreach (var item in Rules)
            {
                CollectDependencies(item.Config.FullSearchPath);
            }
            //打包共享的资源
            BuildSharedAssets(dlc);

            foreach (var item in Rules)
            {
                item.Build();
            }
            EditorUtility.ClearProgressBar();
            return Builds;
        }
        static void AddToPackedAssets(string path)
        {
            PackedAssets_Internal.Add(path);
            PackedAssets_Native.Add(path);
            PackedAssets_DLC.Add(path);
        }
        static bool IsContainInPackedAssets(string path)
        {
            if (PackedAssets_Internal.Contains(path) ||
                PackedAssets_Native.Contains(path) ||
                PackedAssets_DLC.Contains(path))
                return true;
            return false;
        }
        private static void CollectDependencies(string path)
        {
            var files = GetFilesWithoutDirectories(path);
            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                var dependencies = AssetDatabase.GetDependencies(item);
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("Collecting... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }

                foreach (var assetPath in dependencies)
                {
                    if (!AllDependencies.ContainsKey(assetPath))
                    {
                        AllDependencies[assetPath] = new HashSet<string>();
                    }

                    if (!AllDependencies[assetPath].Contains(item))
                    {
                        AllDependencies[assetPath].Add(item);
                    }
                }
            }
        }
        /// <summary>
        /// 打包共享的资源
        /// </summary>
        protected static void BuildSharedAssets(DLCItem dlc)
        {
            //打散包的时候需要用到
            Dictionary<string,List<string>> dicAssets = new Dictionary<string,List<string>>();
            List<string> assets = new List<string>();
            foreach (var item in AllDependencies)
            {
                var assetPath = item.Key;
                if (!assetPath.EndsWith(".cs", StringComparison.CurrentCulture) &&
                    !assetPath.EndsWith(".js", StringComparison.CurrentCulture))
                {
                    if (BuildConfig.IsSafeBuild)
                    {
                        //bundle里的资源不包含到共享包
                        if (assetPath.Contains(SysConst.Dir_Bundles))
                            continue;
                        //resource里的资源不包含到共享包
                        if (assetPath.Contains(SysConst.Dir_Resources))
                            continue;
                    }
                    if (IsContainInPackedAssets(assetPath))
                        continue;
                    //如果依赖的资源小于2个,则跳过
                    if (item.Value.Count <= 1)
                        continue;
                    string dirPath = Path.GetDirectoryName(assetPath);
                    if (!dicAssets.ContainsKey(dirPath))
                        dicAssets.Add(dirPath, new List<string>());
                    dicAssets[dirPath].Add(assetPath);
                    assets.Add(assetPath);
                    AddToPackedAssets(assetPath);
                }
            }

            if (dicAssets.Count > 0 &&
                assets.Count > 0)
            {
                //离散共享包
                if (BuildConfig.IsDiscreteShared)
                {
                    foreach (var item in dicAssets)
                    {
                        Tuple<string, string> bundleName = GetABNameSharedDiscrete(dlc, item.Key);
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundleName.Item1;
                        build.assetNames = item.Value.ToArray();
                        Builds.Add(build);
                        AddToAllBundles(bundleName, true);
                    }
                }
                //非离散共享包
                else
                {
                    Tuple<string, string> bundleName = GetABNameShared(dlc);
                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = bundleName.Item1;
                    build.assetNames = assets.ToArray();
                    Builds.Add(build);
                    AddToAllBundles(bundleName, true);
                }
            }
        }

        private static void _BuildEXE()
        {
            string path = BuildConfig.DirPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string location = BuildConfig.ExePath;
            CLog.Log("location = {0}", location);
            BuildOptions op = BuildOptions.None;
            if (BuildConfig.IsUnityDevelopmentBuild)
                op |= BuildOptions.Development;

            List<string> names = new List<string>();
            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
            {
                if (e == null)
                    continue;
                if (e.enabled)
                    names.Add(e.path);
            }
            string[] result = names.ToArray();
            BuildPipeline.BuildPlayer(result, location, GetBuildTarget(), op);
        }
        private static BuildTarget GetBuildTarget()
        {
            if (BuildConfig.Platform == Platform.Windows64)
                return BuildTarget.StandaloneWindows64;
            else if (BuildConfig.Platform == Platform.Android)
                return BuildTarget.Android;
            else if (BuildConfig.Platform == Platform.IOS)
                return BuildTarget.iOS;
            return BuildTarget.StandaloneWindows64;
        }
        private static BuildTargetGroup GetBuildTargetGroup()
        {
            var temp = GetBuildTarget();
            if (
                temp == BuildTarget.StandaloneWindows64 ||
                temp == BuildTarget.StandaloneOSX)
            {
                return BuildTargetGroup.Standalone;
            }
            else if (temp == BuildTarget.Android)
                return BuildTargetGroup.Android;
            else if (temp == BuildTarget.iOS)
                return BuildTargetGroup.iOS;
            return BuildTargetGroup.Standalone;
        }
        // 获得文件路径
        static List<string> GetFilesWithoutDirectories(string path)
        {
            if (!Directory.Exists(path))
            {
                return new List<string>();
            }
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            List<string> items = new List<string>();
            foreach (var item in files)
            {
                if (item.EndsWith(".meta", StringComparison.CurrentCulture))
                    continue;
                var assetPath = item.Replace('\\', '/');
                if (!Directory.Exists(assetPath))
                {
                    items.Add(assetPath);
                }
            }
            return items;
        }
        static void OnPreBuild()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(GetBuildTargetGroup(), GetBuildTarget());
            AssetDatabase.Refresh();
            BuildConfig.LastBuildTime = DateTime.Now.ToString("d");
            EditorUtility.SetDirty(BuildConfig);
            AssetDatabase.SaveAssets();

        }
        static void OnPostBuild()
        {
            //SDK PostBuild
            Type tempSDK = BaseGlobal.GetDistributionType();
            var sdk = Activator.CreateInstance(tempSDK) as BasePlatSDKMgr;
            sdk.PostBuilder();
        }
        #endregion

        #region add
        private static void AddToAllBundles(Tuple<string, string> bundleName, bool isShared = false)
        {
            if (!AllBundles.ContainsKey(bundleName.Item1))
                AllBundles.Add(bundleName.Item1, bundleName.Item2);
            if (isShared)
            {
                if (!AllSharedBundles.Contains(bundleName.Item1))
                    AllSharedBundles.Add(bundleName.Item1);
            }
        }
        private static void AddToAllAssets(string item, Tuple<string, string> bundleName,BuildRuleConfig config)
        {
            if (!AllAssets.ContainsKey(item))
                AllAssets.Add(item, bundleName.Item2);
            if (!AllAssetsBuildRuleName.ContainsKey(item))
                AllAssetsBuildRuleName.Add(item,config.Name.ToLower());
        }
        #endregion

        #region build rule
        public abstract class BuildRule
        {
            protected DLCItem DLCItem;
            #region utile
            public BuildRuleConfig Config { get; private set; }
            public BuildRule(DLCItem dlc, BuildRuleConfig config)
            {
                DLCItem = dlc;
                Config = config;
            }
            public abstract void Build();
            #endregion
        }
        class BuildAssetsWithDirectroy : BuildRule
        {
            public BuildAssetsWithDirectroy(DLCItem dlc, BuildRuleConfig config) : base(dlc, config)
            {
            }
            public override void Build()
            {
                List<string> packedAsset = new List<string>();
                var files = GetFilesWithoutDirectories(Config.FullSearchPath);
                Tuple<string, string> bundleName = GetABNameDirectory(DLCItem, Config);
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    if (!IsContainInPackedAssets(item))
                    {
                        packedAsset.Add(item);
                        AddToPackedAssets(item);
                    }
                    AddToAllAssets(item, bundleName,Config);
                    AddToAllBundles(bundleName);
                }

                if (packedAsset.Count == 0)
                    return;
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = bundleName.Item1;
                build.assetNames = packedAsset.ToArray();
                Builds.Add(build);
            }
        }
        class BuildAssetsWithPath : BuildRule
        {
            public BuildAssetsWithPath(DLCItem dlc, BuildRuleConfig config) : base(dlc, config)
            {
            }
            public override void Build()
            {
                var files = GetFilesWithoutDirectories(Config.FullSearchPath);
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    Tuple<string, string> bundleName = GetABNamePath(DLCItem, Config, item);
                    if (!IsContainInPackedAssets(item))
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundleName.Item1;
                        build.assetNames = new string[] { item };
                        Builds.Add(build);
                        AddToPackedAssets(item);
                    }
                    AddToAllAssets(item, bundleName,Config);
                    AddToAllBundles(bundleName);
                }
            }
        }
        class BuildAssetsWithFile : BuildRule
        {
            public BuildAssetsWithFile(DLCItem dlc, BuildRuleConfig config) : base(dlc, config)
            {
            }
            public override void Build()
            {
                var files = GetFilesWithoutDirectories(Config.FullSearchPath);
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    Tuple<string, string> bundleName = GetABNameFile(DLCItem, Config, item);
                    if (!IsContainInPackedAssets(item))
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundleName.Item1; 
                        build.assetNames = new string[] { item };
                        Builds.Add(build);
                        AddToPackedAssets(item);
                    }
                    AddToAllAssets(item, bundleName,Config);
                    AddToAllBundles(bundleName);
                }
            }
        }
        #endregion
    }
}