//**********************************************
// Class Name	: CYMBase
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using CYM.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MoonSharp.Interpreter;
using System.Collections;
using UnityEngine.Video;
using System.Reflection;

namespace CYM
{
    [HideMonoScript]
    [RequireComponent(typeof(EventSystem))]
    [RequireComponent(typeof(StandaloneInputModule))]   
    public partial class BaseGlobal : BaseCoreMono
    {
        #region Distrubute
        public static Type Examine { get; private set; } = typeof(BaseExamineSDKMgr);
        public static Type Usual { get; private set; } = typeof(BaseUsualSDKMgr);
        public static Type Trial { get; private set; } = typeof(BaseTrialSDKMgr);
        #endregion

        #region Pool Item
        public static PoolItem PoolCommon { get; private set; } = new PoolItem(nameof(PoolCommon));
        public static PoolItem PoolUnit { get; private set; } = new PoolItem(nameof(PoolUnit));
        public static PoolItem PoolPerform { get; private set; } = new PoolItem(nameof(PoolPerform));
        public static PoolItem PoolHUD { get; private set; } = new PoolItem(nameof(PoolHUD));
        public static PoolItem PoolJumpText { get; private set; } = new PoolItem(nameof(PoolJumpText));
        public static PoolItem PoolScene { get; private set; } = new PoolItem(nameof(PoolScene));
        #endregion

        #region Resources Cache
        public static RsCacher<Material> RsMaterial { get; private set; } = new RsCacher<Material>(SysConst.BN_Materials);
        public static RsCacher<GameObject> RsPrefab { get; private set; } = new RsCacher<GameObject>(SysConst.BN_Prefab);
        public static RsCacher<GameObject> RsPerfom { get; private set; } = new RsCacher<GameObject>(SysConst.BN_Perform);
        public static RsCacher<Sprite> RsIcon { get; private set; } = new RsCacher<Sprite>(SysConst.BN_Icon);
        public static RsCacher<Sprite> RsHead { get; private set; } = new RsCacher<Sprite>(SysConst.BN_Head);
        public static RsCacher<AudioClip> RsAudio { get; private set; } = new RsCacher<AudioClip>(SysConst.BN_Audio);
        public static RsCacher<AudioClip> RsNarration { get; private set; } = new RsCacher<AudioClip>(SysConst.BN_Narration);
        public static RsCacher<AudioClip> RsMusic { get; private set; } = new RsCacher<AudioClip>(SysConst.BN_Music);
        public static RsCacher<GameObject> RsUI { get; private set; } = new RsCacher<GameObject>(SysConst.BN_UI);
        public static RsCacher<VideoClip> RsVideo { get; private set; } = new RsCacher<VideoClip>(SysConst.BN_Video);
        public static RsCacher<RuntimeAnimatorController> RsAnimator { get; private set; } = new RsCacher<RuntimeAnimatorController>(SysConst.BN_Animator);
        public static RsCacher<Sprite> RsBG { get; private set; } = new RsCacher<Sprite>(SysConst.BN_BG);
        public static RsCacher<Texture2D> RsTexture { get; private set; } = new RsCacher<Texture2D>(SysConst.BN_Texture);
        public static RsCacher<Sprite> RsIllustration { get; private set; } = new RsCacher<Sprite>(SysConst.BN_Illustration);
        #endregion

        #region Global Prop
        public static GameObject TempGO { get; private set; }
        public static Transform TempTrans
        {
            get
            {
                if (!Application.isPlaying)
                    return null;
                return TempGO?.transform;
            }
        }
        public static BaseGlobal Ins { get; protected set; }
        public static BuildConfig BuildConfig => BuildConfig.Ins;
        public static Dictionary<Type, IUnitSpawnMgr<BaseUnit>> UnitSpawnMgrs { get; private set; } = new Dictionary<Type, IUnitSpawnMgr<BaseUnit>>();
        public static Dictionary<Type, ITDSpawnMgr<TDBaseData>> TDSpawnMgrs { get; private set; } = new Dictionary<Type, ITDSpawnMgr<TDBaseData>>();
        public static IUnitSpawnMgr<BaseUnit> PlayerSpawnMgr { get; private set; }
        public static List<BaseUIMgr> UIMgrs { get; private set; } = new List<BaseUIMgr>();
        public static HashSet<string> CommandLineArgs { get; private set; } = new HashSet<string>();
        private static BoolState BoolPause { get; set; } = new BoolState();
        public static Camera MainCamera { get; private set; }
        public static DBBaseSettings Settings => SettingsMgr?.Settings;
        public static BaseUnit Player => ScreenMgr?.Player;
        public static event Callback OnInstallGlobalComponet;
        static HashSet<Type> AddedComponets = new HashSet<Type>();
        #endregion

