//------------------------------------------------------------------------------
// DLCDownloader.cs
// Created by CYM on 2022/9/15
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
namespace CYM.DLC
{
    public class DLCDownloader : BaseCoreMono
    {
        #region prop
        public static bool IsAllAssetBundleDownloaded { get; private set; } = false;
        static BuildConfig BuildConfig => BuildConfig.Ins;
        static string CacheDirPath;
        static string FullNetPath;
        static HashSet<string> DownloadedAssets = new HashSet<string>();
        static Dictionary<string, byte[]> abBytes = new Dictionary<string, byte[]>();
        static Dictionary<string, AssetBundle> LoadedBundles { get; set; } = new Dictionary<string, AssetBundle>();
        static Dictionary<string, UnityWebRequest> DownloadedUnityWebRequest { get; set; } = new Dictionary<string, UnityWebRequest>();
        static Dictionary<string, DownloadData> ServerPredonwloadAssets { get; set; } = new Dictionary<string, DownloadData>();
        static Dictionary<string, DownloadData> LocalPredonwloadAssets { get; set; } = new Dictionary<string, DownloadData>();
        #endregion

        #region life
        void OnGUI()
        {
            if (!IsAllAssetBundleDownloaded)
            {
                float percent = (float)DownloadedAssets.Count / (float)ServerPredonwloadAssets.Count * 100;
                GUILayout.Label($"Downloading...{percent}%");
            }
        }
        #endregion

        #region is
        public static bool IsDownloaded(string item)
        {
            if (Application.isEditor && BuildConfig.IsSimulationEditor)
                return true;
            return DownloadedAssets.Contains(item);
        }
        #endregion

        #region set
        #endregion

        #region get
        static string GetBundleNetPath(string filename)
        {
            string uri = FullNetPath + filename;
            return uri;
        }
        public static AssetBundle GetAssetBundle(string name)
        {
            if (!LoadedBundles.ContainsKey(name))
            {
                AssetBundle ab = AssetBundle.LoadFromFile(CacheDirPath + "/" + name+SysConst.Extention_AssetBundle);
                LoadedBundles.Add(name, ab);
                return ab;
            }
            return LoadedBundles[name];
        }
        public static async Task<AssetBundle> GetAssetBundleAsnyc(string name)
        {
            if (!IsDownloaded(name))
            {
                await EM_Download(name);
            }
            if (!LoadedBundles.ContainsKey(name))
            {
                AssetBundle ab = await AssetBundle.LoadFromFileAsync(CacheDirPath + "/" + name + SysConst.Extention_AssetBundle);
                LoadedBundles.Add(name, ab);
                return ab;
            }
            return LoadedBundles[name];
        }
        #endregion

        #region Enum
        public static IEnumerator StartDownload()
        {
            //设置资源缓存路径
            if (BuildConfig.IsHotFix)
            {
                if (!BuildConfig.IsEditorMode)
                    CacheDirPath = SysConst.Path_PersistentBundle;
                else
                    CacheDirPath = SysConst.Path_StreamingAssets;
                //确保路径存在
                if (!Directory.Exists(CacheDirPath))
                    Directory.CreateDirectory(CacheDirPath);
                string ip = BuildConfig.TestIP;
                if (Version.IsPublic)
                {
                    ip = BuildConfig.PublicIP;
                }
                FullNetPath = ip + "/";

                if (!BuildConfig.IsEditorMode)
                {
                    yield return DownloadConfig();
                    yield return DownloadAllAssetBundle();
                }
            }
            IsAllAssetBundleDownloaded = true;
        }
        static IEnumerator DownloadConfig()
        {
            //读取本地Predownload文件
            var hashdatapath = CacheDirPath + "/" + SysConst.File_Download;
            if (File.Exists(hashdatapath))
            {
                var deserdata = File.ReadAllText(hashdatapath);
                LocalPredonwloadAssets = JsonConvert.DeserializeAnonymousType(deserdata, LocalPredonwloadAssets);
            }

            yield return EM_DownloadDownloadList();

            //写入最新的清单文件
            var serdata = JsonConvert.SerializeObject(ServerPredonwloadAssets);
            File.WriteAllText(hashdatapath, serdata);

            IEnumerator EM_DownloadDownloadList()
            {
                //下载服务器的Predownload文件
                var path = GetBundleNetPath(SysConst.File_Download);
                UnityWebRequest request;
                request = UnityWebRequest.Get(path);
                yield return request.SendWebRequest();
                if (!request.isNetworkError && !request.isHttpError)
                {
                    ServerPredonwloadAssets = JsonConvert.DeserializeAnonymousType(request.downloadHandler.text, ServerPredonwloadAssets);
                    Debug.Log($"Read Download Config Success!!{path}");

                }
                else
                {
                    Debug.LogError(request.error);
                    yield return EM_DownloadDownloadList();
                }
            }
        }

        static IEnumerator EM_Download(string item)
        {
            UnityWebRequest request;
            string uri = GetBundleNetPath(item);
            string savepath = CacheDirPath + "/" + item;
            //已经存在就不再下载
            //Hash验证
            if (
                File.Exists(savepath) &&
                ServerPredonwloadAssets.TryGetValue(item, out DownloadData serverbundledata) &&
                LocalPredonwloadAssets.TryGetValue(item, out DownloadData localbundledata) &&
                localbundledata.HashCode == serverbundledata.HashCode
                )
            {
                DownloadedAssets.Add(item);
                yield break;
            }
            request = UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();
            if (!request.isNetworkError && !request.isHttpError)   
            {
                if (!DownloadedUnityWebRequest.ContainsKey(item))
                {
                    DownloadedUnityWebRequest.Add(item, request);
                    if (Application.isEditor)
                        Debug.Log("下载成功:" + item);
                }

                try
                {
                    FileInfo fi = new FileInfo(savepath);
                    if (!fi.Directory.Exists)
                        fi.Directory.Create();
                    File.WriteAllBytes(savepath, request.downloadHandler.data);
                    DownloadedAssets.Add(item);
                    Debug.LogFormat("文件写入成功:{0}", savepath);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogErrorFormat("文件写入失败:{0}", savepath);
                }
            }
            else
            {
                Debug.LogError(request.error + "，"+item); 
                yield return EM_Download(item); 
            }
        }
        static IEnumerator DownloadAllAssetBundle()
        {
            DownloadedAssets.Clear();
            if (!IsAllAssetBundleDownloaded)
            {
                //下载所有资源文件
                CLog.Log("开始下载远端资源:" + ServerPredonwloadAssets);
                foreach (var item in ServerPredonwloadAssets)
                {
                    //立绘不要预先下载
                    if (item.Value.IsPredownload)
                        yield return EM_Download(item.Key);
                }
                CLog.Log("远端资源下载完成");

                IsAllAssetBundleDownloaded = true;
                yield break;
            }
        }
        #endregion
    }
}