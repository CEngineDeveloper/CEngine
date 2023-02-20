using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Invoke;
using System;

namespace CYM.UI
{
    #region data class
    public enum ViewLevel
    {
        Root,//根界面
        Main,//主界面
        Sub,//副界面
    }
    public enum ViewGroup
    {
        None = 0,
        Main = 1,
        View1 = 2,
        View2 = 3,
        View3 = 4,
        SideTip = 10,
        Top=99,
    }
    [System.Serializable]
    public class ShowAnim
    {
        public bool IsReset = true;
        public Ease InEase = Ease.OutBack;
        public Ease OutEase = Ease.InBack;
    }
    [System.Serializable]
    public class ShowAnimMove : ShowAnim
    {
        public Vector2 StartPos = Vector2.zero;
    }
    #endregion

    /// <summary>
    /// OnOpen & OnClose
    /// OnOpenSubView & OnCloseSubView
    /// OnShow
    /// OnFadeIn & OnFadeOut
    /// </summary>
    [HideMonoScript][DisallowMultipleComponent]
    public class UView : BaseCoreMono, IUIDirty, IDragHandler, IPointerDownHandler
    {
        #region Callback Value
        public Callback<UView, bool> Callback_OnClose { get; set; }
        public Callback<UView, bool> Callback_OnOpen { get; set; }
        public Callback<bool> Callback_OnShow { get; set; }
        #endregion

        #region Inspector
        [FoldoutGroup("ViewGroup"), SerializeField, HideIf("Inspector_HideGroup")]// 0意味着不在任何的组里面
        public ViewGroup Group = ViewGroup.None;
        [FoldoutGroup("ViewGroup"), SerializeField, HideIf("Inspector_HideIsExclusive")]// 是否互斥
        public bool IsExclusive = false;
        [FoldoutGroup("ViewGroup"), SerializeField, HideIf("Inspector_HideIsReturn")]// 当前界面关闭后,是否返回上次的互斥界面
        public bool IsReturn = false;
        [FoldoutGroup("ViewGroup"), SerializeField, HideIf("Inspector_HideIsReturn")]// 是否只和自己所在的UIMgr互斥
        public bool IsMutextAll = false;

        [FoldoutGroup("FullScreen"), SerializeField]// UI界面是否为全屏
        protected bool IsFullScreen = false;
        [FoldoutGroup("FullScreen"), SerializeField, HideIf("Inspector_HideAddBlocker")]// UI界面是否自动添加UIBlocker
        protected bool IsAddBlocker = true;
        [FoldoutGroup("FullScreen"), SerializeField, HideIf("Inspector_HideIsBlockerClose")]// UI界面是否自动添加UIBlocker
        protected bool IsBlockerClose = false;

        [FoldoutGroup("Prop"), SerializeField]// 默认是打开还是显示
        protected bool IsShowDefault = false;
        [FoldoutGroup("Prop"), SerializeField]// GO 是否根据界面IsShow变量 自动Active
        protected bool IsActiveByShow = true;
        [FoldoutGroup("Prop"), SerializeField]// 是否可以拖动
        public bool IsDragable = false;
        [FoldoutGroup("Prop"), SerializeField]// 是否记录UI界面到最近打开的界面列表,一般可以用于逐个关闭(比如按下ESC,关闭当前界面)
        protected bool IsRecord = false;

        [FoldoutGroup("Show"), SerializeField]// 是否重置效果
        protected bool IsReset = false;
        [FoldoutGroup("Show"), SerializeField]// 打开和关闭的UI时间是否一样
        protected bool IsSameTime = true;
        [FoldoutGroup("Show"), SerializeField]// 是否缩放
        protected bool IsScale = false;
        [FoldoutGroup("Show"), SerializeField]// 是否移动
        protected bool IsMove = false;

        [FoldoutGroup("Show"), HideIf("Inspector_ShowTime"), SerializeField]// 打开特效持续时间
        protected float Duration = 0.3f;
        [FoldoutGroup("Show"), HideIf("Inspector_IsSameTime"), SerializeField]// 打开界面时间
        protected float InTime = 0.3f;
        [FoldoutGroup("Show"), HideIf("Inspector_IsSameTime"), SerializeField]// 关闭界面时间
        protected float OutTime = 0.3f;

