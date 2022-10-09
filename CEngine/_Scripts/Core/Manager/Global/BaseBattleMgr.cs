using CYM.DLC;
using CYM.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CYM
{
    /// <summary>
    /// 战场管理器
    /// OnLoadBattleStart
    /// RandTip
    /// Callback_OnBattleLoad
    /// OnLoadSceneStart
    /// LoadScene
    /// SetActiveScene
    /// Callback_OnBattleLoadedScene
    /// BeforeLoadResources
    /// Callback_OnReadBattleDataStart
    /// StartReadGameData
    /// Callback_OnReadBattleDataEnd
    /// System.GC.Collect()
    /// Callback_OnBattleLoaded
    /// OnLoadBattleEnd
    /// Callback_OnGameStart
    /// BattleStart
    /// Callback_OnStartCustomBattleCoroutine
    /// CustomStartBattleCoroutine
    /// Callback_OnEndCustomBattleCoroutine
    /// Callback_OnGameStartOver
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class BaseBattleMgr<TData> : BaseGFlowMgr, IBattleMgr<TData>
        where TData : TDBaseBattleData, new()
    {
        #region Callback
        public event Callback Callback_OnStartNewGame;
        public event Callback Callback_OnCloseLoadingView;
        public event Callback Callback_OnGameStartOver;
        public event Callback Callback_OnBackToStart;
        public event Callback Callback_OnLoad;
        public event Callback Callback_OnLoaded;
        public event Callback Callback_OnLoadedScene;
        public event Callback Callback_OnReadDataEnd;
        public event Callback Callback_OnUnLoad;
        public event Callback Callback_OnUnLoaded;
        public event Callback Callback_OnGameStart;
        public event Callback Callback_OnGameStarted;
        public event Callback Callback_OnLoadStart;
        public event Callback<string, float> Callback_OnLoadingProgress;
        public event Callback Callback_OnStartCustomFlow;
        public event Callback Callback_OnEndCustomFlow;
        public event Callback Callback_OnRandTip;
        public event Callback Callback_OnInPauseLoadingView;
        #endregion

        #region misc
        // 当前游戏存档数据
        protected DBBaseGame CurBaseGameData => BaseGlobal.DBMgr.CurGameData;
        // 当前战场
        public TData CurData { get; private set; }
        private ITDConfig ITDConfig;
        #endregion

        #region prop
        // 场景的Bundle资源
        protected Asset SceneAsset;
        // 已经加载的场景的名城
        public string SceneName { get; private set; } = SysConst.STR_Inv;
        public Scene Scene { get; private set; }
        public Scene SceneStart => SceneManager.GetSceneByName(SysConst.SCE_Start);
        public Scene SceneSelf => SceneManager.GetSceneByName(SceneName);
        // 战场内游戏的时间
        public Timer PlayTimer { get; protected set; } = new Timer();
        // 已经加载的BattleID
        public string BattleID { get; private set; } = SysConst.STR_Inv;
        public int LoadBattleCount { get; private set; }
        #endregion

        #region is
        //是否为第一次加载
        public bool IsFirstLoad => LoadBattleCount == 1;
        //是否在自定义加载中
        public virtual bool IsInCustomLoading => false;
        // 是否在战场
        public bool IsInBattle => CurData != null;
        // 是否正在加载场景
        public bool IsLoadingBattle { get; private set; } = false;
        // 是否已经加载完毕战场,正式开始游戏
        public bool IsGameStartOver { get; protected set; } = false;
        //游戏开始
        public bool IsGameStart { get; protected set; } = false;
        // 是否加在战场结束
        public bool IsLoadBattleEnd { get; protected set; } = false;
        //是否加载完了场景
        public bool IsLoadedScene { get; protected set; } = false;
        // 进入加载界面暂停画面
        public bool IsInPauseLoadingView { get; protected set; } = false;
        // 锁定游戏开始流程
        public bool IsLockBattleStartFlow { get; private set; } = false;
        //是否在战场中加载存档
        public bool IsInReloading { get; private set; } = false;
        #endregion

        #region CoroutineMgr
        protected Corouter BattleCoroutine => BaseGlobal.BattleCorouter;
        protected Corouter CommonCoroutine => BaseGlobal.CommonCorouter;
        protected Corouter MainUICoroutine => BaseGlobal.MainUICorouter;
        #endregion

        #region mgr
        BaseAudioMgr AudioMgr => BaseGlobal.AudioMgr;
        IDBMgr<DBBaseGame> DBMgr => BaseGlobal.DBMgr;
        IPlotMgr PlotMgr => BaseGlobal.PlotMgr;
        #endregion

        #region 生命周期函数
        //是否需要暂停加载界面，等待玩家点击后再进入战场
        protected virtual bool NeedPauseLoading => false;
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        #endregion

        #region set
        // 解锁/锁定游戏开始流程
        public void LockBattleStartFlow(bool b)
        {
            IsLockBattleStartFlow = b;
        }
        public void UnPauseLoadingView()
        {
            IsInPauseLoadingView = false;
        }
        #endregion

        #region Load
        // 加载新游戏
        public virtual void StartNewGame(string battleId = "")
        {
            if (SubBattleMgr!=null && SubBattleMgr.IsInLevel)
            {
                CLog.Error("正在SubBattle中");
                return;
            }
            if (IsInBattle)
            {
                CLog.Error("正在游戏中");
                return;
            }

            TData tempData = ITDConfig.Get<TData>(battleId);
            if (tempData == null)
            {
                CLog.Error("没有这个战场:{0}", battleId);
                return;
            }

            BattleID = battleId;
            DBBaseGame data = DBMgr.StartNewGame();
            if (data == null)
            {
                CLog.Error("游戏存档为空");
                return;
            }
            data.GameNetMode = GameNetMode.PVE;
            data.GamePlayStateType = GamePlayStateType.NewGame;
            LoadScene(tempData);
            Callback_OnStartNewGame?.Invoke();
        }
        // 继续游戏
        public virtual void ContinueGame()
        {
            LoadGame(Prefers.GetLastAchiveID());
        }
        //开始教程
        public virtual void StartTutorial(string battleId = "")
        {
            StartNewGame(battleId);
            CurBaseGameData.GamePlayStateType = GamePlayStateType.Tutorial;
        }
        // 加载游戏
        public virtual void LoadGame(string dbKey)
        {
            if (SubBattleMgr != null && SubBattleMgr.IsInLevel)
            {
                CLog.Error("正在SubBattle中");
                return;
            }
            if (IsInBattle)
            {
                ReloadGame(dbKey);
                return;
            }

            DBMgr.Load(dbKey, GameConfig.Ins.DBLoadAsyn, (x, data) => {
                if (x)
                {
                    if (data == null)
                    {
                        CLog.Error("游戏存档为空");
                        return;
                    }
                    TData tempData = ITDConfig.Get<TData>(data.BattleID);
                    if (tempData == null)
                    {
                        CLog.Error("没有这个战场:{0}", data.BattleID);
                        return;
                    }
                    data.GamePlayStateType = GamePlayStateType.LoadGame;
                    LoadScene(tempData);
                }
            });
        }
        // 重载游戏
        protected void ReloadGame(string dbKey)
        {
            if (SubBattleMgr != null && SubBattleMgr.IsInLevel)
            {
                CLog.Error("正在SubBattle中");
                return;
            }
            if (!IsInBattle)
            {
                CLog.Error("不在游戏中");
                return;
            }
            IsInReloading = true;
            UnLoadBattle(() => LoadGame(dbKey));
        }
        public void GoToStart()
        {
            if (SubBattleMgr!=null && SubBattleMgr.IsInLevel)
            {
                CLog.Error("正在SubBattle中");
                return;
            }
            UnLoadBattle(() =>
            {
                SceneManager.SetActiveScene(SceneStart);
                Callback_OnBackToStart?.Invoke();
                MainUICoroutine.Run(BackToStart());
            });
        }
        // 加载战场
        public void LoadBattle(string tdid)
        {
            if (SubBattleMgr != null && SubBattleMgr.IsInLevel)
            {
                CLog.Error("正在SubBattle中");
                return;
            }
            CurBaseGameData.GamePlayStateType = GamePlayStateType.LoadGame;
            if (!IsInBattle)
            {
                TData data = ITDConfig.Get<TData>(tdid);
                LoadScene(data, false);
            }
            else
            {
                UnLoadBattle(() => LoadBattle(tdid));
            }
        }
        // 卸载战斗场景
        protected void UnLoadBattle(Callback onDone = null)
        {
            BattleCoroutine.Kill();
            MainUICoroutine.Run(_unLoadBattle(onDone));
        }
        // 加载场景
        protected void LoadScene(TData data, bool readData = true)
        {
            if (data == null) 
                return;
            MainUICoroutine.Kill();
            CurData = data.Copy<TData>();
            if (CurData != null)
            {
                BattleID = data.TDID;
                CurData.OnBeAdded(SelfBaseGlobal);
                BattleCoroutine.Run(_loadBattle(readData));
            }
            else
            {
                CLog.Error("Battle not found ！error id=" + data.TDID);
            }
        }
        #endregion

        #region phrase
        protected override void OnBattleLoadStart()
        {
            IsGameStartOver = false;
        }
        protected virtual void OnLoadSceneStart()
        {

        }
        #endregion

        #region enumator
        // 随机提示
        IEnumerator<float> _randTip()
        {
            while (IsLoadingBattle)
            {
                Callback_OnRandTip?.Invoke();
                yield return Timing.WaitForSeconds(10.0f);
            }
            yield break;
        }
        // 加载战场
        // readData:是否读取数据
        IEnumerator<float> _loadBattle(bool readData = true)
        {
            BaseGlobal.ResumeGame();
            BaseInputMgr.ResetFullScreenState();
            Callback_OnLoadStart?.Invoke();
            yield return Timing.WaitForOneFrame;
            float startTime = Time.realtimeSinceStartup;
            IsInReloading = false;
            IsLoadedScene = false;
            IsLoadingBattle = true;
            IsLoadBattleEnd = false;
            IsGameStartOver = false;
            IsGameStart = false;
            IsInPauseLoadingView = false;
            //开始Tip
            BattleCoroutine.Run(_randTip());
            //开始加载
            Callback_OnLoad?.Invoke();
            yield return Timing.WaitForOneFrame;
            OnLoadSceneStart();
            Callback_OnLoadingProgress?.Invoke(Util.GetStr("StartToLoad"), 0.0f);
            //演示几秒,给UI渐变的时间
            if (ULoadingView.Default != null)
            {
                while (!ULoadingView.Default.IsCompleteShow)
                    yield return Timing.WaitForOneFrame;
            }
            //加载场景
            SceneName = CurData.GetSceneName();
            if (CurData.UseAssetBundle)
            {
                SceneAsset = DLCManager.LoadScene(SceneName);
                while (!SceneAsset.IsDone)
                {
                    yield return Timing.WaitForOneFrame;
                    Callback_OnLoadingProgress?.Invoke(Util.GetStr("LoadingScene"), SceneAsset.Progress * 0.8f);
                }
            }
            else
            {
                var operation = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
                while (!operation.isDone)
                {
                    yield return Timing.WaitForOneFrame;
                    Callback_OnLoadingProgress?.Invoke(Util.GetStr("LoadingScene"), operation.progress * 0.8f);
                }
            }
            //延时一帧
            yield return Timing.WaitForOneFrame;
            Scene = SceneManager.GetSceneByName(SceneName);
            SceneManager.SetActiveScene(Scene);
            IsLoadedScene = true;
            Callback_OnLoadedScene?.Invoke();
            if (BaseSceneObject == null)
            {
                CLog.Error("错误，场景里没有SceneObject");
            }
            //这里必须延迟一帧,等待UI创建,注册事件
            yield return Timing.WaitForOneFrame;
            //读取数据前,资源加载
            yield return Timing.WaitUntilDone(BattleCoroutine.Run(BeforeReadData()));
            Callback_OnLoadingProgress?.Invoke(Util.GetStr("BeforeReadData"), 0.9f);
            if (readData)
            {
                BaseGlobal.IsUnReadData = false;
                //读取战场数据
                BaseGlobal.DBMgr.ReadGameDBData();
                BaseGlobal.IsUnReadData = true;
            }
            yield return Timing.WaitForOneFrame;
            Callback_OnReadDataEnd?.Invoke();
            //增加加载战场次数
            LoadBattleCount++;
            //读取数据后资源加载
            yield return Timing.WaitUntilDone(BattleCoroutine.Run(AffterReadData()));
            Callback_OnLoadingProgress?.Invoke(Util.GetStr("AffterReadData"), 0.95f);
            while(IsInCustomLoading) yield return Timing.WaitForOneFrame;
            BaseGlobal.ForceGCCollect();
            Callback_OnLoadingProgress?.Invoke(Util.GetStr("GC"), 1.0f);
            IsLoadingBattle = false;
            //场景加载结束
            Callback_OnLoaded?.Invoke();
            IsLoadBattleEnd = true;
            yield return Timing.WaitForOneFrame;
            //进入默认剧情入口
            BaseGlobal.PlotMgr?.Start(CurData.Plot);
            //游戏开始(预处理最后阶段)
            SelfBaseGlobal.OnGameStart1();
            SelfBaseGlobal.OnGameStart2();
            Callback_OnGameStart?.Invoke();
            yield return Timing.WaitForOneFrame;
            SelfBaseGlobal.OnGameStarted1();
            SelfBaseGlobal.OnGameStarted2();
            Callback_OnGameStarted?.Invoke();
            IsGameStart = true;
            //暂停在加载界面
            Callback_OnInPauseLoadingView?.Invoke();
            IsInPauseLoadingView = NeedPauseLoading;
            while (IsInPauseLoadingView) yield return Timing.WaitForOneFrame;
            SelfBaseGlobal.OnCloseLoadingView();
            Callback_OnCloseLoadingView?.Invoke();
            //锁定进入战场流程
            while (IsLockBattleStartFlow) yield return Timing.WaitForOneFrame;
            yield return Timing.WaitUntilDone(BattleCoroutine.Run(BattleStart()));
            //进入自定义流程的时候暂停
            Callback_OnStartCustomFlow?.Invoke();
            if (PlotMgr != null)
            {
                PlotMgr?.SetPlotPause(true);
                yield return Timing.WaitUntilDone(PlotMgr.CustomStartBattleCoroutine());
                PlotMgr?.SetPlotPause(false);
            }
            Callback_OnEndCustomFlow.Invoke();
            //结尾部分
            SelfBaseGlobal.OnGameStartOver();
            Callback_OnGameStartOver?.Invoke();
            IsGameStartOver = true;
        }
        // 卸载战场
        IEnumerator<float> _unLoadBattle(Callback onDone)
        {
            IsLoadedScene = false;
            IsLoadingBattle = false;
            IsLoadBattleEnd = false;
            IsGameStartOver = false;
            IsInPauseLoadingView = false;
            CurData.OnBeRemoved();
            CurData = null;
            //暂停一段时间
            BaseGlobal.PauseGame(true);
            BaseInputMgr.ResetFullScreenState();
            BaseGlobal.PlotMgr?.Stop();
            Callback_OnUnLoad?.Invoke();
            yield return Timing.WaitForOneFrame;
            Callback_OnLoadingProgress?.Invoke("Start to load", 0.01f);
            //延时一秒.防止UI卡住
            if (ULoadingView.Default != null)
            {
                while (!ULoadingView.Default.IsCompleteShow)
                    yield return Timing.WaitForOneFrame;
            }
            var wait = SceneManager.UnloadSceneAsync(SceneName);
            while (!wait.isDone)
            {
                yield return Timing.WaitForOneFrame;
                Callback_OnLoadingProgress?.Invoke("UnloadScene", wait.progress);
            }

            //卸载未使用的资源
            DLCManager.UnloadScene(SceneAsset);

            Callback_OnLoadingProgress?.Invoke("GC", 1.0f);
            BaseGlobal.ForceGCCollect();
            yield return Timing.WaitForOneFrame;
            Callback_OnUnLoaded?.Invoke();
            BaseGlobal.ResumeGame();
            onDone?.Invoke();
        }
        // 读取数据前加载资源
        protected virtual IEnumerator<float> BeforeReadData()
        {
            yield return Timing.WaitForOneFrame;
        }
        // 读取数据后加载资源
        protected virtual IEnumerator<float> AffterReadData()
        {
            yield return Timing.WaitForOneFrame;
        }
        // 关卡开始
        protected virtual IEnumerator<float> BattleStart()
        {
            yield return Timing.WaitForOneFrame;
        }
        // 回到初始场景
        protected virtual IEnumerator<float> BackToStart()
        {
            yield return Timing.WaitForOneFrame;
        }
        #endregion

        #region Callback
        public override void OnGameStart1()
        {
            base.OnGameStart1();
        }
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
            AudioMgr?.EnableSFX(false);
        }
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            PlayTimer.Restart();
            AudioMgr?.EnableSFX(true);
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            PlayTimer.Pause();
            foreach (var item in BaseGlobal.ClearWhenBattleUnload)
                item.Clear();
        }
        #endregion

        #region DB
        public override void OnRead1(DBBaseGame data)
        {
            base.OnRead1(data);
            LoadBattleCount = data.LoadBattleCount;
        }
        public override void OnWrite(DBBaseGame data)
        {
            base.OnWrite(data);
            data.BattleID = CurData.TDID;
            data.LoadBattleCount = LoadBattleCount;
        }
        #endregion
    }

}