        #region 内置自动添加的组件
        public static BaseLoaderMgr LoaderMgr { get; protected set; }
        public static BaseGRMgr GRMgr { get; protected set; }
        public static BaseLogoMgr LogoMgr { get; protected set; }
        public static BaseLuaMgr LuaMgr { get; protected set; }
        public static BaseTextAssetsMgr TextAssetsMgr { get; protected set; }
        public static BaseExcelMgr ExcelMgr { get; protected set; }
        public static BaseLangMgr LangMgr { get; protected set; }
        public static BaseConditionMgr ACM { get; protected set; }
        public static BaseBGMMgr BGMMgr { get; protected set; }
        public static BaseAudioMgr AudioMgr { get; protected set; }
        public static BaseDateTimeMgr DateTimeMgr { get; protected set; }
        public static BasePoolMgr PoolMgr { get; protected set; }
        public static BaseLoggerMgr LoggerMgr { get; protected set; }
        #endregion

        #region 内置UI组件
        public static BaseHUDUIMgr HUDUIMgr { get; protected set; }
        public static BaseCommonUIMgr CommonUIMgr { get; protected set; }
        public static BaseMainUIMgr MainUIMgr { get; protected set; }
        public static BaseBattleUIMgr BattleUIMgr { get; protected set; }
        public static BaseLevelUIMgr LevelUIMgr { get; protected set; }
        #endregion

        #region 非必要组件
        public static ISettingsMgr<DBBaseSettings> SettingsMgr { get; protected set; }
        public static IDBMgr<DBBaseGame> DBMgr { get; protected set; }
        public static IDiffMgr<DBBaseGameDiff> DiffMgr { get; protected set; }
        public static IScreenMgr<BaseUnit> ScreenMgr { get; protected set; }
        public static IPlotMgr PlotMgr { get; protected set; }
        public static IBattleMgr<TDBaseBattleData> BattleMgr { get; protected set; }
        public static ILevelMgr<TDBaseLevelData> LevelMgr { get; protected set; }
        public static BasePostProcessMgr PostProcessMgr { get; protected set; }
        public static BaseInputMgr InputMgr { get; protected set; }
        public static BaseLoginMgr LoginMgr { get; protected set; }
        public static BaseCameraMgr CameraMgr { get; protected set; }
        public static BasePlatSDKMgr PlatSDKMgr { get; protected set; }
        public static BaseCursorMgr CursorMgr { get; protected set; }
        public static BaseRefMgr RefMgr { get; protected set; }
        #endregion

        #region prop
        public static Corouter CommonCorouter { get; protected set; }
        public static Corouter MainUICorouter { get; protected set; }
        public static Corouter BattleCorouter { get; protected set; }
        public static Corouter LevelCorouter { get; protected set; }
        #endregion