        [FoldoutGroup("Show"), HideIf("Inspector_HideScale"), SerializeField]
        protected ShowAnim TweenScale = new ShowAnim();
        [FoldoutGroup("Show"), HideIf("Inspector_HidePos"), SerializeField]
        protected ShowAnimMove TweenMove = new ShowAnimMove();
        #endregion

        #region pub is
        public bool IsInShowDefault { get; private set; } = false;
        public bool IsDragged { get; protected set; } = false;
        public bool IsShow { get; protected set; }
        public bool IsCompleteShow { get; protected set; }
        public bool IsCompleteClose { get; protected set; }
        public bool IsFirstEnabled { get; protected set; } = false;
        // 是否为根界面
        public bool IsRootView => ViewLevel == ViewLevel.Root;
        #endregion

        #region pub val
        public int ShowCount { get; protected set; } = 0;
        public float HUDItemOffset { get; protected set; } = 0.0f;
        #endregion

        #region Root View Componet
        //安全RootView
        public static UView GoldRootView { get; private set; }
        Canvas _canvas;
        CanvasScaler _canvasScaler;
        GraphicRaycaster _graphicRaycaster;
        RectTransform _canvasTrans;
        public Canvas Canvas => _canvas ?? (RootView ?? GoldRootView)._canvas;
        public CanvasScaler CanvasScaler => _canvasScaler ?? (RootView ?? GoldRootView)._canvasScaler;
        public GraphicRaycaster GraphicRaycaster => _graphicRaycaster ?? (RootView ?? GoldRootView)._graphicRaycaster;
        public RectTransform CanvasTrans => _canvasTrans ?? (RootView ?? GoldRootView)._canvasTrans;
        public Camera WorldCamera => Canvas.worldCamera;
        #endregion

        #region 公共属性
        // 界面等级
        public ViewLevel ViewLevel { get; set; } = ViewLevel.Main;
        public RectTransform RectTrans { get; private set; }
        // 子界面的集合
        public HashList<UView> SubViews { get; private set; } = new HashList<UView>();
        // 界面所在的UI管理器
        public BaseUIMgr UIMgr { get; set; }
        public UView ParentView { get; private set; }
        public UView RootView { get; private set; }
        public HashSet<UView> MutexPreviews { get; protected set; } = new HashSet<UView>();
        #endregion

        #region 内部    
        //UImage Blocker;
        RectTransform ParentRectTrans;
        private IJob closeInvoke;
        protected Tweener alphaTween;
        protected Tweener scaleTween;
        protected Tweener moveTween;
        protected Vector3 sourceLocalPos;
        protected float Delay { get; private set; } = -1;
        protected float ShowTime { get; private set; } = -1;
        List<UView> DirtyAllWhenClose = new List<UView>();
        protected Corouter CommonCoroutine => BaseGlobal.CommonCorouter;
        protected Corouter MainUICoroutine => BaseGlobal.MainUICorouter;
        protected Corouter BattleCoroutine => BaseGlobal.BattleCorouter;
        protected List<DirtyAction> DirtyActions = new List<DirtyAction>();
        #endregion

