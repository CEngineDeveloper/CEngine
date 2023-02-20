//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using CYM.DLC;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;

namespace CYM
{
    public class RsCacher<T>:IRsCacher , IRsCacherT<T>
        where T : UnityEngine.Object
    {
        public string Bundle { get; private set; }
        public ObjectRegister<T> Data { get; private set; } = new ObjectRegister<T>();
        public RsCacher(string bundleName)
        {
            Bundle = bundleName.ToLower();
            BaseGRMgr.BundleCachers.Add(bundleName,this);
        }

        public bool IsHave(string name)
        {
            if (DLCManager.IsHave(Bundle, name))
                return true;
            return false;
        }

        public T Get(string name,bool isLogError=true)
        {
            return GetResWithCache(Bundle,name, Data, isLogError);
        }
        public T Spawn(string name)
        {
            return GameObject.Instantiate<T>(Get(name));
        }

        public List<T> Get(List<string> names)
        {
            List<T> clips = new List<T>();
            if (names == null)
                return clips;
            for (int i = 0; i < names.Count; ++i)
            {
                if (names[i].IsInv())
                    continue;
                var temp = Get(names[i]);
                if (temp != null)
                    clips.Add(temp);
            }
            return clips;
        }
        public List<T> Get()
        {
            List<T> clips = new List<T>();
            foreach (var item in DLCManager.GetAssetsByBundle(Bundle))
            {
                var temp = Get(item);
                if (temp != null)
                    clips.Add(temp);
            }
            return clips;
        }
        public List<string> GetStrsByCategory(string category)
        {
            return DLCManager.GetAssetsByCategory(Bundle, category);
        }
        public List<string> GetStrs()
        {
            return DLCManager.GetAssetsByBundle(Bundle);
        }
        public List<T> GetByCategory(string category)
        {
            List<T> clips = new List<T>();
            foreach (var item in DLCManager.GetAssetsByCategory(Bundle, category))
            {
                var temp = Get(item);
                if (temp != null)
                    clips.Add(temp);
            }
            return clips;
        }
        private T GetResWithCache(string rulekey,string name, ObjectRegister<T> cache, bool logError = true)
        {
            DLCManager.IsNextLogError = logError;
            string bundleName = DLCManager.GetSourceBundleName(rulekey+name);
            if (name.IsInv())
                return null;
            if (
                BaseGlobal.LoaderMgr!=null &&
                !BaseGlobal.LoaderMgr.IsLoadEnd
                )
                return null;
            if (cache.ContainsKey(name))
            {
                return cache[name];
            }
            else
            {
                var asset = DLCManager.LoadAsset<T>(bundleName, name);
                if (asset == null)
                    return null;
                T retGO = asset.Object as T;
                if (retGO == null)
                {
                    if (logError)
                        CLog.Error("no this res in bundle {0}, name = {1}", bundleName, name);
                }
                else
                {
                    if (!cache.ContainsKey(retGO.name)) cache.Add(retGO);
                    else cache[retGO.name] = retGO;
                }
                return retGO;
            }
        }

        public void RemoveNull()
        {
            Data.RemoveNull();
        }
    }
    /// <summary>
    /// $LocalPlayer表示动态ID
    /// </summary>
    public class BaseGRMgr : BaseGFlowMgr, ILoader
    {
        #region member variable
        private DLCConfig DLCConfig => DLCConfig.Ins;
        public static Dictionary<string, IRsCacher> BundleCachers { get; private set; } = new Dictionary<string, IRsCacher>();
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            OnAddDynamicRes();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

        }
        protected virtual void OnAddDynamicRes() { }
        #endregion

        #region get
        public GameObject GetResources(string path, bool instance = false)
        {
            var temp = Resources.Load<GameObject>(path);
            if (temp == null)
            {
                CLog.Error("错误,没有这个对象:"+ path);
                return null;
            }
            if (instance)
            {
                var ret = GameObject.Instantiate(temp);
                ret.transform.SetParent(SelfBaseGlobal.Trans);
                return ret;
            }
            else return temp;
        }
        public T GetResources<T>(string path) where T: UnityEngine.Object
        {
            var temp = Resources.Load<T>(path);
            if (temp == null)
            {
                CLog.Error("错误,没有这个对象:" + path);
                return null;
            }
            return temp;
        }
        #endregion

        #region Callback
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
        }
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            Resources.UnloadUnusedAssets();
            GlobalMonoManager.RemoveAllNull();
            BundleCachers.ForEach(x => x.Value.RemoveNull());
            BaseGlobal.GCCollect();
        }
        protected override void OnSubBattleUnLoaded()
        {
            base.OnSubBattleUnLoaded();
        }
        public IEnumerator Load()
        {
            var dlcItemConfigs = GetDLCItemConfigs();

            foreach (var item in dlcItemConfigs)
            {
                DLCManager.LoadDLC(item);
                yield return new WaitForEndOfFrame();
            }

            //初始化的加载所有Bundle
            if (!BuildConfig.Ins.IsEditorMode)
            {
                if (BuildConfig.Ins.IsInitLoadSharedBundle)
                {
                    var data = DLCManager.LoadAllSharedBundle();
                    foreach (var bundle in data)
                    {
                        while (!bundle.IsDone)
                            yield return new WaitForEndOfFrame();
                    }
                }
                if (BuildConfig.Ins.IsInitLoadDirBundle)
                {
                    var data = DLCManager.LoadAllDirBundle();
                    foreach (var bundle in data)
                    {
                        while (!bundle.IsDone)
                            yield return new WaitForEndOfFrame();
                    }
                }
            }

            if(BuildConfig.Ins.IsWarmupAllShaders)
                Shader.WarmupAllShaders(); 
            yield break;

            // 获得DLC的根目录
            List<DLCItemConfig> GetDLCItemConfigs()
            {
                if (BuildConfig.Ins.IsEditorOrAssetBundleMode)
                {
                    return DLCConfig.ConfigAll;
                }
                else
                {
                    List<DLCItemConfig> ret = new List<DLCItemConfig>();
                    string[] files = Directory.GetFiles(SysConst.Path_StreamingAssets, SysConst.STR_DLCItem + ".json", SearchOption.AllDirectories);
                    foreach (var item in files)
                    {
                        var dlcJson = FileUtil.LoadJson<DLCItemConfig>(item);
                        if(dlcJson.IsActive)
                            ret.Add(dlcJson);
                    }
                    return ret;
                }
            }
        }
        public string GetLoadInfo()
        {
            return "Load Resources";
        }
        #endregion

        #region 语法糖
        public Material FontRendering =>BaseGlobal.RsMaterial.Get("FontRendering");
        public Material ImageGrey =>BaseGlobal.RsMaterial.Get("ImageGrey");
        #endregion
    }

}