        #region life
        public static bool IsDestroy { get; private set; }
        public override LayerData LayerData => SysConst.Layer_System;
        public override MonoType MonoType => MonoType.Global;
        protected virtual void OnReplaceInnerComponet()
        { 
        
        }
        protected override void OnAttachComponet() 
        {
            base.OnAttachComponet();
            //添加内置组件
            OnReplaceInnerComponet();
            AddComponent<BaseLoaderMgr>();
            AddComponent<BaseGRMgr>();
            AddComponent<BaseLogoMgr>();
            AddComponent<BaseLuaMgr>();
            AddComponent<BaseTextAssetsMgr>();
            AddComponent<BaseExcelMgr>();
            AddComponent<BaseLangMgr>();
            AddComponent<BaseConditionMgr>();
            AddComponent<BaseBGMMgr>();
            AddComponent<BaseAudioMgr>();
            AddComponent<BaseDateTimeMgr>();
            AddComponent<BasePoolMgr>();
            AddComponent<BaseLoggerMgr>();
            //触发消息
            OnInstallGlobalComponet?.Invoke();
            //安装插件
            foreach (var item in PluginGlobal.All)
            {
                item.OnInstall?.Invoke(this);
            }
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
        }
        public override void Awake()
        {
            IsDestroy = false;
            if (Ins == null) Ins = this;
            //刷新发布渠道
            RefreshDistribution();
            //创建临时对象
            TempGO = new GameObject("TempGO");
            TempGO.hideFlags = HideFlags.HideInHierarchy;
            //创建UICamera
            Util.CreateGlobalResourceObj<UICameraObj>("UICamera");
            UICameraObj.GO.transform.hideFlags = HideFlags.HideInHierarchy;
            //使应用程序无法关闭
            Application.wantsToQuit += OnWantsToQuit;
            WinUtil.DisableSysMenuButton();
            //创建必要的文件目录
            FileUtil.EnsureDirectory(SysConst.Path_Dev);
            FileUtil.EnsureDirectory(SysConst.Path_Screenshot);
            FileUtil.EnsureDirectory(SysConst.Path_LocalDB);
            FileUtil.EnsureDirectory(SysConst.Path_CloudDB);
            //添加必要的组件
            SetupComponent<UIVideoer>();
            SetupComponent<Videoer>();
            SetupComponent<Prefers>();
            SetupComponent<Feedback>();
            SetupComponent<GlobalUITextMgr>();
            SetupComponent<LogFileWriter>();
            CMail.Init(BuildConfig.FullName);
            LuaReader.Init(BuildConfig.NameSpace);
            DOTween.Init();
            DOTween.instance.transform.SetParent(Trans);
            Timing.Instance.transform.SetParent(Trans);
            QueueHub.Instance.transform.SetParent(Trans);
            Delay.Ins.transform.SetParent(Trans);
            WinUtil.DisableSysMenuButton();
            //创建所有DataParse
            OnProcessAssembly();
            base.Awake();
            //添加SDK组件
            OnAddPlatformSDKComponet();
            //读取命令行参数
            OnProcessCMDArgs();
            DontDestroyOnLoad(this);
            //携程
            CommonCorouter = new Corouter("Common");
            MainUICorouter = new Corouter("MainUI");
            BattleCorouter = new Corouter("Battle");
            LevelCorouter = new Corouter("SubBattle");
            Pos = SysConst.VEC_GlobalPos;
            //注册所有Lua函数
            OnRegisterLuaAssembly();
            //CALLBACK
            LoaderMgr.Callback_OnAllLoadEnd1 += OnAllLoadEnd1;
            LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
            LuaMgr.Callback_OnParseStart += OnLuaParseStart;
            LuaMgr.Callback_OnParseEnd += OnLuaParseEnd;
            //注册所有的Flow
            BaseFlow.InitAllFlow();
        }
        protected virtual void OnRegisterLuaAssembly()
        {
            //Auto User Data
            UserData.RegisterAssembly();
            //Global Func
            LuaMgr["GetStr"] = (Func<string,object[],string>)BaseLangMgr.Get;
            LuaMgr["Color"] = (Action<string,string, object[]>)CLog.Color;
            LuaMgr["Log"] = (Action<string, object[]>)CLog.Log;
            LuaMgr["Error"] = (Action<string, object[]>)CLog.Error;
            LuaMgr["Warn"] = (Action<string, object[]>)CLog.Warn;
            LuaMgr["Cyan"] = (Action<string, object[]>)CLog.Cyan;
            LuaMgr["Green"] = (Action<string, object[]>)CLog.Green;
            LuaMgr["Red"] = (Action<string, object[]>)CLog.Red;
            LuaMgr["Yellow"] = (Action<string, object[]>)CLog.Yellow;
            //Static
            BaseLuaMgr.RegisterStatic(typeof(CLog));
            BaseLuaMgr.RegisterStatic(typeof(UIUtil));
            BaseLuaMgr.RegisterStatic(typeof(FileUtil));
            BaseLuaMgr.RegisterStatic(typeof(FormulaUtil));
            BaseLuaMgr.RegisterStatic(typeof(HTTPUtil));
            BaseLuaMgr.RegisterStatic(typeof(IDUtil));
            BaseLuaMgr.RegisterStatic(typeof(IMUIUtil));
            BaseLuaMgr.RegisterStatic(typeof(RandUtil));
            BaseLuaMgr.RegisterStatic(typeof(StrUtil));
            BaseLuaMgr.RegisterStatic(typeof(TransformUtil));
            BaseLuaMgr.RegisterStatic(typeof(WinUtil));
            BaseLuaMgr.RegisterStatic(typeof(Util));
            //Type
            BaseLuaMgr.RegisterType<TDBaseData>();
            BaseLuaMgr.RegisterType<IList>();
            //Func
            BaseLuaMgr.RegisterFunc<int>();
            BaseLuaMgr.RegisterFunc<float>();
            BaseLuaMgr.RegisterFunc<bool>();
            BaseLuaMgr.RegisterFunc<Sprite>();
            BaseLuaMgr.RegisterFunc<Color>();
            BaseLuaMgr.RegisterFunc<string>();
            BaseLuaMgr.RegisterFunc<TDBaseData>();
            BaseLuaMgr.RegisterFunc<IList>();
            BaseLuaMgr.RegisterFunc<int, bool>();
            BaseLuaMgr.RegisterFunc<object, object>();
            BaseLuaMgr.RegisterFunc<string[]>();
            BaseLuaMgr.RegisterFunc<float, string>();
            //Action
            BaseLuaMgr.RegisterCallback();
            BaseLuaMgr.RegisterCallback<UControl, PointerEventData>();
            BaseLuaMgr.RegisterCallback<object, object>();
            BaseLuaMgr.RegisterCallback<bool>();
            BaseLuaMgr.RegisterCallback<int>();
            BaseLuaMgr.RegisterCallback<float>();
            BaseLuaMgr.RegisterCallback<UControl, bool>();
            //C#
            BaseLuaMgr.RegisterType<object>();
            //View
            BaseLuaMgr.RegisterType<UView>();
            BaseLuaMgr.RegisterType<UUIView>();
            //System
            BaseLuaMgr.RegisterType<UData>();
            BaseLuaMgr.RegisterType<UControl>();
            BaseLuaMgr.RegisterType<PointerEventData>();
            //Custom
            BaseLuaMgr.RegisterType<UCustom>();
            BaseLuaMgr.RegisterType<UCustomData>();
            //Text
            BaseLuaMgr.RegisterType<UText>();
            BaseLuaMgr.RegisterType<UTextData>();
            //Button
            BaseLuaMgr.RegisterType<UButton>();
            BaseLuaMgr.RegisterType<UButtonData>();
            //CheckBox
            BaseLuaMgr.RegisterType<UCheck>();
            BaseLuaMgr.RegisterType<UCheckData>();
            //Dropdown
            BaseLuaMgr.RegisterType<UDropdown>();
            BaseLuaMgr.RegisterType<UDropdownData>();
            //Slider
            BaseLuaMgr.RegisterType<USlider>();
            BaseLuaMgr.RegisterType<USliderData>();
            //Dupplicate
            BaseLuaMgr.RegisterType<UDupplicate>();
            BaseLuaMgr.RegisterType<UDupplicateData>();
            //Scroll
            BaseLuaMgr.RegisterType<UScroll>();
            BaseLuaMgr.RegisterType<UScrollData>();
            //Table
            BaseLuaMgr.RegisterType<UTable>();
            BaseLuaMgr.RegisterType<UTableData>();
            //Collect
            BaseLuaMgr.RegisterType<UCollect>();
            BaseLuaMgr.RegisterType<UCollect.Collect>();
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
            NeedGUI = true;
            NeedUpdate = true;
            NeedLateUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            MainCamera = Camera.main;
        }
        // 添加平台SDK组建
        protected void OnAddPlatformSDKComponet()
        {
            var type = GetDistributionType();
            if (type == null)
            {
                return;
            }
            PlatSDKMgr = AddComponent(type) as BasePlatSDKMgr;
            CLog.Cyan($"设置渠道:{GetDistributionName()},{PlatSDKMgr}");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            transform.position = SysConst.VEC_GlobalPos;
        }
#endif