        #region life
        protected virtual string FocusClip => "AppTab";
        public override MonoType MonoType => MonoType.View;
        public override LayerData LayerData => SysConst.Layer_UI;
#if UNITY_EDITOR
        void OnValidate()
        {
            var temp_canvas = GetComponent<Canvas>();
            var temp_canvasScaler = GetComponent<CanvasScaler>();
            var temp_graphicRaycaster = GetComponent<GraphicRaycaster>();
            var temp_radioMatch = GetComponent<UICanvasMatch>();
            if (temp_canvas != null)
                temp_canvas.hideFlags = HideFlags.NotEditable;
            if (temp_canvasScaler != null)
                temp_canvasScaler.hideFlags = HideFlags.NotEditable;
            if (temp_graphicRaycaster != null)
                temp_graphicRaycaster.hideFlags = HideFlags.NotEditable;
            if (temp_radioMatch != null)
                temp_radioMatch.hideFlags = HideFlags.NotEditable;

            if (temp_canvas != null &&
                temp_canvasScaler != null &&
                temp_graphicRaycaster != null &&
                temp_radioMatch != null)
            {
                this.hideFlags = HideFlags.NotEditable;
            }
        }
#endif
        public override void Awake()
        {
            base.Awake();
            _canvas = GetComponentInChildren<Canvas>();
            _canvasScaler = GetComponentInChildren<CanvasScaler>();
            _graphicRaycaster = GetComponentInChildren<GraphicRaycaster>();
            if (_canvas != null)
            {
                _canvasTrans = _canvas.transform as RectTransform;              
            }
            if (_canvasScaler != null)
            {
                _canvasScaler.referenceResolution = new Vector2(UIConfig.Ins.Width,UIConfig.Ins.Height);
            }
            DirtyActions.Clear();
            RectTrans = GetComponent<RectTransform>();
            sourceLocalPos = Trans.localPosition;
            UIUtil.CreateFullscreenBG(Trans, IsFullScreen, IsAddBlocker, IsBlockerClose, UIConfig.Ins.BlockerBGColor, () => Close());
            if (!IsExclusive)
                IsReturn = false;
            if (BaseGlobal.ScreenMgr != null)
            {
                BaseGlobal.ScreenMgr.Callback_OnSetPlayer += OnSetPlayer;
            }
        }
        public override void OnDestroy()
        {
            if (BaseGlobal.ScreenMgr != null)
            {
                BaseGlobal.ScreenMgr.Callback_OnSetPlayer -= OnSetPlayer;
            }
            if (IsRecord) BaseUIMgr.RemoveRecordView(this);
            if (IsShow && IsFullScreen) BaseInputMgr.PushFullScreenState(false);
            base.OnDestroy();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (!IsFirstEnabled)
            {
                IsFirstEnabled = true;
            }
        }
        public override void Start()
        {
            base.Start();
            FetchSubjects();
            OnCreatedView();
            SetShowTime(0);
            ShowDefault();
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
            NeedUpdate = true;
        }
        protected virtual void OnCreatedView()
        {
            if (!IsRootView)
                ParentRectTrans = RectTrans.parent as RectTransform;
            //绑定事件
            if(BaseGlobal.LangMgr!=null)
                BaseGlobal.LangMgr.Callback_OnSwitchLanguage += OnSwitchLanguage;
        }
        public virtual void Attach(ViewLevel viewLevel, UView beAttchedView)
        {
            UIMgr = beAttchedView?.UIMgr;
            ParentView?.SubViews.Remove(this);
            if (viewLevel == ViewLevel.Root)
            {
                if (GoldRootView == null)
                    GoldRootView = beAttchedView;
                ParentView = beAttchedView;
                RootView = beAttchedView;
                ViewLevel = ViewLevel.Main;
                RectTrans.SetParent(RootView?.CanvasTrans);
                UIMgr.AddToMainViews(this);
                UIMgr.AddToGroupViews(this);
            }
            else if (viewLevel == ViewLevel.Main)
            {
                ParentView = beAttchedView;
                RootView = beAttchedView.RootView;
                ViewLevel = ViewLevel.Sub;
                RectTrans.SetParent(RootView?.CanvasTrans);
                UIMgr.AddToGroupViews(this);
                //移动到父节点下面
                Trans.SetSiblingIndex(ParentView.Trans.GetSiblingIndex() + ParentView.SubViews.Count + 1);
            }
            else
                CLog.Error("无法挂载到:" + viewLevel);
            ParentView.SubViews.Add(this);
        }
        // 销毁UI
        public virtual void DoDestroy()
        {
            if (BaseGlobal.LangMgr != null)
                BaseGlobal.LangMgr.Callback_OnSwitchLanguage -= OnSwitchLanguage;
            foreach (var item in SubViews)
                item.DoDestroy();
            Destroy(GO);
            Callback_OnClose = null;
            Callback_OnOpen = null;
        }
        // 获取控件
        protected virtual void FetchSubjects() { }
        // 创建副界面
        protected virtual T CreateSubView<T>(string path) where T : UView
        {
            if (ViewLevel == ViewLevel.Sub)
            {
                CLog.Error("错误! 副界面无法再次创建副界面");
                return null;
            }
            T tempUI = UIMgr.CreateUIGO<T>(path);
            tempUI.Attach(ViewLevel.Main, this);
            tempUI.Callback_OnOpen += OnOpenSubView;
            tempUI.Callback_OnClose += OnCloseSubView;
            return tempUI;
        }
        #endregion

