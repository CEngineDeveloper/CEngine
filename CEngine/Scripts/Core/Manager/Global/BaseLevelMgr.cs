//------------------------------------------------------------------------------
// BaseSubBattleMgr.cs
// Copyright 2019 2019/12/10 
// Created by CYM on 2019/12/10
// Owner: CYM
// Level = 小型关卡，比Battle = 大地图战场
//------------------------------------------------------------------------------

using CYM.DLC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYM
{
    public class BaseLevelMgr<TData> : BaseMgr, ILevelMgr<TData>
        where TData : TDBaseLevelData, new()
    {
        #region state
        public bool IsInLevel => CurData != null;
        public string LevelID { get; private set; } = "";
        public string SceneName { get; private set; } = "";
        public string LastLoadedSceneName { get; private set; } = null;
        public bool IsLoadingLevel { get; private set; } = false;
        public bool IsStartLevel { get; private set; } = false;
        public bool IsLoadLevelEnd { get; private set; } = false;
        #endregion

        #region prop
        // 场景的Bundle资源
        protected Asset SceneAsset;
        protected Asset ScenePrefabAsset;
        protected GameObject ScenePrefabIns;
        protected Corouter LevelCoroutine => BaseGlobal.LevelCorouter;
        protected Corouter BattleCoroutine => BaseGlobal.BattleCorouter;
        protected BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        #endregion

        #region Callback Val
        public event Callback Callback_OnLoad;
        public event Callback Callback_OnLoaded;
        public event Callback Callback_OnLoadedScene;
        public event Callback Callback_OnUnLoad;
        public event Callback Callback_OnUnLoaded;
        public event Callback Callback_OnLoadStart;
        public event Callback Callback_OnGameStart;
        public event Callback<string, float> Callback_OnLoadingProgress;
        public event Callback Callback_OnRandTip;
        #endregion

        #region Table
        ITDConfig ITDConfig;
        public TData CurData { get; private set; }
        #endregion

        #region life
        //是否使用Prefab Scene
        public virtual LevelLoadType LoadType => LevelLoadType.Scene;
        public override void OnCreate()
        {
            base.OnCreate();
            Callback_OnLoad += OnLevelLoad;
            Callback_OnLoaded += OnLevelLoaded;
            Callback_OnLoadedScene += OnLevelLoadedScene;
            Callback_OnUnLoad += OnLevelUnLoad;
            Callback_OnUnLoaded += OnLevelUnLoaded;
            Callback_OnLoadStart += OnLevelLoadStart;
            Callback_OnGameStart += OnLevelGameStart;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        #endregion

        #region set
        public virtual void Load(string battleID = "")
        {
            if (IsInLevel)
            {
                CLog.Error("正处于SubBattle中");
                return;
            }
            TData tempData = ITDConfig.Get<TData>(battleID);
            if (tempData == null)
            {
                CLog.Error("没有这个战场:{0}", battleID);
                return;
            }
            LevelCoroutine.Kill();
            CurData = tempData.Copy<TData>();
            if (CurData != null)
            {
                LevelID = tempData.TDID;
                CurData.OnBeAdded(SelfBaseGlobal);
                BattleCoroutine.Run(_LoadBattle());
            }
            else
            {
                CLog.Error("Battle not found ！error id=" + battleID);
            }
        }
        public virtual void UnLoad()
        {
            if (!IsInLevel)
            {
                CLog.Error("没有加载SubBattle");
                return;
            }
            LevelCoroutine.Kill();
            BattleCoroutine.Run(_UnLoadBattle(() =>
            {
                SceneManager.SetActiveScene(BaseGlobal.BattleMgr.SceneSelf);
                BattleCoroutine.Run(BackToBattle());
            }));
        }
        #endregion

        #region enumator
        IEnumerator<float> _RandTip()
        {
            while (IsLoadingLevel)
            {
                Callback_OnRandTip?.Invoke();
                yield return Timing.WaitForSeconds(10.0f);
            }
            yield break;
        }
        // 加载战场
        // readData:是否读取数据
        IEnumerator<float> _LoadBattle()
        {
            yield return Timing.WaitForOneFrame;
            Callback_OnLoadStart?.Invoke();
            float startTime = Time.realtimeSinceStartup;
            IsLoadingLevel = true;
            IsLoadLevelEnd = false;
            IsStartLevel = false;
            //开始Tip
            LevelCoroutine.Run(_RandTip());
            //开始加载
            Callback_OnLoad?.Invoke();
            yield return Timing.WaitForOneFrame;
            OnLoadSceneStart();
            Callback_OnLoadingProgress?.Invoke("开始加载", 0.0f);
            //演示几秒,给UI渐变的时间
            yield return Timing.WaitForSeconds(0.5f);
            //加载场景
            SceneName = CurData.GetRawSceneName();
            if (LoadType == LevelLoadType.Prefab)
            {
                if (LastLoadedSceneName != SceneName)
                {
                    if(ScenePrefabIns) GameObject.Destroy(ScenePrefabIns);
                    if(ScenePrefabAsset!=null) DLCManager.UnloadAsset(ScenePrefabAsset, false);
                    ScenePrefabAsset = DLCManager.LoadAssetAsync<GameObject>(SysConst.BN_System, SceneName);
                    while (!ScenePrefabAsset.IsDone)
                    {
                        yield return Timing.WaitForOneFrame;
                        Callback_OnLoadingProgress?.Invoke("加载场景", ScenePrefabAsset.Progress * 0.8f);
                    }
                    ScenePrefabIns = GameObject.Instantiate(ScenePrefabAsset.Object as GameObject);
                    LastLoadedSceneName = SceneName;
                }
                ScenePrefabIns?.SetActive(true);
            }
            else
            {
                SceneAsset = DLCManager.LoadScene(SceneName);
                while (!SceneAsset.IsDone)
                {
                    yield return Timing.WaitForOneFrame;
                    Callback_OnLoadingProgress?.Invoke("加载场景", SceneAsset.Progress * 0.8f);
                }
                //延时一帧
                yield return Timing.WaitForOneFrame;
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneName));
            }
            Callback_OnLoadedScene?.Invoke();
            //这里必须延迟一帧,等待UI创建,注册事件
            yield return Timing.WaitForOneFrame;
            BaseGlobal.GCCollect();
            Callback_OnLoadingProgress?.Invoke("清理内存", 1.0f);
            IsLoadingLevel = false;
            //场景加载结束
            Callback_OnLoaded?.Invoke();
            IsLoadLevelEnd = true;
            IsStartLevel = true;
            Callback_OnGameStart?.Invoke();
        }

        // 卸载战场
        IEnumerator<float> _UnLoadBattle(Callback onDone)
        {
            if (CurData == null) yield break;
            IsLoadingLevel = false;
            IsLoadLevelEnd = false;
            IsStartLevel = false;

            yield return Timing.WaitForOneFrame;
            Callback_OnUnLoad?.Invoke();
            yield return Timing.WaitForOneFrame;
            Callback_OnLoadingProgress?.Invoke("Start to load", 0.01f);
            //延时一秒.防止UI卡住
            yield return Timing.WaitForSeconds(0.5f);
            if (LoadType == LevelLoadType.Prefab)
            {
                ScenePrefabIns?.SetActive(false);
                Callback_OnLoadingProgress?.Invoke("UnloadScene", 1.0f);
            }
            else
            {
                var wait = SceneManager.UnloadSceneAsync(SceneName);
                while (!wait.isDone)
                {
                    yield return Timing.WaitForOneFrame;
                    Callback_OnLoadingProgress?.Invoke("UnloadScene", wait.progress);
                }
                DLCManager.UnloadScene(SceneAsset);
            }

            CurData.OnBeRemoved();
            CurData = null;
            Callback_OnLoadingProgress?.Invoke("GC", 1.0f);
            BaseGlobal.ResumeGame();
            yield return Timing.WaitForOneFrame;
            Callback_OnUnLoaded?.Invoke();
            onDone?.Invoke();
        }
        protected virtual IEnumerator<float> BackToBattle()
        {
            yield return Timing.WaitForOneFrame;
        }
        #endregion

        #region Callback
        protected virtual void OnLoadSceneStart() { }
        protected virtual void OnLevelLoad() { }
        protected virtual void OnLevelLoaded() { }
        protected virtual void OnLevelLoadedScene() { }
        protected virtual void OnLevelUnLoad() 
        {
            foreach (var item in BaseGlobal.ClearWhenLevelUnload)
                item.Clear();
        }
        protected virtual void OnLevelUnLoaded() { }
        public virtual void OnLevelGameStart() { }
        protected virtual void OnLevelLoadStart() { }
        #endregion
    }
}