        private void OnProcessAssembly()
        {
#if UNITY_EDITOR

#endif
        }
        private void OnProcessCMDArgs()
        {
#if UNITY_EDITOR
            string[] commandLineArgs = System.Environment.GetCommandLineArgs();
            foreach (var item in commandLineArgs)
            {
                CommandLineArgs.Add(item);
                if (item == "-GM")
                {
                    DiffMgr.SetGMMod(true);
                }
                CLog.Info("CMDline arg:"+item);
            }
#endif
        }
        public override void OnDestroy()
        {
            DestroyImmediate(TempGO);
            IsDestroy = true;
            base.OnDestroy();
        }
        #endregion

        #region Callback
        protected bool OnWantsToQuit() => true;
        protected virtual void OnAllLoadEnd1() { }
        protected virtual void OnAllLoadEnd2() { }
        protected virtual void OnLuaParseStart() { }
        protected virtual void OnLuaParseEnd() { }
        #endregion

        #region pause
        // 停止游戏
        public static void PauseGame(bool b)
        {
            BoolPause.Push(b);
            DoPauseGame();
        }
        public static void ResumeGame()
        {
            BoolPause.Reset();
            DoPauseGame();
        }
        private static void DoPauseGame()
        {
            if (BoolPause.IsIn())
            {
                GlobalMonoManager.SetPauseType(MonoType.Unit);
                BattleCorouter.Pause();
                LevelCorouter.Pause();
            }
            else
            {
                GlobalMonoManager.SetPauseType(MonoType.None);
                BattleCorouter.Resume();
                LevelCorouter.Pause();
            }
        }
        #endregion

