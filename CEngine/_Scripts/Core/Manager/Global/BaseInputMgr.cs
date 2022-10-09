//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using CYM.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace CYM
{
    public class BaseInputMgr : BaseGFlowMgr
    {
        #region const
        const string StrMouseScrollWheel = "Mouse ScrollWheel";
        const float LongPressTimeDuration = 0.2f;
        #endregion

        #region Callback
        public event Callback<BaseUnit, bool> Callback_OnSelectedUnit;
        public event Callback<Vector3, int> Callback_OnInputDown;
        public event Callback<Vector3, int> Callback_OnInputUp;
        public event Callback<BaseUnit> Callback_OnEnterUnit;
        public event Callback<BaseUnit> Callback_OnExitUnit;
        public event Callback<BaseUnit> Callback_OnLeftDown;
        public event Callback<BaseUnit> Callback_OnRightDown;
        public event Callback<BaseUnit> Callback_OnLeftUp;
        public event Callback<BaseUnit> Callback_OnRightUp;
        public event Callback<BaseUnit> Callback_OnLeftClick;
        public event Callback<BaseUnit> Callback_OnRightClick;
        public event Callback Callback_OnInputMapChanged;
        public event Callback Callback_OnAnyKeyDown;
        #endregion

        #region mgr
        protected BaseUnit BasePlayer => BaseGlobal.ScreenMgr?.Player;
        protected static BaseInputMgr Ins => BaseGlobal.InputMgr;
        BuildConfig BuildConfig => BuildConfig.Ins;
        #endregion

        #region map
        public static InputActionMap GamePlayMap { get; private set; }
        public static InputActionMap MenuMap { get; private set; }
        protected static InputActionAsset InputAsset { get; private set; }
        #endregion

        #region last
        public static GameObject LastHitUI
        {
            get
            {
                if (LastHitUIResults.Count > 0)
                    return LastHitUIResults[0].gameObject;
                return null;
            }
        }
        public static List<RaycastResult> LastRaycastUIObj { get; private set; } = new List<RaycastResult>();
        public static List<RaycastResult> LastHitUIResults { get; private set; } = new List<RaycastResult>();
        protected static List<BaseUnit> LeftSelectUnits { get; set; } = new List<BaseUnit>();
        protected static int LeftSelectFilterIndex { get; set; } = 0;
        protected static Component LastHitCollider { get; set; }
        protected static Component LastHitUpCollider { get; set; }
        protected static Vector3 LastMouseDownPos { get; private set; }
        protected static Vector3 LastMousePos { get; private set; }
        protected static Vector3 LastTouchDownPos { get; private set; }
        protected static RaycastHit LastHit => lastHit;
        static RaycastHit lastHit;
        #endregion

        #region state
        protected static BoolState IsFullScreenState { get; set; } = new BoolState();
        protected static BoolState IsDisablePlayerInputState { get; set; } = new BoolState();
        protected static BoolState IsDisableUnitSelectState { get; set; } = new BoolState();
        #endregion

        #region static obj
        public static UHUDBar HoverHUDBar { get; private set; } = null;
        public static BaseUnit PreLastMouseOverUnit { get; private set; } = null;
        public static BaseUnit LastMouseOverUnit { get; private set; } = null;
        public static BaseUnit HoverUnit { get; private set; } = null;
        public static BaseUnit SelectedUnit { get; private set; } = null;
        public static HashList<BaseUnit> PreSelectUnits { get; private set; } = new HashList<BaseUnit>();
        public static HashList<BaseUnit> SelectedUnits { get; private set; } = new HashList<BaseUnit>();
        public static int PreSelectUnitsCount => PreSelectUnits.Count;
        public static int SelectedUnitsCount => SelectedUnits.Count;
        #endregion

        #region timer
        // 选择单位后的一个记时,可以防止重复操作
        static Timer SelectUnitTimer = new Timer(0.1f);
        static Timer MouseEnterUnitTimer = new Timer(0.02f);
        #endregion

        #region private
        static float TouchDPI = 1;
        static float DragDPI = 1;
        static float TouchLastAngle;
        static float TouchLastDist = 0;
        static float LongPressTimeFlag = 0;
        #endregion

        #region pub
        public static float LongPressTime
        {
            get
            {
                if (LongPressTimeFlag <= 0)
                    return 0;
                return Time.time - LongPressTimeFlag;
            }
        }
        public static float PreLongPressTime { get; private set; }
        #endregion

        #region pub prop
        public static bool IsDevConsoleShow { get; private set; } = false;
        //是否全屏
        public static bool IsFullScreen => IsFullScreenState.IsIn();
        public static bool IsEnablePlayerInput => !IsDisablePlayerInputState.IsIn();
        public static bool IsStayHUDBar { get; private set; } = false;
        public static bool IsStayInUI { get; private set; } = false;
        public static bool IsStayInUIWithoutHUD => IsStayInUI && !IsStayHUDBar;
        public static bool IsLastHitUI { get; private set; } = false;
        public static bool IsStayInTerrain { get; private set; } = false;
        public static bool IsStayInUnit { get; private set; } = false;
        public static bool IsScrollWheel { get; private set; }
        public static bool IsPress { get; private set; }
        public static bool IsInGuideMask { get; private set; } = false;
        #endregion

        #region Is
        //传入的Unit是否和最近悬浮的Unit是一样的
        public static bool IsOverSameLastUnit(BaseUnit unit) => LastMouseOverUnit == unit;
        // 鼠标按下的位置是否和弹起的时候处于同一个位置
        public static bool IsSameMousePt(float val = 0f)
        {
            if (val > 0) return MathUtil.Approximately(LastMouseDownPos, Input.mousePosition, val);
            else return MathUtil.Approximately(LastMouseDownPos, Input.mousePosition);
        }
        public static bool IsSameTouchPt(float val = 0f)
        {
            if (val > 0) return MathUtil.Approximately(LastTouchDownPos, ScreenPos, val);
            else return MathUtil.Approximately(LastTouchDownPos, ScreenPos);
        }
        public static bool IsSamePt(float val = 0f)
        {
            if (val > 0) return MathUtil.Approximately(LastScreenDownPos, ScreenPos, val);
            else return MathUtil.Approximately(LastScreenDownPos, ScreenPos);
        }
        public static bool IsHaveLastHitUI()
        {
            if (LastHitUIResults.Count > 0) return true;
            return false;
        }
        public static bool IsHaveLastHitCollider() => LastHitCollider != null;
        public static bool IsSameLastHitCollider()
        {
            if (LastHitCollider == null || LastHitUpCollider == null)
                return false;
            return LastHitCollider == LastHitUpCollider;
        }

        public static bool IsOverSamePreLastUnit() => LastMouseOverUnit == PreLastMouseOverUnit;
        public static bool IsMouseEnterUnitTimerOver() => MouseEnterUnitTimer.CheckOver();
        public static bool IsLongPressTime(float duration = LongPressTimeDuration)
        {
            if (LongPressTime >= duration)
                return true;
            if (PreLongPressTime >= duration)
                return true;
            return false;
        }
        public static bool IsSelectUnit(BaseUnit unit)
        {
            return SelectedUnits.Contains(unit);
        }
        #endregion

        #region is can
        public static bool IsCanSelectUnit()
        {
            if (!IsCanInput()) return false;
            return !IsDisableUnitSelectState.IsIn();
        }
        public static bool IsCanInput()
        {
            if (BaseGlobal.Ins == null) return false;
            if (IsFullScreen) return false;
            if (IsDevConsoleShow) return false;
            return true;
        }
        public static bool IsCanMenuInput()
        {
            if (BaseGlobal.Ins == null) return false;
            if (BaseGlobal.BattleMgr == null) return false;
            if (BaseGlobal.BattleMgr.IsInBattle) return false;
            if (!IsCanInput()) return false;
            return true;
        }
        public static bool IsCanGamePlayInput()
        {
            if (BaseGlobal.Ins == null) return false;
            if (BaseGlobal.IsPause) return false;
            if (BaseGlobal.BattleMgr == null) return false;
            if (!BaseGlobal.BattleMgr.IsInBattle) return false;
            if (!IsCanInput()) return false;
            if (IsDisablePlayerInputState.IsIn()) return false;
            if (IsDevConsoleShow) return false;
            return true;
        }
        #endregion

        #region is have
        public bool IsHaveSelectedUnits()
        {
            if (SelectedUnits.Count > 0)
                return true;
            if (SelectedUnit != null)
                return true;
            return false;
        }
        #endregion

        #region Static select unit
        //单不选单位
        public static void UnSelectUnit()
        {
            ScilentUnSingleSelectUnit();
            ScilentUnGroupSelectAllUnits();
            Ins?.OnUnSelectUnit();
        }
        //单选单位
        public static void SingleSelectUnit(BaseUnit unit)
        {
            //检测这个是否可以被选择
            if (!Ins.IsCanSelectUnit(unit))
                return;

            //选择一个单位后无法再次选择
            if (IsInSelectUnitTime())
                return;

            //取消框选
            ScilentUnGroupSelectAllUnits();
            //检测是否重复选择
            bool isRepeat = IsInSelect(unit);
            //如果是新的单位则取消上一个单位的选择
            if (!isRepeat)
            {
                SelectedUnit?.OnUnBeSelected();
            }
            //选择单位
            if (unit)
            {
                SelectedUnits.Add(unit);
                SelectedUnit = unit;
                unit?.OnBeSelected(isRepeat);
            }
            //空选单位
            else
            {
                SelectedUnit?.OnUnBeSelected();
                SelectedUnit = null;
            }
            SelectUnitTimer.Restart();
            Ins?.OnSelectedUnit(SelectedUnit, isRepeat);
            Ins?.OnSingleSelectUnit(SelectedUnit);
        }
        public static void GroupToggleUnit(BaseUnit unit)
        {
            if (IsInSelect(unit))
            {
                SilentUnGroupSelectUnit(unit);
                Ins?.OnGroupToggleUnit(false, unit);
            }
            else
            {
                GroupSeletcUnit(unit);
                Ins?.OnGroupToggleUnit(true, unit);
            }
        }
        #endregion

        #region Silent 静默选择,不会触发一些回调函数 
        static void ScilentUnSingleSelectUnit()
        {
            SelectedUnit?.OnUnBeSelected();
            SelectedUnits.Remove(SelectedUnit);
            SelectedUnit = null;
        }
        static void SilentUnGroupSelectUnit(BaseUnit unit)
        {
            //检测是否重复选择
            bool isRepeat = IsInSelect(unit);
            if (unit && isRepeat)
            {
                unit?.OnUnBeSelected();
                SelectedUnits.Remove(unit);
                SelectedUnit = null;
            }
        }
        static void ScilentUnGroupSelectAllUnits()
        {
            PreSelectUnits.Clear();
            foreach (var item in SelectedUnits)
            {
                item.OnUnBeSelected();
                PreSelectUnits.Add(item);
            }
            SelectedUnits.Clear();
        }
        #endregion

        #region unit
        protected virtual float OverlapRadius => 3.0f;
        protected virtual LayerMask SelectUnitLayerMask => (LayerMask)SysConst.Layer_Default;
        protected virtual bool IsCanSelectUnit(BaseUnit unit)
        {
            if (unit != null &&
                BaseGlobal.PlotMgr != null &&
                BaseGlobal.PlotMgr.IsBlockerUnit(unit))
                return false;
            if (!IsCanSelectUnit()) 
                return false;
            //如果这个单位死亡后,则不可被选择
            if (unit!=null && !unit.IsLive) 
                return false;
            return true;
        }
        protected static bool IsInSelect(BaseUnit unit)
        {
            if (unit == null)
                return false;
            //检测是否重复选择
            bool isRepeat = false;
            if (SelectedUnit == unit ||
                SelectedUnits.Contains(unit))
            {
                isRepeat = true;
            }
            return isRepeat;
        }
        //框选单位
        static void GroupSeletcUnit(BaseUnit unit)
        {
            //检测这个是否可以被选择
            if (!Ins.IsCanSelectUnit(unit))
                return;

            //检测是否重复选择
            bool isRepeat = IsInSelect(unit);

            if (unit)
            {
                SelectedUnits.Add(unit);
                SelectedUnit = unit;
                unit?.OnBeSelected(isRepeat);
            }
            Ins?.OnSelectedUnit(unit, isRepeat);
        }
        protected virtual void OnStartRectSelect()
        {
            if (!NeedGroupSelect)
                return;
        }
        protected virtual void OnEndRectSelect()
        {
            if (!NeedGroupSelect)
                return;
            if (BaseGlobal.BattleMgr == null)
                return;

            if (BaseGlobal.BattleMgr.IsGameStartOver)
            {
                ScilentUnGroupSelectAllUnits();
                ScilentUnSingleSelectUnit();

                var rawData = GetGroupSelectRawUnits();
                if (rawData == null)
                {
                    CLog.Error($"错误：{nameof(GetGroupSelectRawUnits)} 为 null");
                    return;
                }
                foreach (var item in rawData)
                {
                    if (SelectionRect.IsSelected(item))
                    {
                        GroupSeletcUnit(item);
                    }
                }
                OnGroupSelectUnit(SelectedUnits);
            }
        }
        //用户自定义的函数，用于提供框选单位的原始数据
        protected virtual IEnumerable<BaseUnit> GetGroupSelectRawUnits()
        {
            return null;
        }
        #endregion

        #region set state
        public static void PushPlayerInputState(bool b) => IsDisablePlayerInputState.Push(!b);
        public static void PushFullScreenState(bool b) => IsFullScreenState.Push(b);
        public static void PushUnitSelect(bool b) => IsDisableUnitSelectState.Push(!b);
        public static void ResetFullScreenState()=> IsFullScreenState.Reset();
        public static void ResetPlayerInputState() => IsDisablePlayerInputState.Reset();
        public static void ResetUnitSelectState() => IsDisableUnitSelectState.Reset();
        #endregion

        #region life
        protected virtual bool NeedUpdateRaycast3D => true;
        protected virtual bool NeedUpdateHitUI => true;
        protected virtual bool NeedGroupSelect => false;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            TouchDPI = Mathf.Max(Screen.dpi, BuildConfig.TouchDPI);
            DragDPI = Mathf.Max(Screen.dpi, BuildConfig.DragDPI);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            BaseUnit.Callback_OnRealDeathG += OnRealDeathG;
            BaseUnit.Callback_OnDeathG += OnDeathG;
            BaseUIMgr.Callback_OnControlClick += OnControlClick;
            SelectionRect.Callback_OnStartSelect += OnStartRectSelect;
            SelectionRect.Callback_OnEndSelect += OnEndRectSelect; 

        }
        public override void OnDisable()
        {
            BaseUnit.Callback_OnDeathG -= OnDeathG;
            BaseUnit.Callback_OnRealDeathG -= OnRealDeathG;
            BaseUIMgr.Callback_OnControlClick -= OnControlClick;
            SelectionRect.Callback_OnStartSelect -= OnStartRectSelect;
            SelectionRect.Callback_OnEndSelect -= OnEndRectSelect;
            base.OnDisable();
        }
        public override void OnAffterStart()
        {
            base.OnAffterStart();
            InputAsset = Resources.Load<InputActionAsset>("InputConfig");
            if (InputAsset == null)
            {
                CLog.Error("没有配置:InputConfig,请自行创建：GamePlay,Menu");
            }
            else
            {
                GamePlayMap = TryGetActionMap("GamePlay");
                MenuMap = TryGetActionMap("Menu");
            }
            Load();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateMapEnable(GamePlayMap, IsCanGamePlayInput);
            UpdateMapEnable(MenuMap, IsCanMenuInput);
            UpdateMouse();
            UpdateKey();
            UpdateTouch();
            UpdateRaycast();
            UpdateGUI();
            UpdateHitUI();
            UpdateFlag();
        }
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
            ResetPlayerInputState();
            ResetUnitSelectState();
        }
        #endregion

        #region update
        void UpdateKey()
        {
            if (Application.isMobilePlatform)
                return;
            if (IsAnyKeyDown())
            {
                Callback_OnAnyKeyDown?.Invoke();
            }
        }
        void UpdateGUI()
        {
            if (UGuideView.Default != null)
            {
                IsInGuideMask = UGuideView.Default.IsInMask;
            }
            IsDevConsoleShow = SysConsole.IsShow();
            bool pre = IsStayInUI;
            IsStayInUI = CheckOverUI();
            if (IsStayInUI != pre)
            {
                if (IsStayInUI)
                    OnEnterUI();
                else
                    OnExitUI();
            }
        }
        void UpdateHitUI()
        {
            if (!NeedUpdateHitUI)
                return;
            if (IsStayInUI && GetDown())
            {
                LastHitUIResults.Clear();
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                eventDataCurrentPosition.position = ScreenPos;
                EventSystem.current.RaycastAll(eventDataCurrentPosition, LastHitUIResults);
            }
        }
        void UpdateRaycast()
        {
            if (ULoadingView.IsInLoading)
                return;
            if (!NeedUpdateRaycast3D)
                return;
            if (!BattleMgr.IsInBattle) 
                return;
            if (IsStayInUI)
            {
                IsStayInTerrain = false;
                return;
            }
            if (IsStayInUnit)
            {
                IsStayInTerrain = false;
                return;
            }
            if (BaseGlobal.MainCamera == null) return;
            if(Util.ScreenRayCast(out lastHit, (LayerMask)SysConst.Layer_Terrain))
            {
                bool pre = IsStayInTerrain;
                IsStayInTerrain = true;
                IsStayInUI = false;
                IsStayInUnit = false;
                if (pre != IsStayInTerrain)
                {
                    if (IsStayInTerrain)
                        OnEnterTerrain(LastHit.point);
                    else
                        OnExitTerrain();
                }
            }
            else
            {
                IsStayInTerrain = false;
            }
        }
        void UpdateMouse()
        {
            if (Application.isMobilePlatform)
                return;
            if (ULoadingView.IsInLoading)
                return;
            IsScrollWheel = Input.GetAxis(StrMouseScrollWheel) != 0;
            IsPress = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
            if (!IsCanGamePlayInput())
                return;
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    LastMouseDownPos = Input.mousePosition;
                    OnInputDown(Input.mousePosition, i);
                    Callback_OnInputDown?.Invoke(Input.mousePosition, i);
                }
                else if (Input.GetMouseButtonUp(i))
                {
                    OnInputUp(Input.mousePosition, i);
                    Callback_OnInputUp?.Invoke(Input.mousePosition, i);
                }
            }

            if (LastMousePos != Input.mousePosition)
            {
                OnInputMove(Input.mousePosition, 0);
            }
            LastMousePos = Input.mousePosition;
        }
        void UpdateTouch()
        {
            if (!Application.isMobilePlatform)
                return;
            if (ULoadingView.IsInLoading)
                return;
            CalculateTouchPosition();
            CalculateTouchDrag();
            CalculateTouchScale();
            CalculateTouchRotate();
            void CalculateTouchPosition()
            {
                IsPress = false;
                if (Input.touchCount > 0)
                {
                    var touch = Input.touches[0];
                    if (touch.phase == UnityEngine.TouchPhase.Began)
                    {
                        LastTouchDownPos = TouchPosition;
                        OnInputDown(TouchPosition, 0);
                        Callback_OnInputDown?.Invoke(TouchPosition, 0);
                    }
                    else if (touch.phase == UnityEngine.TouchPhase.Ended ||
                             touch.phase == UnityEngine.TouchPhase.Canceled)
                    {
                        OnInputUp(TouchPosition, 0);
                        Callback_OnInputUp?.Invoke(TouchPosition, 0);
                    }
                    else if (touch.phase == UnityEngine.TouchPhase.Stationary)
                    {
                        IsPress = true;
                    }
                }
            }
            void CalculateTouchDrag()
            {
                if (Input.touchCount != 1 || Input.touches[0].phase == UnityEngine.TouchPhase.Ended)
                {
                    if (PreLongPressTime < 0.2f ||
                        LongPressTime<0.2f)
                    {
                        if (IsLastHitUI)
                        {
                            TouchDragValue = Vector2.zero;
                            return;
                        }
                        LerpToZero();
                    }
                    else
                    {
                        TouchDragValue = Vector2.zero;
                    }
                    return;
                }

                if (Input.touches[0].phase != UnityEngine.TouchPhase.Moved)
                {
                    TouchDragValue = Vector2.zero;
                    return;
                }

                if (Input.touchCount > 0)
                {
                    float threold=0.005f;
                    Vector2 dragVal = DeltaMovementForTouch(0);
                    if (Mathf.Abs(dragVal.x) > threold || Mathf.Abs(dragVal.y) > threold)
                        TouchDragValue = -dragVal;
                    else
                        TouchDragValue = Vector2.zero;
                }

                Vector2 DeltaMovementForTouch(int fingerID)
                {
                    Touch touch = Input.touches[fingerID];
                    return touch.deltaPosition / DragDPI;
                }
                void LerpToZero()
                {
                    TouchDragValue = Vector3.Lerp(TouchDragValue, Vector2.zero,Time.deltaTime*15);
                }
            }
            void CalculateTouchScale()
            {
                if (Input.touchCount != 2)
                {
                    TouchScaleValue = 0f;
                    TouchLastDist = 0;
                    return;
                }

                if (Input.touches[0].phase != UnityEngine.TouchPhase.Moved && Input.touches[1].phase != UnityEngine.TouchPhase.Moved)
                {
                    TouchScaleValue = 0;
                    if (Input.touches[0].phase == UnityEngine.TouchPhase.Ended && Input.touches[1].phase == UnityEngine.TouchPhase.Ended) TouchLastDist = 0;

                    return;
                }

                float curDist = DistanceForTouch(0, 1);
                if (TouchLastDist == 0) TouchLastDist = curDist;
                TouchScaleValue = (curDist - TouchLastDist) * -0.01f;
                TouchLastDist = curDist;

                float DistanceForTouch(int fingerA, int fingerB)
                {
                    return (Input.touches[0].position - Input.touches[1].position).magnitude / TouchDPI;
                }
            }
            void CalculateTouchRotate()
            {
                if (Input.touchCount != 2)
                {
                    TouchRotateValue = 0;
                    TouchLastAngle = 0;
                    return;
                }
                Vector2 v2 = (Input.touches[1].position - Input.touches[0].position) / TouchDPI;
                float curAngle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
                if (TouchLastAngle == 0) TouchLastAngle = curAngle;

                TouchRotateValue = curAngle - TouchLastAngle;
                TouchLastAngle = curAngle;
            }
        }
        void UpdateFlag()
        {
            if (GetUp(false))
            {
                PreLongPressTime = LongPressTimeFlag;
                LongPressTimeFlag = 0;
            }
            if (GetDown(false))
            {
                PreLongPressTime = 0;
                LongPressTimeFlag = Time.time;
            }
        }
        #endregion

        #region get
        protected InputActionMap TryGetActionMap(string id) => InputAsset?.FindActionMap(id);
        protected InputAction GetGamePlayAction(string id) => GamePlayMap?.FindAction(id);
        protected InputAction GetMenuAction(string id) => MenuMap?.FindAction(id);
        #endregion

        #region set
        public static void SetHoverHUDBar(UHUDBar bar)
        {
            if (bar != null)
            {
                IsStayHUDBar = true;
                HoverHUDBar = bar;
            }
            else
            {
                IsStayHUDBar = false;
            }
        }
        void UpdateMapEnable(InputActionMap map, Func<bool> DoIsEnable)
        {
            if (map != null && DoIsEnable != null)
            {
                bool temp = DoIsEnable();
                if (map.enabled != temp)
                {
                    if (temp)
                        map.Enable();
                    else
                        map.Disable();
                }
            }
        }
        protected InputAction RegisterGameplay(string id, Action<CallbackContext> perform, Action<CallbackContext> start = null, Action<CallbackContext> cancel = null)
        {
            var item = GetGamePlayAction(id);
            if (item == null)
                return null;
            if (perform!=null) item.performed += perform;
            if (start != null) item.started += start;
            if (cancel != null) item.canceled += cancel;
            return item;
        }
        protected InputAction RegisterMenu(string id, Action<CallbackContext> perform, Action<CallbackContext> start = null, Action<CallbackContext> cancel = null)
        {
            var item = GetMenuAction(id);
            if (item == null)
                return null;
            if (perform != null) item.performed += perform;
            if (start != null) item.started += start;
            if (cancel != null) item.canceled += cancel;
            return item;
        }
        public void Save()
        {
            FileUtil.SaveJson(SysConst.Path_Shortcuts, InputAsset.ToJson());
        }
        public void Load()
        {
            string data = FileUtil.LoadFile(SysConst.Path_Shortcuts);
            if (data == null)
                return;
            InputAsset.LoadFromJson(data);
        }
        #endregion

        #region mouse click
        public virtual void LeftClick(BaseUnit arg1, bool isForce = false)
        {
            if (arg1 == null)
            {
                if (!isForce)
                {
                    if (!IsStayInUI && !IsHaveLastHitCollider() && IsSamePt(0))
                        UnSelectUnit();
                }
                else
                {
                    UnSelectUnit();
                }
            }
            else if (arg1 != null)
            {
                if (isForce)
                {
                    SingleSelectUnit(arg1);
                }
                else if (!IsStayInUIWithoutHUD)
                    SingleSelectUnit(arg1);
            }
            Callback_OnLeftClick?.Invoke(arg1);
        }
        public virtual void RightClick(BaseUnit arg1, bool isForce = false)
        {
            Callback_OnRightClick?.Invoke(arg1);
        }
        public virtual void LeftDown(BaseUnit arg1, bool isForce = false)
        {
            Callback_OnLeftDown?.Invoke(arg1);
        }
        public virtual void RightDown(BaseUnit arg1, bool isForce = false)
        {
            Callback_OnRightDown?.Invoke(arg1);
        }
        public virtual void LeftUp(BaseUnit arg1, bool isForce = false)
        {
            Callback_OnLeftUp?.Invoke(arg1);
        }
        public virtual void RightUp(BaseUnit arg1, bool isForce = false)
        {
            Callback_OnRightUp?.Invoke(arg1);
        }
        #endregion

        #region pub callback
        protected virtual bool OnEnterUnit(BaseUnit arg1)
        {
            if (CheckOverUI(true))
                return false;
            MouseEnterUnitTimer.Restart();
            PreLastMouseOverUnit = LastMouseOverUnit;
            IsStayInUnit = true;
            LastMouseOverUnit = arg1;
            HoverUnit = arg1;
            Callback_OnEnterUnit?.Invoke(arg1);
            arg1.Callback_OnMouseEnter?.Invoke();
            return true;
        }
        protected virtual bool OnExitUnit(BaseUnit arg1)
        {
            IsStayInUnit = false;
            HoverUnit = null;
            Callback_OnExitUnit?.Invoke(arg1);
            arg1.Callback_OnMouseExit?.Invoke();
            return true;
        }
        Invoke.IJob ButtonExitJob;
        public void DoEnterUnit(BaseUnit arg1)
        {
            ButtonExitJob?.Kill();
            OnEnterUnit(arg1);
        }
        public void DoExitUnit(BaseUnit arg1)
        {
            OnExitUnit(arg1);
            //ButtonExitJob = Util.Invoke(() => ,0.02f);
        }
        #endregion

        #region Callback
        protected virtual void OnControlClick(UControl arg1, PointerEventData arg2)
        {
            SelectUnitTimer.Restart();
        }
        protected virtual void OnInputMove(Vector3 mousePosition, int i) { }
        protected virtual void OnInputUp(Vector3 mousePosition, int i)
        {
            if (IsLongPressTime())
                return;
            if (IsStayInUI)
            {
                BaseUIMgr.CloseRecordControl();
                SelectUnitTimer.Restart();
                return;
            }

            var picked = Util.PickColliderCom(SelectUnitLayerMask);
            {
                LastHitUpCollider = picked;
                BaseUnit final = null;
                if (IsSameLastHitCollider())
                {
                    var cols = Util.OverlapSphereCom(LastHitUpCollider.transform.position, OverlapRadius, SelectUnitLayerMask);
                    if (cols != null)
                    {
                        foreach (var item in cols)
                            LeftSelectUnits.Add(item.GetComponent<BaseUnit>());
                    }
                    //过滤
                    LeftSelectUnits = LeftSelectUnits.FindAll((x) => OnInputUpFilter(x)).ToList();
                    if (LeftSelectUnits.Count <= 0)
                    {
                        final = null;
                    }
                    else
                    {
                        if (LeftSelectFilterIndex < LeftSelectUnits.Count) { }
                        else { LeftSelectFilterIndex = 0; }

                        final = LeftSelectUnits[LeftSelectFilterIndex];
                        LeftSelectFilterIndex++;
                    }
                }
                if (OnManualPickedClick(i, LeftSelectUnits))
                {
                    //do nothing
                }
                else
                {
                    ManualTriggerClick(i, final);
                }
            }
        }
        //用户自定义单选函数
        protected virtual bool OnManualPickedClick(int index, List<BaseUnit> units)
        {
            return false;
        }
        //用户触发点击函数
        protected void ManualTriggerClick(int index, BaseUnit unit,bool isForce=false)
        {
            if (index == 1)
            {
                RightClick(unit, isForce);
                RightUp(unit, isForce);
            }
            else if (index == 0)
            {
                LeftClick(unit, isForce);
                LeftUp(unit, isForce);
            }
        }
        //单选择过滤，只有己方的军团可以被选择，所有的城市均可以被选择
        protected virtual bool OnInputUpFilter(BaseUnit unit)
        {
            if (BasePlayer == null)
                return false;
            return BasePlayer.IsSelf(unit);
        }
        protected virtual void OnInputDown(Vector3 mousePosition, int i)
        {
            LeftSelectUnits.Clear();
            IsLastHitUI = IsStayInUI = CheckOverUI();
            var picked = Util.PickColliderCom(SelectUnitLayerMask);
            {
                if (IsStayInUI)
                    return;
                LastHitCollider = picked;
                BaseUnit tempUnit = null;
                if (LastHitCollider != null)
                    tempUnit = LastHitCollider.GetComponent<BaseUnit>();
                if (i == 1)//右键
                {
                    RightDown(tempUnit);
                }
                else if (i == 0) 
                {
                    LeftDown(tempUnit);
                }
            }
        }
        protected virtual void OnEnterTerrain(Vector3 point) { }
        protected virtual void OnExitTerrain() { }
        protected virtual void OnEnterUI() { }
        protected virtual void OnExitUI() { }
        protected virtual void OnSelectedUnit(BaseUnit arg1, bool repeat) => Callback_OnSelectedUnit?.Invoke(arg1, repeat);
        protected virtual void OnRealDeathG(BaseUnit arg1) { }
        protected virtual void OnDeathG(BaseUnit arg1)
        {
            if (SelectedUnit == arg1)
            {
                SingleSelectUnit(null);
            }
        }
        //当框选所有的单位
        protected virtual void OnGroupSelectUnit(HashList<BaseUnit> list)
        {
            OnGenerateSelectUnit();
        }
        //Ctrl+鼠标 单选某个单位的时候触发
        protected virtual void OnGroupToggleUnit(bool isToggle,BaseUnit unit)
        {
            OnGenerateSelectUnit();
        }
        //单选单位,鼠标单选某个单位的时候触发
        protected virtual void OnSingleSelectUnit(BaseUnit unit)
        {
            OnGenerateSelectUnit();
        }
        //当没有选择任何单位的时候
        protected virtual void OnUnSelectUnit()
        {
        }
        //宽泛的选择单位的时候
        protected virtual void OnGenerateSelectUnit()
        { 
        
        }
        #endregion

        #region check util
        //选择单位之后的时间
        private static bool IsInSelectUnitTime() => !SelectUnitTimer.IsOver();
        // 检测鼠标是否悬在在ui上
        // isIgnoreHUD 如果是true 如果 检测到了hud 则 返回false 表示鼠标不悬浮在UI上
        private static bool CheckOverUI(bool isIgnoreHUD = false)
        {
            if (IsFullScreen) return true;
            if (EventSystem.current == null) return false;

            if (isIgnoreHUD)
            {
                bool isOver;
                if (Application.isMobilePlatform)
                {
                    if (Input.touchCount <= 0) return false;
                    isOver = EventSystem.current.IsPointerOverGameObject(FingerId);
                }
                else isOver = EventSystem.current.IsPointerOverGameObject();

                if (isOver)
                {
                    if (IsStayHUDBar) return false;
                    return true;
                }
                return false;
            }
            else
            {
                if (Application.isMobilePlatform)
                {
                    if (Input.touchCount <= 0) return false;
                    return EventSystem.current.IsPointerOverGameObject(FingerId);
                }
                else return EventSystem.current.IsPointerOverGameObject();
            }
        }
        #endregion

        #region Old Input
        public static bool IsAnyKey() => Input.anyKey || GetMouse(0, false) || GetTouchDown(false);
        public static bool IsAnyKeyDown() => Input.anyKeyDown || GetMouseDown(0,false) || GetTouchDown(false);
        public static bool GetMouseDown(int index, bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            return Input.GetMouseButtonDown(index);
        }
        public static bool GetMouseUp(int index, bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            return Input.GetMouseButtonUp(index);
        }
        public static bool GetMouse(int index, bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            return Input.GetMouseButton(index);
        }
        public static bool GetMouseClick(int index, bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            return IsSameMousePt() && Input.GetMouseButtonUp(index);
        }
        public static bool GetMousePress(int index, float duration = 0.5f, bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            if (Input.GetMouseButton(index) && IsSameMousePt())
            {
                if (LongPressTime > duration)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool GetKeyDown(KeyCode keyCode) => Input.GetKeyDown(keyCode);
        public static bool GetKeyUp(KeyCode keyCode) => Input.GetKeyUp(keyCode);
        public static bool GetKey(KeyCode keyCode) => Input.GetKey(keyCode);
        #endregion

        #region Old Touch
        static public int FingerId => Input.GetTouch(0).fingerId;
        static public Vector2 TouchDragValue { get; private set; }
        static public float TouchScaleValue { get; private set; }
        static public float TouchRotateValue { get; private set; }
        static public Vector3 TouchPosition
        {
            get
            {
                if (Input.touchCount > 0) 
                    return Input.touches[0].position;
                return Vector2.zero;
            }
        }
        static public bool GetTouchStationary(bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            if (Input.touchCount != 1) return false;
            else if (Input.touches[0].phase == UnityEngine.TouchPhase.Stationary) return true;
            return false;
        }
        static public bool GetTouchDown(bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            if (Input.touchCount != 1) return false;
            else if (Input.touches[0].phase == UnityEngine.TouchPhase.Began) return true;
            return false;
        }
        static public bool GetTouchUp(bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            if (Input.touchCount != 1) return false;
            else if (Input.touches[0].phase == UnityEngine.TouchPhase.Ended) return true;
            return false;
        }
        public static bool GetTouchPress(float duration=0.5f, bool checkUI = true)
        {
            if ((IsStayInUI) && checkUI) return false;
            if (GetTouchStationary() && IsSameTouchPt())
            {
                if (LongPressTime > duration)
                    return true;
            }
            return false;
        }
        public static bool GetTouchCount(int count)
        {
            if (count <= 0)
                return false;
            return Input.touchCount >= count;
        }
        #endregion

        #region Mouse & Touch
        public static Vector2 ScreenPos
        {
            get
            {
                if (Application.isMobilePlatform)
                    return TouchPosition;
                else return Input.mousePosition;
            }
        }
        public static Vector2 LastScreenDownPos
        {
            get
            {
                if (Application.isMobilePlatform)
                    return LastTouchDownPos;
                else return LastMouseDownPos;
            }
        }
        public static bool GetStationary(bool checkUI = true) => GetMouse(0, checkUI) || GetMouse(1, checkUI) || GetTouchStationary(checkUI);
        public static bool GetDown(bool checkUI = true) => GetMouseDown(0, checkUI) || GetMouseDown(1, checkUI) || GetTouchDown(checkUI);
        public static bool GetUp(bool checkUI = true) => GetMouseUp(0, checkUI) || GetMouseUp(1, checkUI) || GetTouchUp(checkUI);
        public static bool GetPress(float duration=SysConst.LongPressDuration, bool checkUI = true) => GetMousePress(0,duration, checkUI) || GetMousePress(1, duration, checkUI) || GetTouchPress(duration, checkUI);
        public static bool GetClick(bool checkUI = true) => (IsSamePt(0) || IsSamePt(1)) && GetUp(checkUI) && !IsLongPressTime(0.2f);
        #endregion
    }

}