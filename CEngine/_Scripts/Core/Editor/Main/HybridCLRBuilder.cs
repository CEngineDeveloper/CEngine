using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CYM
{
    public static class HybridCLRBuilder
    {
        public static string HybridCLRBuildCacheDir => Application.dataPath + "/Resources/Temp/HybridCLRBuildCache";

        public static string AssetBundleOutputDir => $"{HybridCLRBuildCacheDir}/AssetBundleOutput";

        public static string AssetBundleSourceDataTempDir => $"{HybridCLRBuildCacheDir}/AssetBundleSourceData";

        public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
        {
            "mscorlib",
            "System",
            "System.Core",
        };
        public static string ToRelativeAssetPath(string s)
        {
            return s.Substring(s.IndexOf("Assets/"));
        }

        /// <summary>
        /// 将HotFix.dll和HotUpdatePrefab.prefab打入common包.
        /// 将HotUpdateScene.unity打入scene包.
        /// </summary>
        /// <param name="tempDir"></param>
        /// <param name="outputDir"></param>
        /// <param name="target"></param>
        private static void BuildAssetBundles(string tempDir, string outputDir, BuildTarget target, bool buildAot)
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(outputDir);
            CompileDllCommand.CompileDll(target);

            List<AssetBundleBuild> abs = new List<AssetBundleBuild>();

            if (buildAot)
            {
                var aotDllAssets = new List<string>();
                string aotDllDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);

                foreach (var dll in AOTMetaAssemblyNames)
                {
                    string dllPath = $"{aotDllDir}/{dll}.dll";
                    if (!File.Exists(dllPath))
                    {
                        Debug.LogError($"ab中添加AOT补充元数据dll:{dllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                        continue;
                    }
                    string dllBytesPath = $"{tempDir}/{dll}.bytes";
                    File.Copy(dllPath, dllBytesPath, true);
                    aotDllAssets.Add(dllBytesPath);
                    Debug.Log($"[BuildAssetBundles] copy AOT dll {dllPath} -> {dllBytesPath}");
                }

                abs.Add(new AssetBundleBuild
                {
                    assetBundleName = "aotdlls" + SysConst.Extention_AssetBundle,
                    assetNames = aotDllAssets.Select(s => ToRelativeAssetPath(s)).ToArray(), 
                });
            }


            {
                var hotUpdateDllAssets = new List<string>();

                string hotfixDllSrcDir = SettingsUtil.GetHotFixDllsOutputDirByTarget(target);
                foreach (var dll in SettingsUtil.HotUpdateAssemblyFiles)
                {
                    string dllPath = $"{hotfixDllSrcDir}/{dll}";
                    string dllBytesPath = $"{tempDir}/{dll}.bytes";
                    File.Copy(dllPath, dllBytesPath, true);
                    hotUpdateDllAssets.Add(dllBytesPath);
                    Debug.Log($"[BuildAssetBundles] copy hotfix dll {dllPath} -> {dllBytesPath}");
                }

                abs.Add(new AssetBundleBuild
                {
                    assetBundleName = "hotupdatedlls"+SysConst.Extention_AssetBundle,
                    assetNames = hotUpdateDllAssets.Select(s => ToRelativeAssetPath(s)).ToArray(),
                });;
            }

            BuildPipeline.BuildAssetBundles(outputDir, abs.ToArray(), BuildAssetBundleOptions.None, target);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            string streamingAssetPathDst = $"{Application.streamingAssetsPath}";
            Directory.CreateDirectory(streamingAssetPathDst);

            foreach (var ab in abs)
            {
                AssetDatabase.CopyAsset(ToRelativeAssetPath($"{outputDir}/{ab.assetBundleName}"),
                    ToRelativeAssetPath($"{streamingAssetPathDst}/{ab.assetBundleName}"));
            }
        }

        public static void BuildAssetBundleByTarget(BuildTarget target, bool buildAot)
        {
            BuildAssetBundles(AssetBundleSourceDataTempDir, AssetBundleOutputDir, target, buildAot);
        }

        //[MenuItem("HybridCLR/BuildBundles/BuildAll")]
        public static void BuildSceneAssetBundleActiveBuildTarget()
        {
            BuildAssetBundleByTarget(EditorUserBuildSettings.activeBuildTarget, true);
        }


        //[MenuItem("HybridCLR/BuildBundles/BuildBundle")]
        public static void BuildSceneAssetBundleActiveBuildTargetExcludeAOT()
        {
            BuildAssetBundleByTarget(EditorUserBuildSettings.activeBuildTarget, false);
        }


    }
}
