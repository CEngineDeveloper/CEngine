//------------------------------------------------------------------------------
// BaseFlow.cs
// Copyright 2023 2023/1/1 
// Created by CYM on 2023/1/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class BaseFlow 
    {
        static List<BaseFlow> AllFlows = new List<BaseFlow>();

        IBattleMgr<TDBaseBattleData> BattleMgr => BaseGlobal.BattleMgr;
        IDBMgr<DBBaseGame> DBMgr => BaseGlobal.DBMgr;

        protected virtual string FlowName=>"None";
        public bool IsNewGame => DBMgr.CurGameData.IsNewGame();
        public bool IsLoadGame => DBMgr.CurGameData.IsLoadGame();

        #region life
        public static void InitAllFlow()
        {
            foreach (var item in AllFlows)
            {
                item.Init();
            }
        }
        public BaseFlow()
        {
            AllFlows.Add(this);
        }
        void Init()
        {
            CLog.Green($"注册流程：{FlowName}");
            OnInitFlow();
            BattleMgr.Callback_OnStartNewGame+= OnStartNewGame;
            BattleMgr.Callback_OnCloseLoadingView+= OnCloseLoadingView;
            BattleMgr.Callback_OnBackToStart+= OnBackToStart;
            BattleMgr.Callback_OnLoad+= OnLoad;
            BattleMgr.Callback_OnLoaded+= OnLoaded;
            BattleMgr.Callback_OnLoadedScene+= OnLoadedScene;
            BattleMgr.Callback_OnReadDataEnd+= OnReadDataEnd;
            BattleMgr.Callback_OnUnLoad+= OnUnLoad;
            BattleMgr.Callback_OnUnLoaded+= OnUnLoaded;
            BattleMgr.Callback_OnGameStart+= OnGameStart;
            BattleMgr.Callback_OnGameStarted+= OnGameStarted;
            BattleMgr.Callback_OnGameStartOver+= OnGameStartOver;
            BattleMgr.Callback_OnLoadStart+= OnLoadStart;
            BattleMgr.Callback_OnLoadingProgress+= OnLoadingProgress;
            BattleMgr.Callback_OnStartCustomFlow+= OnStartCustomFlow;
            BattleMgr.Callback_OnEndCustomFlow+= OnEndCustomFlow;
            BattleMgr.Callback_OnRandTip+= OnRandTip;
            BattleMgr.Callback_OnInPauseLoadingView+= OnInPauseLoadingView;

            DBMgr.Callback_OnGenerateNewGameData += OnGenerateNewGameData;
            DBMgr.Callback_OnModifyGameData += OnModifyGameData;
            DBMgr.Callback_OnReadGameData += OnReadGameData;
            DBMgr.Callback_OnReadGameDataStart += OnReadGameDataStart;
            DBMgr.Callback_OnReadGameDataEnd += OnReadGameDataEnd;
            DBMgr.Callback_OnWriteGameData += OnWriteGameData;

            DBMgr.Callback_OnRead1+= OnRead1;
            DBMgr.Callback_OnRead2 += OnRead2;
            DBMgr.Callback_OnRead3 += OnRead3;
            DBMgr.Callback_OnReadEnd += OnReadEnd;
        }
        #endregion

        #region Common
        protected virtual void OnInitFlow()
        { 
        
        }
        #endregion

        #region DB
        protected virtual void OnGenerateNewGameData(DBBaseGame arg1)
        {

        }
        protected virtual void OnWriteGameData(DBBaseGame arg1)
        {

        }
        protected virtual void OnReadGameDataEnd(DBBaseGame arg1)
        {

        }
        protected virtual void OnReadGameDataStart(DBBaseGame arg1)
        {

        }
        protected virtual void OnReadGameData(DBBaseGame arg1)
        {

        }
        protected virtual void OnModifyGameData(DBBaseGame arg1)
        {

        }
        protected virtual void OnReadEnd(DBBaseGame arg1)
        {

        }

        protected virtual void OnRead3(DBBaseGame arg1)
        {

        }

        protected virtual void OnRead2(DBBaseGame arg1)
        {

        }

        protected virtual void OnRead1(DBBaseGame arg1)
        {

        }
        #endregion

        #region Callback Battle
        protected virtual void OnInPauseLoadingView()
        {

        }

        protected virtual void OnRandTip()
        {

        }

        protected virtual void OnEndCustomFlow()
        {

        }

        protected virtual void OnStartCustomFlow()
        {

        }

        protected virtual void OnLoadingProgress(string arg1, float arg2)
        {
        }

        protected virtual void OnLoadStart()
        {

        }

        protected virtual void OnGameStartOver()
        {
        }

        protected virtual void OnGameStarted()
        {
        }

        protected virtual void OnGameStart()
        {
        }

        protected virtual void OnUnLoaded()
        {
        }

        protected virtual void OnUnLoad()
        {
        }

        protected virtual void OnReadDataEnd()
        {
        }

        protected virtual void OnLoadedScene()
        {
        }

        protected virtual void OnLoaded()
        {
        }

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnBackToStart()
        {
        }

        protected virtual void OnCloseLoadingView()
        {
        }

        protected virtual void OnStartNewGame()
        {
        }
        #endregion
    }
}