        #region Seald
        public sealed override void OnTurnbase(bool day, bool month, bool year)
        {
            base.OnTurnbase(day, month, year);
        }
        public sealed override void OnTurnframe(int gameFramesPerSecond)
        {
            base.OnTurnframe(gameFramesPerSecond);
        }
        public sealed override void OnAnimTrigger(int param)
        {
            base.OnAnimTrigger(param);
        }
        protected sealed override void OnAttachComponet()
        {
            base.OnAttachComponet();
        }
        #endregion

        #region show
        // 打开界面,关联一个父界面,当父界面关闭的时候自身也会关闭
        public void Show(UView parentView)
        {
            Attach(ViewLevel.Main, parentView);
            Show(true, false, true);
        }
        // 回到默认的开启状态
        public void ShowDefault()
        {
            IsInShowDefault = true;
            if (IsRootView) Show(true, false, true);
            if (!IsShowDefault) Show(false, false, true);
            else Show(true, false, true);
            IsInShowDefault = false;
        }
        public void ShowDirect(bool b, bool force = false)
        {
            Show(b,false, force);
        }
        public void ShowOrDirtyAll()
        {
            if (!IsShow)
                Show();
            else
                SetDirtyAll();
        }
        // 显示或者关闭界面
        public virtual void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            closeInvoke?.Kill();
            IsCompleteClose = false;
            IsCompleteShow = false;
            ShowCount++;
            IsShow = b;
        }
        public void Toggle() => Show(!IsShow);
        public void Close(float delay = 0)
        {
            if (!IsShow) return;
            delay = Mathf.Clamp(delay, 0, float.MaxValue);
            closeInvoke?.Kill();
            if (delay == 0) Show(false);
            else closeInvoke = Util.Invoke(() => Show(false), delay);
        }
        #endregion

        #region set
        //设置这个界面关闭的时候,需要关联刷新的界面
        public void SetDirtyAllWhenClose(params UView[] views)
        {
            if (views == null) return;
            DirtyAllWhenClose = new List<UView>(views);
        }
        public void SetDelay(float delay) => Delay = delay;
        public void SetShowTime(float showTime) => ShowTime = showTime;
        public bool SetFocus()
        {
            if (UIMgr == null)
                return false;
            //将主界面等级并且有组的界面设置焦点
            if (Group != ViewGroup.None)
            {
                if (!UIMgr.GroupViews.ContainsKey(Group)) return false;
                List<UView> views = UIMgr.GroupViews[Group].OrderBy((x) => x.RectTrans.GetSiblingIndex()).ToList();
                if (views.Count == 0) return false;
                int newIndex = views[views.Count - 1].RectTrans.GetSiblingIndex();
                if (newIndex == RectTrans.GetSiblingIndex()) return false;
                RectTrans.SetSiblingIndex(newIndex);
                SetDirtyAll();
                return true;
            }
            return false;
        }
        public DirtyAction CreateDirtyAction(Callback action)
        {
            var temp = new DirtyAction(action);
            DirtyActions.Add(temp);
            return temp;
        }
        #endregion

        #region get
        // 获得翻译
        protected static string GetStr(string key, params object[] objs) => BaseLangMgr.Get(key, objs);
        protected static string JointStr(string key, params object[] objs) => BaseLangMgr.Joint(key,objs);
        #endregion