        #region add componet
        public override T AddComponent<T>()
        {
            Type keyType = typeof(T);
            if (AddedComponets.Contains(keyType))
            {
                CLog.Error($"重复添加Global组建:{keyType}");
                return null;
            }
            AddedComponets.Add(keyType);

            var ret = base.AddComponent<T>();
            BaseLuaMgr.RegisterInstance(typeof(T).Name, ret);
            OnPostAddComponet(ret);
            return ret;
        }
        public override BaseMgr AddComponent(Type type)
        {
            var ret = base.AddComponent(type);
            BaseLuaMgr.RegisterInstance(type.Name, ret);
            OnPostAddComponet(ret);
            return ret;
        }
        void OnPostAddComponet(BaseMgr ret)
        {
            //自动赋值
            if (ret is IUnitSpawnMgr<BaseUnit> unitSpawner)
            {
                if (unitSpawner.IsAddToGlobalSpawnerMgr)
                    UnitSpawnMgrs.Add(unitSpawner.UnitType, unitSpawner);
                if (unitSpawner.IsPlayerSpawnMgr)
                    PlayerSpawnMgr = unitSpawner;
            }
            if (ret is ITDSpawnMgr<TDBaseData> tdSpawner)
            {
                if (tdSpawner.IsAddToGlobalSpawnerMgr)
                    TDSpawnMgrs.Add(tdSpawner.UnitType, tdSpawner);
            }
            if (ret is BaseUIMgr uiMgr)
            {
                UIMgrs.Add(uiMgr);
            }

            if (ret is BaseLoaderMgr && LoaderMgr == null) LoaderMgr = ret as BaseLoaderMgr;
            else if (ret is BaseGRMgr && GRMgr == null) GRMgr = ret as BaseGRMgr;
            else if (ret is BaseLogoMgr && LogoMgr == null) LogoMgr = ret as BaseLogoMgr;
            else if (ret is BaseExcelMgr && ExcelMgr == null) ExcelMgr = ret as BaseExcelMgr;
            else if (ret is BaseLuaMgr && LuaMgr == null) LuaMgr = ret as BaseLuaMgr;
            else if (ret is BaseTextAssetsMgr && TextAssetsMgr == null) TextAssetsMgr = ret as BaseTextAssetsMgr;
            else if (ret is BaseLangMgr && LangMgr == null) LangMgr = ret as BaseLangMgr;
            else if (ret is BaseConditionMgr && ACM == null) ACM = ret as BaseConditionMgr;
            else if (ret is BaseBGMMgr && BGMMgr == null) BGMMgr = ret as BaseBGMMgr;
            else if (ret is BaseAudioMgr && AudioMgr == null) AudioMgr = ret as BaseAudioMgr;
            else if (ret is BaseDateTimeMgr && DateTimeMgr == null) DateTimeMgr = ret as BaseDateTimeMgr;
            else if (ret is BasePoolMgr && PoolMgr == null) PoolMgr = ret as BasePoolMgr;
            else if (ret is BaseLoggerMgr && LoggerMgr == null) LoggerMgr = ret as BaseLoggerMgr;

            //UI组件
            else if (ret is BaseHUDUIMgr && HUDUIMgr == null) HUDUIMgr = ret as BaseHUDUIMgr;
            else if (ret is BaseCommonUIMgr && CommonUIMgr == null) CommonUIMgr = ret as BaseCommonUIMgr;
            else if (ret is BaseMainUIMgr && MainUIMgr == null) MainUIMgr = ret as BaseMainUIMgr;
            else if (ret is BaseBattleUIMgr && BattleUIMgr == null) BattleUIMgr = ret as BaseBattleUIMgr;
            else if (ret is BaseLevelUIMgr && LevelUIMgr == null) LevelUIMgr = ret as BaseLevelUIMgr;

            //非必要接口
            else if (ret is ISettingsMgr<DBBaseSettings> && SettingsMgr == null) SettingsMgr = ret as ISettingsMgr<DBBaseSettings>;
            else if (ret is IDBMgr<DBBaseGame> && DBMgr == null) DBMgr = ret as IDBMgr<DBBaseGame>;
            else if (ret is IDiffMgr<DBBaseGameDiff> && DiffMgr == null) DiffMgr = ret as IDiffMgr<DBBaseGameDiff>;
            else if (ret is IScreenMgr<BaseUnit> && ScreenMgr == null) ScreenMgr = ret as IScreenMgr<BaseUnit>;
            else if (ret is IPlotMgr && PlotMgr == null) PlotMgr = ret as IPlotMgr;
            else if (ret is IBattleMgr<TDBaseBattleData> && BattleMgr == null) BattleMgr = ret as IBattleMgr<TDBaseBattleData>;
            else if (ret is ILevelMgr<TDBaseLevelData> && LevelMgr == null) LevelMgr = ret as ILevelMgr<TDBaseLevelData>;

            //非必要组件
            else if (ret is BaseInputMgr && InputMgr == null) InputMgr = ret as BaseInputMgr;
            else if (ret is BaseRefMgr && RefMgr == null) RefMgr = ret as BaseRefMgr;
            else if (ret is BaseLoginMgr && LoginMgr == null) LoginMgr = ret as BaseLoginMgr;
            else if (ret is BasePlatSDKMgr && PlatSDKMgr == null) PlatSDKMgr = ret as BasePlatSDKMgr;
            else if (ret is BaseCursorMgr && CursorMgr == null) CursorMgr = ret as BaseCursorMgr;
            else if (ret is BaseCameraMgr && CameraMgr == null) CameraMgr = ret as BaseCameraMgr;

            //执行插件代码
            foreach (var item in PluginGlobal.All)
            {
                item.OnPostAddComponet?.Invoke(ret);
            }
        }
        #endregion

