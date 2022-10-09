//------------------------------------------------------------------------------
// BaseGlobalCoreMgr.cs
// Copyright 2018 2018/4/24 
// Created by CYM on 2018/4/24
// Owner: CYM
// 好汉游戏流程触发事件的全局管理器
//------------------------------------------------------------------------------

using UnityEngine;

namespace CYM
{
    public class BaseGFlowMgr : BaseMgr
    {
        #region prop
        protected IBattleMgr<TDBaseBattleData> BattleMgr => BaseGlobal.BattleMgr;
        protected ILevelMgr<TDBaseLevelData> SubBattleMgr => BaseGlobal.LevelMgr;
        protected BaseLuaMgr LuaMgr => BaseGlobal.LuaMgr;
        protected BaseLoaderMgr LoaderMgr => BaseGlobal.LoaderMgr;
        protected GameObject ResourceObj;
        protected GameObject SystemObj;
        #endregion

        #region life
        //适用于游戏启动后创建的对象(放在Resources目录下)
        protected virtual string ResourcePrefabKey => SysConst.STR_Inv;
        //适用于全部加载完成后创建的系统对象(放在Bundle/System目录下)
        protected virtual string SystemPrefab => SysConst.STR_Inv;
        public sealed override MgrType MgrType => MgrType.Global;
        public override void OnCreate()
        {
            base.OnCreate();
            if (!ResourcePrefabKey.IsInv())
                ResourceObj = BaseGlobal.GRMgr.GetResources(ResourcePrefabKey, true);
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (BattleMgr != null)
            {
                BattleMgr.Callback_OnLoadStart += OnBattleLoadStart;
                BattleMgr.Callback_OnStartNewGame += OnStartNewGame;
                BattleMgr.Callback_OnBackToStart += OnBackToStart;
                BattleMgr.Callback_OnLoad += OnBattleLoad;
                BattleMgr.Callback_OnLoaded += OnBattleLoaded;
                BattleMgr.Callback_OnLoadedScene += OnBattleLoadedScene;
                BattleMgr.Callback_OnReadDataEnd += OnBattleReadDataEnd;
                BattleMgr.Callback_OnUnLoad += OnBattleUnLoad;
                BattleMgr.Callback_OnUnLoaded += OnBattleUnLoaded;
                BattleMgr.Callback_OnLoadingProgress += OnLoadingProgressChanged;
                BattleMgr.Callback_OnStartCustomFlow += OnStartCustomBattleCoroutine;
                BattleMgr.Callback_OnEndCustomFlow += OnEndCustomBattleCoroutine;
            }

            if (SubBattleMgr != null)
            {
                SubBattleMgr.Callback_OnLoad += OnSubBattleLoad;
                SubBattleMgr.Callback_OnLoaded += OnSubBattleLoaded;
                SubBattleMgr.Callback_OnUnLoad += OnSubBattleUnLoad;
                SubBattleMgr.Callback_OnUnLoaded += OnSubBattleUnLoaded;
                SubBattleMgr.Callback_OnLoadedScene += OnSubBattleLoadedScene;
            }

            if (LoaderMgr != null)
            {
                LoaderMgr.Callback_OnStartLoad += OnStartLoad;
                LoaderMgr.Callback_OnLoadEnd += OnLoadEnd;
                LoaderMgr.Callback_OnAllLoadEnd1 += OnAllLoadEnd1;
                LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
            }

            if (LuaMgr != null)
            {
                LuaMgr.Callback_OnParseStart += OnLuaParseStart;
                LuaMgr.Callback_OnParseEnd += OnLuaParseEnd;
            }
        }

        public override void OnDisable()
        {
            if (BattleMgr != null)
            {
                BattleMgr.Callback_OnLoadStart -= OnBattleLoadStart;
                BattleMgr.Callback_OnStartNewGame -= OnStartNewGame;
                BattleMgr.Callback_OnBackToStart -= OnBackToStart;
                BattleMgr.Callback_OnLoad -= OnBattleLoad;
                BattleMgr.Callback_OnLoaded -= OnBattleLoaded;
                BattleMgr.Callback_OnLoadedScene -= OnBattleLoadedScene;
                BattleMgr.Callback_OnReadDataEnd -= OnBattleReadDataEnd;
                BattleMgr.Callback_OnUnLoad -= OnBattleUnLoad;
                BattleMgr.Callback_OnUnLoaded -= OnBattleUnLoaded;
                BattleMgr.Callback_OnLoadingProgress -= OnLoadingProgressChanged;
                BattleMgr.Callback_OnStartCustomFlow -= OnStartCustomBattleCoroutine;
                BattleMgr.Callback_OnEndCustomFlow -= OnEndCustomBattleCoroutine;
            }

            if (SubBattleMgr != null)
            {
                SubBattleMgr.Callback_OnLoad -= OnSubBattleLoad;
                SubBattleMgr.Callback_OnLoaded -= OnSubBattleLoaded;
                SubBattleMgr.Callback_OnUnLoad -= OnSubBattleUnLoad;
                SubBattleMgr.Callback_OnUnLoaded -= OnSubBattleUnLoaded;
                SubBattleMgr.Callback_OnLoadedScene -= OnSubBattleLoadedScene;
            }

            if (LoaderMgr != null)
            {
                LoaderMgr.Callback_OnStartLoad -= OnStartLoad;
                LoaderMgr.Callback_OnLoadEnd -= OnLoadEnd;
                LoaderMgr.Callback_OnAllLoadEnd1 -= OnAllLoadEnd1;
                LoaderMgr.Callback_OnAllLoadEnd2 -= OnAllLoadEnd2;
            }

            if (LuaMgr != null)
            {
                LuaMgr.Callback_OnParseStart -= OnLuaParseStart;
                LuaMgr.Callback_OnParseEnd -= OnLuaParseEnd;
            }
            base.OnDisable();
        }
        #endregion

        #region Sub Battle
        protected virtual void OnSubBattleUnLoad()
        {

        }

        protected virtual void OnSubBattleLoad()
        {

        }
        protected virtual void OnSubBattleUnLoaded()
        {

        }

        protected virtual void OnSubBattleLoaded()
        {

        }
        protected virtual void OnSubBattleLoadedScene()
        {
        }
        #endregion

        #region Callback
        protected virtual void OnBattleLoadStart()
        {
        }
        protected virtual void OnStartNewGame() { }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
        }
        protected virtual void OnBackToStart() { }
        protected virtual void OnBattleLoad() { }
        protected virtual void OnBattleLoaded() { }
        protected virtual void OnBattleLoadedScene() { }
        protected virtual void OnBattleReadDataEnd() { }
        protected virtual void OnBattleUnLoad() { }
        protected virtual void OnBattleUnLoaded() { }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
        }
        protected virtual void OnLoadingProgressChanged(string info, float val) { }
        protected virtual void OnStartLoad() { }
        protected virtual void OnLoadEnd(LoadEndType type, string info) { }
        protected virtual void OnAllLoadEnd1() { }
        protected virtual void OnAllLoadEnd2()
        {
            if (!SystemPrefab.IsInv())
                SystemObj = BaseGlobal.RsPrefab.Get(SystemPrefab, true);
        }
        protected virtual void OnLuaParseStart() { }
        protected virtual void OnLuaParseEnd() { }
        protected virtual void OnStartCustomBattleCoroutine() { }
        protected virtual void OnEndCustomBattleCoroutine() { }
        #endregion
    }
}