        #region other
        public virtual void Enable(bool b) => IsEnable = b;
        public virtual void Interactable(bool b) { }
        protected AudioSource PlayClip(string id, bool isLoop = false) => BaseGlobal.AudioMgr.PlayUI(id, isLoop);
        protected void PlayClips(string[] id) => BaseGlobal.AudioMgr.PlayUI(RandUtil.RandArray(id), false);
        #endregion

        #region dirty
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (IsDirtyShow) RefreshShow();
            if (IsDirtyData) RefreshData();
            if (IsDirtyRefresh) Refresh();
            if (IsDirtyCell) RefreshCell();
            foreach (var item in DirtyActions)
                item.OnUpdate();
        }
        public bool IsDirtyRefresh { get; protected set; } = false;
        public bool IsDirtyCell { get; protected set; } = false;
        public bool IsDirtyData { get; protected set; } = false;
        public bool IsDirtyShow { get; protected set; } = false;
        public virtual void SetDirtyRefresh()
        {
            if (!NeedFixedUpdate)
                CLog.Error("{0}:没有设置NeedFixedUpdate 导致 SetDirty无法生效", GOName);
            IsDirtyRefresh = true;
        }
        public virtual void SetDirtyCell()
        {
            if (!NeedFixedUpdate)
                CLog.Error("{0}:没有设置NeedFixedUpdate 导致 SetDirty无法生效", GOName);
            IsDirtyCell = true;
        }
        public virtual void SetDirtyData()
        {
            if (!NeedFixedUpdate)
                CLog.Error("{0}:没有设置NeedFixedUpdate 导致 SetDirtyData无法生效", GOName);
            IsDirtyData = true;
            IsDirtyRefresh = true;
            IsDirtyCell = true;
        }
        public virtual void SetDirtyShow()
        {
            if (!NeedFixedUpdate)
                CLog.Error("{0}:没有设置NeedFixedUpdate 导致 SetDirtyShow无法生效", GOName);
            IsDirtyShow = true;
            IsDirtyRefresh = true;
        }
        public virtual void SetDirtyAll()
        {
            if (!NeedFixedUpdate)
                CLog.Error("{0}:没有设置NeedFixedUpdate 导致 SetDirtyData无法生效", GOName);
            IsDirtyShow = true;
            IsDirtyData = true;
            IsDirtyRefresh = true;
            IsDirtyCell = true;
        }
        public void SetDirtyAll(float delay)
        {
            Util.Invoke(() => SetDirtyAll(), delay);
        }
        public virtual void Refresh() => IsDirtyRefresh = false;
        public virtual void RefreshCell() => IsDirtyCell = false;
        public virtual void RefreshData() => IsDirtyData = false;
        public virtual void RefreshShow() => IsDirtyShow = false;
        public virtual void RefreshAll()
        {
            IsDirtyRefresh = false;
            IsDirtyData = false;
            IsDirtyShow = false;
        }
        #endregion

        #region Inspector
        [Button("AutoSetup")]
        public virtual void AutoSetup()
        {
        }
        bool Inspector_HidePos()=> !IsMove;
        bool Inspector_HideScale()=> !IsScale;
        bool Inspector_IsSameTime()=> IsSameTime;
        bool Inspector_ShowTime() => !IsSameTime;
        protected virtual bool Inspector_HideGroup()=> false;
        protected virtual bool Inspector_HideIsExclusive()=> Group == ViewGroup.None;
        protected virtual bool Inspector_HideIsReturn()=> Group == ViewGroup.None || !IsExclusive;
        protected virtual bool Inspector_HideAddBlocker()=> !IsFullScreen;
        protected virtual bool Inspector_HideBlockerCol()=> !IsAddBlocker || !IsFullScreen;
        protected virtual bool Inspector_HideIsBlockerClose()=> !IsAddBlocker || !IsFullScreen;
        #endregion

        #region open & close
        protected virtual void OnFadeIn() 
        {
            IsCompleteShow = true;
        }
        protected virtual void OnFadeOut()
        {
            if (GO == null) return;
            if (IsActiveByShow) GO.SetActive(false);
            IsCompleteClose = true;
        }
        protected virtual void OnSwitchLanguage()
        {
            SetDirtyRefresh();
        }
        // 显示状态发生变化的时候调用
        protected virtual void OnShow()
        {
            Delay = -1;
            ShowTime = -1;
            Callback_OnShow?.Invoke(IsShow);
            if (IsFullScreen) BaseInputMgr.PushFullScreenState(IsShow);
        }
        protected virtual void OnOpenDelay(UView baseView, bool useGroup)
        { 
        
        }
        protected virtual void OnOpen(UView baseView, bool useGroup)
        {
            if (GO == null) return;
            if (IsActiveByShow) GO.SetActive(true);
            if (IsRecord) BaseUIMgr.AddRecordView(this);
            IsDragged = false;
            Callback_OnOpen?.Invoke(baseView, useGroup);
        }
        protected virtual void OnClose(bool useGroup)
        {
            if (IsRecord) BaseUIMgr.RemoveRecordView(this);
            if (ParentView != null)
            {
                //关闭界面的时候自动刷新父级界面
                if (ParentView.IsShow && !ParentView.IsRootView)
                    ParentView.SetDirtyAll();
            }
            if (DirtyAllWhenClose.Count > 0)
            {
                //关闭界面的时候自动刷新关联界面
                foreach (var item in DirtyAllWhenClose)
                    item.SetDirtyAll();
                DirtyAllWhenClose.Clear();
            }
            //不是RootView的话就关闭所有子界面
            if (!IsRootView)
            {
                foreach (var item in SubViews)
                    item.Show(false);
            }
            Callback_OnClose?.Invoke(this, useGroup);
        }
        void OnOpenSubView(UView view, bool useGroup)
        {
            //子界面UI互斥,相同UI组只能有一个UI被打开
            if (useGroup &&
                view.Group > 0 &&
                view.ViewLevel == ViewLevel.Sub)
            {
                view.MutexPreviews.Clear();
                foreach (var item in SubViews)
                {
                    if (
                        !item.IsDragged &&
                        item != view &&
                        item.Group != ViewGroup.None &&
                        item.Group == view.Group &&
                        item.ViewLevel == view.ViewLevel &&
                        item.IsExclusive && view.IsExclusive &&
                        item.IsShow &&
                        item.RootView == view.RootView)
                    {
                        view.MutexPreviews.Add(item); 
                        item.Show(false, false);
                    }
                }
            }
        }
        void OnCloseSubView(UView view, bool useGroup)
        {
            if (useGroup && view.Group > 0)
            {
                if (view.IsReturn)
                {
                    foreach (var item in view.MutexPreviews)
                    {
                        if (item.RootView == view.RootView)
                            item.Show(true, false);
                    }
                }
            }
        }
        #endregion

        #region Callback
        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsDragable)
            {
                SetFocus();
                PlayClip(FocusClip);
            }
        }
        public void OnDrag(PointerEventData data)
        {
            if (IsDragable)
            {
                //if (GO != BaseInputMgr.LastHitUI) return;
                if (RectTrans == null || ParentRectTrans == null) return;
                RectTrans.Translate(data.delta);
                ClampToWindow();
                IsDragged = true;
                // 限制当前面板在父节点中的区域位置  
            }
        }
        protected void ClampToWindow()
        {
            // 面板位置  
            Vector3 pos = RectTrans.localPosition;
            // 如果是UI父节点，设置面板大小为0，那么最大最小位置为正负屏幕的一半  
            Vector3 minPosition = ParentRectTrans.rect.min - RectTrans.rect.min;
            Vector3 maxPosition = ParentRectTrans.rect.max - RectTrans.rect.max;
            pos.x = Mathf.Clamp(RectTrans.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(RectTrans.localPosition.y, minPosition.y, maxPosition.y);
            RectTrans.localPosition = pos;
        }
        protected virtual void OnSetPlayer(BaseUnit arg1, BaseUnit arg2)
        {
            if (IsShow)
            {
                SetDirtyAll(0.1f);
            }
        }
        #endregion
    }

}