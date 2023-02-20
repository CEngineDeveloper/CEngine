//**********************************************
// Class Name	: CYMBase
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CYM.Excel;
using CYM.UI;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.ComponentModel;
using UnityEngine.Rendering;

namespace CYM
{
    #region UI
    public interface IUIDirty
    {
        bool IsDirtyShow { get; }
        bool IsDirtyData { get; }
        bool IsDirtyCell { get; }
        bool IsDirtyRefresh { get; }

        void SetDirtyAll(float delay);
        void SetDirtyAll();
        void SetDirtyShow();
        void SetDirtyData();
        void SetDirtyRefresh();
        void SetDirtyCell();

        void RefreshAll();
        void RefreshShow();
        void RefreshCell();
        void RefreshData();
        void Refresh();

        void OnFixedUpdate();
    }
    public interface ICheckBoxContainer
    {
        bool GetIsToggleGroup();
        bool GetIsAutoSelect();
        void SelectItem(UControl arg1);
    }
    #endregion

    #region battle
    public interface IBattleMgr<out TDataOut>
    {
        #region prop
        TDataOut CurData { get; }
        Scene Scene { get; }
        Scene SceneStart { get; }
        Scene SceneSelf { get; }
        string SceneName { get; }
        string BattleID { get; }
        Timer PlayTimer { get; }
        int LoadBattleCount { get; }
        #endregion

        #region set
        void StartNewGame(string battleId = "");
        void StartTutorial(string battleId = "");
        void ContinueGame();
        void LoadGame(string dbKey);
        void GoToStart();
        void LoadBattle(string tdid);
        void LockBattleStartFlow(bool b);
        void UnPauseLoadingView();
        #endregion

        #region is
        bool IsInPauseLoadingView { get; }
        bool IsInBattle { get; }
        bool IsLoadingBattle { get; }
        bool IsGameStartOver { get; }
        bool IsLoadBattleEnd { get; }
        bool IsLockBattleStartFlow { get; }
        bool IsLoadedScene { get; }
        bool IsFirstLoad { get; }
        bool IsInCustomLoading { get; }
        bool IsInReloading { get; }
        #endregion

        #region Callback
        event Callback Callback_OnStartNewGame;
        event Callback Callback_OnCloseLoadingView;
        event Callback Callback_OnBackToStart;
        event Callback Callback_OnLoad;
        event Callback Callback_OnLoaded;
        event Callback Callback_OnLoadedScene;
        event Callback Callback_OnReadDataEnd;
        event Callback Callback_OnUnLoad;
        event Callback Callback_OnUnLoaded;
        event Callback Callback_OnGameStart;
        event Callback Callback_OnGameStarted;
        event Callback Callback_OnGameStartOver;
        event Callback Callback_OnLoadStart;
        event Callback<string, float> Callback_OnLoadingProgress;
        event Callback Callback_OnStartCustomFlow;
        event Callback Callback_OnEndCustomFlow;
        event Callback Callback_OnRandTip;
        event Callback Callback_OnInPauseLoadingView;
        #endregion
    }
    public interface ILevelMgr<out TDataOut>
    {
        #region prop
        TDataOut CurData { get; }
        #endregion

        #region set
        void Load(string battleID = "");
        void UnLoad();
        #endregion

        #region is
        bool IsInLevel { get; }
        string SceneName { get; }
        string LevelID { get; }
        bool IsLoadingLevel { get; }
        bool IsLoadLevelEnd { get; }
        #endregion

        #region Callback
        event Callback Callback_OnLoad;
        event Callback Callback_OnLoaded;
        event Callback Callback_OnLoadedScene;
        event Callback Callback_OnUnLoad;
        event Callback Callback_OnUnLoaded;
        event Callback Callback_OnLoadStart;
        event Callback Callback_OnGameStart;
        event Callback<string, float> Callback_OnLoadingProgress;
        event Callback Callback_OnRandTip;
        #endregion
    }
    #endregion