        #region set
        public static List<IClear> ClearWhenBattleUnload { get; private set; } = new List<IClear>();
        public static List<IClear> ClearWhenLevelUnload { get; private set; } = new List<IClear>();
        public static void AddToClearWhenBattleUnload(IClear clear) => ClearWhenBattleUnload.Add(clear);
        public static void AddToClearWhenLevelUnload(IClear clear) => ClearWhenLevelUnload.Add(clear);
        public static void Quit()
        {
            if (!Application.isEditor)
            {
                Application.Quit();
            }
        }
        public static void GCCollect()
        {
            if (Application.isMobilePlatform)
                return;
            GC.Collect();
        }
        public static void ForceGCCollect()
        {
            GC.Collect();
        }
        #endregion

        #region get
        public static Transform GetTransform(Vector3 pos)
        {
            TempTrans.position = pos;
            return TempTrans;
        }
        public static IUnitSpawnMgr<BaseUnit> GetUnitSpawnMgr(Type unitType)
        {
            if (UnitSpawnMgrs.ContainsKey(unitType))
            {
                return UnitSpawnMgrs[unitType];
            }
            return null;
        }
        public static ITDSpawnMgr<TDBaseData> GetTDSpawnerMgr(Type unityType)
        {
            if (TDSpawnMgrs.ContainsKey(unityType))
            {
                return TDSpawnMgrs[unityType];
            }
            return null;
        }
        public static TUnit GetUnit<TUnit>(long id, bool isLogError = true) where TUnit : BaseUnit
        {
            if (id.IsInv()) return null;
            var ret = GetUnit(id, typeof(TUnit)) as TUnit;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏实体!!!,{0}", id);
            }
            return ret;
        }
        public static TUnit GetUnit<TUnit>(string id, bool isLogError = true) where TUnit : BaseUnit
        {
            if (id.IsInv()) return null;
            var ret = GetUnit(id, typeof(TUnit)) as TUnit;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏实体!!!,{0}", id);
            }
            return ret;
        }
        public static BaseUnit GetUnit(long id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in UnitSpawnMgrs)
                {
                    var temp = item.Value.GetUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (UnitSpawnMgrs.ContainsKey(unitType))
                {
                    return UnitSpawnMgrs[unitType].GetUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得Entity,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        public static BaseUnit GetUnit(string id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in UnitSpawnMgrs)
                {
                    var temp = item.Value.GetUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (UnitSpawnMgrs.ContainsKey(unitType))
                {
                    return UnitSpawnMgrs[unitType].GetUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得Entity,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        public static HashList<BaseUnit> GetUnit(List<long> ids)
        {
            HashList<BaseUnit> data = new HashList<BaseUnit>();
            foreach (var item in ids)
            {
                var entity = GetUnit(item);
                if (entity == null) continue;
                data.Add(entity);
            }
            return data;
        }
        public static List<long> GetUnitIDs(HashList<BaseUnit> entity)
        {
            List<long> ids = new List<long>();
            foreach (var item in entity)
            {
                if (item.IsInv()) continue;
                ids.Add(item.ID);
            }
            return ids;
        }
        #endregion

        #region Get TDData
        public static TData GetTDData<TData>(long id, bool isLogError = true) where TData : TDBaseData
        {
            if (id.IsInv()) return null;
            var ret = GetTDData(id, typeof(TData)) as TData;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏配置对象!!!,{0}", id);
            }
            return ret;
        }
        public static TData GetTDData<TData>(string id, bool isLogError = true) where TData : TDBaseData
        {
            if (id.IsInv()) return null;
            var ret = GetTDData(id, typeof(TData)) as TData;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏配置对象!!!,{0}", id);
            }
            return ret;
        }
        public static TDBaseData GetTDData(long id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in TDSpawnMgrs)
                {
                    var temp = item.Value.GetUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (TDSpawnMgrs.ContainsKey(unitType))
                {
                    return TDSpawnMgrs[unitType].GetUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得TDData,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        public static TDBaseData GetTDData(string id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in TDSpawnMgrs)
                {
                    var temp = item.Value.GetUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (TDSpawnMgrs.ContainsKey(unitType))
                {
                    return TDSpawnMgrs[unitType].GetUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得TDData,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        #endregion

        #region is
        // 是否暂停游戏
        public static bool IsPause => BoolPause.IsIn();
        // 是否处于读取数据阶段，用于避免触发一些回调
        public static bool IsUnReadData { get; set; } = true;
        public static bool IsHaveCommandLineArg(string arg)=> CommandLineArgs.Contains(arg);
        public static bool Is3D
        {
            get
            {
                if (MainCamera == null)
                    return false;
                return !MainCamera.orthographic;
            }
        }
        public static bool Is2D
        {
            get
            {
                if (MainCamera == null)
                    return false;
                return MainCamera.orthographic;
            }
        }
        #endregion

        #region 渠道
        public static string[] DistributionOptions { get; private set; }
        static Dictionary<string, Type> Distribution { get;set; } = new Dictionary<string, Type>();
        public static string GetDistributionName()
        {
            if (DistributionOptions == null)
                return "";
            var key = BuildConfig.Ins.Distribution;
            if (key >= DistributionOptions.Length)
                return "";
            return DistributionOptions[key];
        }
        public static Type GetDistributionType()
        {
            if (DistributionOptions == null)
                return null;
            var key = BuildConfig.Ins.Distribution;
            if (key >= DistributionOptions.Length)
                return null;
            return Distribution[DistributionOptions[key]];
        }
        public static void RefreshDistribution()
        {
            Distribution.Clear();
            List<string> tempList = new List<string>();
            var data = typeof(BaseGlobal).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var item in data)
            {
                if (item == null)
                    continue;
                var val = item.GetValue(item) as Type;
                if (val!=null && val.IsSubclassOf(typeof(BasePlatSDKMgr)))
                {
                    tempList.Add(item.Name);
                    Distribution.Add(item.Name, val);
                }
            }
            DistributionOptions = tempList.ToArray();
        }
        #endregion
    }
}