    #region DB Convert
    public interface IDBListConverMgr<T> where T : DBBase
    {
        void LoadDBData(ref List<T> data);
        void SaveDBData(ref List<T> data);
    }
    public interface IDBConverMgr<T> where T : DBBase
    {
        void LoadDBData(ref T data);
        void SaveDBData(ref T data);
    }
    public interface IDBSingleConverMgr<TData>
        where TData : TDBaseData
    {
        void LoadDBData<TDBData>(ref TDBData dbData, Callback<TData, TDBData> action) where TDBData : DBBase, new();
        void SaveDBData<TDBData>(ref TDBData dbData, Callback<TData, TDBData> action) where TDBData : DBBase, new();
    }
    public interface IDBListConvertMgr<TData>
        where TData : TDBaseData
    {
        void LoadDBData<TDBData>(ref List<TDBData> dbData, Callback<TData, TDBData> action) where TDBData : DBBase, new();
        void SaveDBData<TDBData>(ref List<TDBData> dbData, Callback<TData, TDBData> action) where TDBData : DBBase, new();
    }
    public interface IDBDicConvertMgr<TData>
    where TData : TDBaseData
    {
        void LoadDBData<TDBData>(ref Dictionary<int,TDBData> dbData, Callback<int,TData, TDBData> action) where TDBData : DBBase, new();
        void SaveDBData<TDBData>(ref Dictionary<int,TDBData> dbData, Callback<int,TData, TDBData> action) where TDBData : DBBase, new();
    }
    #endregion

    #region DB
    public interface IDBMono
    {
        void OnRead1(DBBaseGame data);
        void OnRead2(DBBaseGame data);
        void OnRead3(DBBaseGame data);
        void OnReadEnd(DBBaseGame data);

        void OnWrite(DBBaseGame data);
    }
    public interface IArchiveFile
    {
        string Name { get; }
        DateTime SaveTime { get; }
        bool IsBroken { get; }
        ArchiveHeader Header { get; }
        DateTime FileTime { get; }
        // 未损坏且版本为最新
        // 则认为可以读取
        bool IsLoadble { get; }
        // 存档版本是否兼容
        bool IsCompatible { get; }
        TimeSpan PlayTime { get; }
        DBBaseGame BaseGameDatas { get; }
    }
    public interface IArchiveMgr
    {
        List<IArchiveFile> GetAllBaseArchives(bool isRefresh = false);

        // 存档是否可以载入
        bool IsArchiveValid(string id);
        // 是否存在相同的存档
        bool IsHaveArchive(string ID);
        bool IsHaveArchive();
    }
    #endregion

    public interface ITDConfig
    {
        Type DataType { get; }
        void OnLuaParseStart();
        void OnLuaParseEnd();
        void OnExcelParseStart();
        void OnExcelParseEnd();
        void OnAllLoadEnd1();
        void OnAllLoadEnd2();
        Dictionary<string, TDBaseData> BaseDatas { get; }
        T Get<T>(string key) where T : TDBaseData;
        IList GetRawGroup(string group);
        bool Contains(string key);
        List<object> ListObjValues { get; }
        List<string> ListKeys { get; }
        TableMapper TableMapper { get;}
        void AddAlterRangeFromObj(IEnumerable<object> data);
    }
    public interface IUnitSpawnMgr<out TUnitOut>
    {
        IList IListData { get; }
        //是否添加到全局单位管理器中
        bool IsAddToGlobalSpawnerMgr { get; }
        //是否作为玩家管理器
        bool IsPlayerSpawnMgr { get; }
        Type UnitType { get; }
        TUnitOut GetUnit(long rtid);
        TUnitOut GetUnit(string tdid);
        void Despawn(BaseUnit data,float delay=0);
        bool IsHave(BaseUnit unit);
    }
    public interface ITDSpawnMgr<out TDataOut>
    {
        bool IsAddToGlobalSpawnerMgr { get; }
        Type UnitType { get; }
        TDataOut GetUnit(long rtid);
        TDataOut GetUnit(string tdid);
        void Despawn(TDBaseData data, float delay = 0);
        bool IsHave(TDBaseData unit);
    }
    public interface IPlotMgr
    {
        #region set
        int PushIndex(int? index=null);
        bool Start(string id,int? index=null);
        void RunTemp(IEnumerator<float> enumerator, string flag = null);
        void RunMain();
        void Stop();
        void EnableAI(bool b);
        void SetPlotPause(bool b, int type = 0);
        #endregion

        #region get
        CoroutineHandle CustomStartBattleCoroutine();
        #endregion

        #region is
        bool IsInPlotPause();
        bool IsInPlot();
        bool IsInPlot(params string[] tdid);
        bool IsEnableAI { get;}
        int CurPlotIndex { get; }
        #endregion

        #region ghost
        void AddToGhostSelUnits(params BaseUnit[] unit);
        void AddToGhostMoveUnits(params BaseUnit[] unit);
        void RemoveFromGhostMoveUnits(params BaseUnit[] unit);
        void AddToGhostAIUnits(params BaseUnit[] unit);
        void RemoveFromGhostSelUnits(params BaseUnit[] unit);
        void RemoveFromGhostAIUnits(params BaseUnit[] unit);
        void AddToGhostAnimUnits(params BaseUnit[] unit);
        void RemoveFromGhostAnimUnits(params BaseUnit[] unit);
        bool IsGhostSel(BaseUnit unit);
        bool IsGhostMove(BaseUnit unit);
        bool IsGhostAI(BaseUnit unit);
        bool IsGhostAnim(BaseUnit unit);
        #endregion

        #region blocker ui
        void BlockClick(bool b);
        void AddIgnoreBlockClickOnce(UControl control);
        void RemIgnoreBlockClickOnce(UControl control);
        bool IsIgnoreBlockClickOnce(UControl control);
        void AddIgnoreBlockClick(UControl control);
        void RemIgnoreBlockClick(UControl control);
        bool IsInIgnoreBlockClick(UControl control);
        bool IsInIgnoreBlockClickView(UView view);
        bool IsBlockClick { get; }
        #endregion

        #region blocker unit
        void BlockSelectUnit(bool b);
        bool IsBlockerUnit(BaseUnit unit);
        #endregion
    }
    public interface IDBMgr<out TDataOut>
    {
        #region Callback
        event Callback<bool> Callback_OnSaveState;
        event Callback<bool, DBBaseGame> Callback_OnLoadState;
        event Callback<DBBaseGame> Callback_OnGenerateNewGameData;
        event Callback<DBBaseGame> Callback_OnModifyGameData;
        event Callback<DBBaseGame> Callback_OnReadGameData;
        event Callback<DBBaseGame> Callback_OnReadGameDataStart;
        event Callback<DBBaseGame> Callback_OnReadGameDataEnd;
        event Callback<DBBaseGame> Callback_OnWriteGameData;
        event Callback<DBBaseGame> Callback_OnRead1;
        event Callback<DBBaseGame> Callback_OnRead2;
        event Callback<DBBaseGame> Callback_OnRead3;
        event Callback<DBBaseGame> Callback_OnReadEnd;
        #endregion

        #region save and load
        void Load(string ID, bool isAsyn, Callback<bool, DBBaseGame> callback);
        void SaveAs(string ID, bool isSnapshot = false, bool isAsyn = true, bool isDirtyList = true, bool isHide = false);
        void AutoSave(bool isSnapshot = false, bool isForce = false);
        void SaveTemp(bool useSnapshot = false,bool isAsyn = false);
        #endregion

        #region set
        TDataOut CurGameData { get; }
        TDataOut StartNewGame();
        TDataOut Snapshot(bool isSnapshot = true);
        void DeleteArchives(string ID);
        void ReadGameDBData();
        void WriteGameDBData();
        #endregion

        #region get
        IArchiveMgr GetAchieveMgr();
        string GetDefaultSaveName();
        string GetTempSavePath();
        #endregion

        #region is
        // 是否存在当前的存档
        bool IsHaveSameArchives(string ID);
        // 是否有游戏数据
        bool IsHaveGameData();
        // 是否可以继续游戏
        bool IsCanContinueGame();
        // 是否存储游戏中
        bool IsHolding { get; }
        #endregion
    }
    public interface ISettingsMgr<out TDataOut>
    {
        #region get
        TDataOut Settings { get; }
        string[] GetResolutionStrs();
        #endregion

        #region set
        void Revert();
        void Save();
        void SetResolution(int index);
        void SetWindowType(WindowType type);
        void SetQuality(int index);
        void SetTerrainAccuracy(bool b);
        void RefreshScreenSettings();
        #endregion
    }
    public interface IDiffMgr<out TDataOut>
    {
        #region set
        void SetDiffType(GameDiffType type);
        void SetGMMod(bool b);
        void SetAnalytics(bool b);
        void SetHavePlot(bool b);
        #endregion

        #region get
        GameDiffType GetDiffType();
        bool IsGMMod { get;  }
        TDataOut Setting { get; }
        #endregion

        #region is
        bool IsAnalytics();
        bool IsGMMode();
        bool IsSettedGMMod();
        bool IsHavePlot();
        #endregion
    }
    public interface IScreenMgr<out TUnitOut>
    {
        TUnitOut TempPlayer { get; }
        TUnitOut Player { get; }
        TUnitOut PrePlayer { get; }

        string SelectedCharaTDID { get; }
        void SelectChara(TDBaseData data);
        void SelectChara(string tdid);
        void SetCurInputSelectPlayer();

        string SelectedDrama { get; }
        void SelectDrama(string tdid);

        TUnitOut GetUnit(string id);
        TUnitOut GetUnit(long id);

        event Callback<TUnitOut, TUnitOut> Callback_OnSetPlayer;
    }
    public interface IUnitMgr
    {
        Type UnitType { get; }

        #region set
        BaseUnit Add(string tdid);
        BaseUnit Add(int rtid);
        BaseUnit SpawnNew(string id, [DefaultValue(nameof(UnitSpawnParam.Default))] UnitSpawnParam param);
        void Despawn(BaseUnit legion);
        void Occupied(BaseUnit unit);
        Vector3 CalcAveragePos();
        void SortByScore();
        #endregion

        #region is
        bool IsHave();
        bool IsHave(BaseUnit unit);
        bool IsHave(string tdid);
        #endregion
    }
    public interface ITDMgr
    {
        Type DataType { get; }

        #region set
        TDBaseData Spawn(string id);
        void Despawn(TDBaseData data);
        #endregion
    }
    public interface ICameraMgr
    {
        event Callback<Camera> Callback_OnFetchCamera;
        float ScrollVal { get; }
        float GetCustomScrollVal(float maxVal);
        Camera MainCamera { get; }
        Transform MainCameraTrans { get; }
        void FetchCamera();
        void Enable(bool b);
        T GetPostSetting<T>() where T : PostProcessEffectSettings;
    }
    #region other
    public interface IMono
    {
        void OnEnable();
        void OnSetNeedFlag();
        void Awake();
        void OnAffterAwake();
        void Start();
        void OnAffterStart();
        void OnUpdate();
        void OnFixedUpdate();
        void OnJobUpdate();
        void OnDisable();
        void OnDestroy();
        T AddComponent<T>() where T : BaseMgr, new();
        void RemoveComponent(BaseMgr component);
    }
    public interface IUnit
    {
        bool IsInited { get; }

        #region Life
        // 角色第一次创建，逻辑初始化的时候
        void OnInit();
        // 角色复活后触发
        void OnReBirth();
        // 角色第一次创建或者复活都会触发
        void OnBirth();
        // 角色第一次创建或者复活都会触发
        void OnBirth2();
        // 角色第一次创建或者复活都会触发
        void OnBirth3();
        // 角色假死亡
        void OnDeath();
        // 角色真的死亡
        void OnRealDeath();
        // 溶解
        void OnDissolve();
        void OnGameStart1();
        void OnGameStart2();
        // 游戏开始后触发
        void OnGameStarted1();
        void OnGameStarted2();
        void OnGameStartOver();
        #endregion

        #region Turn
        // 战旗回合
        void OnTurnbase(bool day, bool month, bool year);
        // 帧回合
        void OnTurnframe(int gameFramesPerSecond);
        #endregion

        #region Login
        void OnLoginInit1(object data);
        void OnLoginInit2(object data);
        void OnLoginInit3(object data);
        #endregion
    }
    public interface IRsCacher
    {
        bool IsHave(string name);
        void RemoveNull();
    }
    public interface IRsCacherT<out T>
    {
        T Get(string name, bool isLogError = true);
    }

    public interface ITDBaseData:IBase
    {
        #region life
        void OnBeAdded(BaseCoreMono selfMono, params object[] obj);
        void OnBeRemoved();
        #endregion

        #region unit
        public BaseUnit OwnerBaseUnit { get;  }
        public BaseUnit SelfBaseUnit { get; }
        //来源
        public BaseUnit CastBaseUnit { get;}
        #endregion

        #region base get
        string GetTDID();
        string GetName();
        string GetDesc(params object[] ps);
        string GetCont();
        // 获取icon
        Sprite GetIcon();
        // 获得禁用的图标,有可能没有
        Sprite GetDisIcon();
        Sprite GetSelIcon();
        // prefab
        GameObject GetPrefab(IRsCacherT<GameObject> cacher = null);
        string GetBuff();
        // 获得animator
        RuntimeAnimatorController GetAnimator();
        //获得SFX
        AudioClip GetSFX();
        #endregion
    }
    public interface IClear
    {
        void Clear();
    }
    public interface IOnAnimTrigger
    {
        void OnAnimTrigger(int param);
    }
    #endregion
}
