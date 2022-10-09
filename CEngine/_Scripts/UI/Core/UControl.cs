using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public enum MutexGroup
    {
        None = 0,

        Button1 = 1001,
        Button2 = 1002,
        Button3 = 1003,
        Button4 = 1004,

        Container1 = 2001,
        Container2 = 2002,
        Container3 = 2003,
        Container4 = 2004,

        Panel1 = 3001,
        Panel2 = 3002,
        Panel3 = 3003,
        Panel4 = 3004,
    }
    //默认显示类型
    public enum DefaultShowType
    { 
        None,   //使用Active
        Show,   //默认显示
        Close,  //默认关闭
    }
    [AddComponentMenu("UI/Control/UControl")]
    [HideMonoScript]
    [DisallowMultipleComponent]
    public class UControl : UIBehaviour, IUIDirty
    {
        #region Callback Val
        public Callback<bool> Callback_OnShow { get; set; }
        public Callback<bool> Callback_OnShowComplete { get; set; }
        #endregion

        #region parent
        public UPanel PPanel { get; set; }
        public UMutexer PMutexer { get; set; }
        public UContainer PContainer { get; set; }
        public UDupplicate PDupplicate { get; set; }
        public UScroll PScroll { get; set; }
        public UControl Parent { get; set; }
        public UUIView PUIView
        {
            get
            {
                if (Parent == null) return puiview;
                return Parent.PUIView;
            }
            set
            {
                puiview = value;
            }
        }
        private UUIView puiview;
        public BaseUIMgr UIMgr => PUIView?.UIMgr;
        #endregion

        #region child
        [PropertyOrder(-1), ReadOnly, ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        protected HashList<UControl> StaticChilds { get; set; } = new HashList<UControl>();
        [PropertyOrder(-1), ReadOnly, ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        protected HashList<UControl> CellChilds { get; set; } = new HashList<UControl>();
        #endregion

        #region vector
        public Vector3 SourceAnchoredPosition3D { get; protected set; }
        public Vector3 SourceLocalScale { get; protected set; }
        public Vector2 SourceAnchorMax { get; protected set; }
        public Vector2 SourceAnchorMin { get; protected set; }
        public Vector2 SourceAnchoredPosition { get; protected set; }
        public Vector2 SourceSizeData { get; protected set; }
        #endregion

        #region Prop
        //调用Show函数的次数
        public int ShowCount { get; private set; } = 0;
        public bool IsShowDefault { get; private set; } = true;
        public string ACTip { get; protected set; }
        //mgr
        protected List<UIShow> EffectShowTrans = new List<UIShow>();
        public LayoutGroup LayoutGroup { get; protected set; }
        public LayoutElement LayoutElement { get;private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public virtual float GroupAlpha
        {
            get
            {
                if (CanvasGroup)
                    return CanvasGroup.alpha;
                return 0;
            }
            set
            {
                if (CanvasGroup)
                    CanvasGroup.alpha = value;
            }
        }
        public UIShow MainEffectShowTrans
        {
            get
            {
                if (EffectShowTrans == null || EffectShowTrans.Count <= 0) 
                    return null;
                return EffectShowTrans[0];
            }
        }
        #endregion

        #region Inspector
        [FoldoutGroup("Base"), SerializeField,Tooltip("是否忽略Blocker")]
        public bool IsIgnoreBlockClick = false;
        [FoldoutGroup("Base"), SerializeField]// 是否记录UI界面到最近打开的界面列表,一般可以用于逐个关闭(比如按下ESC,关闭当前界面)
        protected bool IsRecord = false;
        [FoldoutGroup("Base"), SerializeField, Tooltip("是否Active通过打开/关闭")]
        public bool IsActiveByShow = true;
        [FoldoutGroup("Base"), SerializeField, Tooltip("默认显示类型"), PropertyOrder(-3)]
        public DefaultShowType ShowType = DefaultShowType.None;
        [FoldoutGroup("Base"), SerializeField, PropertyOrder(-3)]
        public MutexGroup MutexGroup = MutexGroup.None;

        [FoldoutGroup("Show"), SerializeField]
        public bool IsAlphaShow = false;
        [FoldoutGroup("Show"), SerializeField]
        public bool IsColorShow = false;
        [FoldoutGroup("Show"), SerializeField]
        public bool IsPosShow = false;
        [FoldoutGroup("Show"), SerializeField]
        public bool IsScaleShow = false;
        [FoldoutGroup("Show"), SerializeField]
        public bool IsSimpleShow = false;
        [FoldoutGroup("Show"), SerializeField, ShowIf("Inspector_AlphaShow")]
        public UIAlphaShow AlphaShow = new UIAlphaShow();
        [FoldoutGroup("Show"), SerializeField, ShowIf("Inspector_ColorShow")]
        public UIColorShow ColorShow = new UIColorShow();
        [FoldoutGroup("Show"), SerializeField, ShowIf("Inspector_PosShow")]
        public UIPosShow PosShow = new UIPosShow();
        [FoldoutGroup("Show"), SerializeField, ShowIf("Inspector_ScaleShow")]
        public UIScaleShow ScaleShow = new UIScaleShow();
        [FoldoutGroup("Show"), SerializeField, ShowIf("Inspector_AlphaSimpleShow")]
        public UIAlphaSimpleShow AlphaSimpleShow = new UIAlphaSimpleShow();
        #endregion

        #region life
        // 是否需要FixedUpdate
        public virtual bool NeedFixedUpdate { get; } = false;
        //是否需要Update
        public virtual bool NeedUpdate { get; } = false;
        protected override void Awake()
        {
            base.Awake();
            OnBeFetched();
        }
        public virtual void OnBeFetched()
        {
            if (IsBeFetched)
                return;
            IsBeFetched = true;
            if (IsAutoInit)
            {
                Init();
            }
            GO.layer = (int)SysConst.Layer_UI;
            SetDefaultShowType();
            AssignSource();
            InitShowEffect();
            ClearChilds();
            FetchSubControls();
            OnCreateControl();
            void AssignSource()
            {
                LayoutGroup = GetComponent<LayoutGroup>();
                LayoutElement = GetComponent<LayoutElement>();
                CanvasGroup = GetComponent<CanvasGroup>();
                SourceAnchorMax = RectTrans.anchorMax;
                SourceAnchorMin = RectTrans.anchorMin;
                SourceLocalScale = RectTrans.localScale;
                SourceSizeData = RectTrans.sizeDelta;
                SourceAnchoredPosition = RectTrans.anchoredPosition;
                SourceAnchoredPosition3D = RectTrans.anchoredPosition3D;
                if (RectTrans.localScale == Vector3.zero)
                    CLog.Error("错误:scale 是 0 error presenter:" + name);
            }
            void InitShowEffect()
            {
                EffectShowTrans.Clear();
                if (IsSimpleShow) EffectShowTrans.Add(AlphaSimpleShow);
                if (IsAlphaShow) EffectShowTrans.Add(AlphaShow);
                if (IsColorShow) EffectShowTrans.Add(ColorShow);
                if (IsPosShow) EffectShowTrans.Add(PosShow);
                if (IsScaleShow) EffectShowTrans.Add(ScaleShow);
                float fadeInDuration = 0;
                float fadeOutDuration = 0;
                UIShow fadeInShow = MainEffectShowTrans;
                UIShow fadeOutShow = MainEffectShowTrans;
                foreach (var item in EffectShowTrans)
                {
                    item.Init(this);
                    var curFadeInDuration = item.GetDuration(true);
                    if (curFadeInDuration > fadeInDuration)
                    {
                        fadeInShow = item;
                        fadeInDuration = curFadeInDuration;
                    }
                    var curFadeOutDuration = item.GetDuration(false);
                    if (curFadeOutDuration > fadeOutDuration)
                    {
                        fadeOutShow = item;
                        fadeOutDuration = curFadeOutDuration;
                    }
                }
                if (fadeInShow != null)
                    fadeInShow.SetFadeInCompleteCallback(OnFadeIn);
                if (fadeOutShow != null)
                    fadeOutShow.SetFadeOutCompleteCallback(OnFadeOut);
            }
        }

        public void SetDefaultShowType()
        {
            if (ShowType == DefaultShowType.None) IsShowDefault = GO.activeSelf;
            else if (ShowType == DefaultShowType.Show) IsShowDefault = true;
            else if (ShowType == DefaultShowType.Close) IsShowDefault = false;
            IsShow = IsShowDefault;
            GO.SetActive(IsShow);
        }

        protected virtual void FetchSubControls()
        {
            if (IsAtom) return;
            //作为顶级Control可以获取自身层级下面的子对象,子对象随着父对象自动刷新
            var childs = GO.GetComponentsInChildren<UControl>(true);
            if (childs != null)
            {
                foreach (var item in childs)
                {
                    if (item == this) continue;
                    item.OnBeFetched();
                    if (item.IsCanBeControlFetch)
                    {
                        AddStaticChild(item);
                    }
                }
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsFirstEnabled)
            {
                IsFirstEnabled = true;
            }
        }
        protected override void OnDestroy()
        {
            Cleanup();
            if (IsRecord) 
                BaseUIMgr.RemoveRecordControl(this);
            base.OnDestroy();
        }
        public virtual void OnFixedUpdate() { }
        public virtual void OnUpdate() { }
        protected virtual void OnCreateControl() { }
        #endregion

        #region Init
        public virtual void Init(Callback<UControl> onInit) => onInit?.Invoke(this);
        public virtual void Init()
        {
            if (IsInited)
            {
                CLog.Error("{0}:一个Presenter:{1} 不能初始化2次",PUIView.GOName,GOName);
                return;
            }
            IsInited = true;
        }
        public virtual void CancleInit()
        {
            if (!IsInited) return;
            IsInited = false;
        }
        #endregion

        #region data
        // 索引
        public int Index
        {
            get
            {
                if (Parent != null && !Parent.IsCollection)
                    return Parent.Index;
                return _index;
            }
        }
        int _index;
        // 数据索引
        public int DataIndex
        {
            get
            {
                if (Parent != null && !Parent.IsCollection)
                    return Parent.DataIndex;
                return _dataIndex;
            }
        }
        int _dataIndex;
        //运行时ID
        public long ID
        {
            get
            {
                if (Parent != null && !Parent.IsCollection)
                    return Parent.ID;
                return _id;
            }
        }
        long _id = 0;
        //UScroll的CustomData
        public object CustomData { get; private set; }
        #endregion

        #region is
        //是否被Fetched,每个控件只能被Fetch一次
        public bool IsBeFetched { get; private set; } = false;
        //是否第一次Enable
        public bool IsFirstEnabled { get; protected set; } = false;
        // 是否初始化
        public bool IsInited { get; protected set; }
        // 是否显示,默认为null,表示显示状态未知
        public bool IsShow { get; protected set; } = false;
        public bool IsShowComplete { get; private set; } = false;
        public bool IsClose => !IsShow;
        public bool IsActiveSelf => GO.activeSelf;
        public bool IsInteractable { get; protected set; } = true;
        public bool IsSelected { get; protected set; } = false;
        //是否播放动画完成了
        public bool IsTweenCoplete
        {
            get
            {
                if (MainEffectShowTrans==null) 
                    return true;
                return MainEffectShowTrans.IsComplete;
            }
        }
        //是否可以被自动刷新
        public bool IsCanAutoRefresh(bool needShow = true, bool needIndependent = true)
        {
            if (!IsInited) return false;
            if (needShow)
            {
                if (!IsShow) return false;
            }
            if (needIndependent)
            {
                if (!IsIndependent) return false;
            }
            return true;
        }
        #endregion

        #region override is
        public bool IsManualFetch { get; set; } = false;
        //是否可以被上层View获取,一般情况下只有独立控件才可以被UI获取
        public virtual bool IsCanBeViewFetch => IsIndependent && !IsManualFetch;
        //是否可以被上层父对象获取,一般情况下只有独立控件才可以被UI获取
        public virtual bool IsCanBeControlFetch => IsIndependent && !IsManualFetch;
        //是否自动初始化，自动调用Init()
        public virtual bool IsAutoInit => false;
        //是否为独立控件，还没有父对象的和没有互斥组的都属于独立组件
        public bool IsIndependent
        {
            get
            {
                if (PMutexer != null) return false;
                if (Parent != null) return false;
                return true;
            }
        }
        //是否为集合组件，目前用于标记，没有实际功能
        public virtual bool IsCollection => false;
        //是否为原子组件,原子组件不会获取子组件，类似UImage，UText
        public virtual bool IsAtom => false;
        #endregion

        #region set
        //重置位置数据
        public void ResetSourcePosData()
        {
            RectTrans.anchorMax = SourceAnchorMax;
            RectTrans.anchorMin = SourceAnchorMin;
            RectTrans.localScale = SourceLocalScale;
            RectTrans.sizeDelta = SourceSizeData;
            RectTrans.anchoredPosition = SourceAnchoredPosition;
            RectTrans.anchoredPosition3D = SourceAnchoredPosition3D;
        }
        //确保有LayoutElement
        public LayoutElement EnsureLayoutElement()
        {
            if (LayoutElement == null)
                LayoutElement = GO.AddComponent<LayoutElement>();
            return LayoutElement;
        }
        public void RebuildLayout()
        {
            if (LayoutGroup != null)
                LayoutGroup.RebuildLayout();
        }
        // 播放音效
        protected AudioSource PlayClip(string clip, bool isLoop = false)
        {
            if (clip.IsInv()) return null;
            return BaseGlobal.AudioMgr?.PlayUI(clip, isLoop);
        }
        public virtual void SetActive(bool b = true)
        {
            IsShow = b;
            if (GO.activeSelf == b) return;
            GO.SetActive(b);
        }
        // 闪烁
        public virtual void Blink()
        {
            //如果presenter 带了 UITranslate 那么等tween完了以后在设置SetActive
            if (EffectShowTrans.Count > 0)
            {
                foreach (var item in EffectShowTrans)
                    item.OnShow(true);
            }
        }
        public virtual void Close() => Show(false);
        // 显示默认状态
        public virtual void ShowDefault()
        {
            if (PMutexer != null) return;
            if (PScroll != null) return;
            ShowDirect(IsShowDefault);
        }
        public void ShowOrDirtyAll()
        {
            if (!IsShow)
                Show();
            else
                SetDirtyAll();
        }
        // 打开关闭:isForce 强制打开,并且刷新
        public virtual void Show(bool b = true, bool isForce = false)
        {
            if (PMutexer == null)
            {
                ShowDirect(b, isForce);
                return;
            }
            if (b) PMutexer.Show(this);
            else PMutexer.ShowDefault();
        }
        public void ShowDirect(bool b = true, bool isForce = false, bool isRefresh = true, bool isRefreshShow = true)
        {
            if (IsShow == b && 
                !isForce && 
                ShowCount>0)
            {
                return;
            }
            ShowCount++;
            IsShowComplete = false;
            IsShow = b;

            //根据需要刷新内容
            if (IsShow)
            {
                if (isRefreshShow)
                {
                    RefreshShow();
                }
                if (isRefresh)
                {
                    RefreshData();
                    Refresh();
                    RefreshCell();
                }
            }

            //如果presenter 带了 UITranslate 那么等tween完了以后在设置SetActive
            if (EffectShowTrans.Count > 0)
            {
                foreach (var item in EffectShowTrans)
                    item.OnShow(b);
            }
            //没有带presenter 直接设置 SetActive
            else
            {
                if (b) OnFadeIn();
                else OnFadeOut();
                SetActive(b);
            }
            //触发回调
            Callback_OnShow?.Invoke(IsShow);
            OnShow(IsShow);
            if (IsShow) OnOpen();
            else OnClose();
        }
        // 设置index
        public virtual void SetIndex(int i) => _index = i;
        public virtual void SetDataIndex(int i) => _dataIndex = i;
        public virtual void SetID(long id) => _id = id;
        public void SetCustomData(object customData)
        {
            CustomData = customData;
        }
        public virtual void Toggle() => Show(!IsShow);
        // 添加自对象
        public virtual bool AddStaticChild(UControl child)
        {
            if (child == this) return false;
            if (!child.IsIndependent)
            {
                CLog.Error("错误! 无法添非独立组件 ");
                return false;
            }
            child.Parent = this;
            StaticChilds.Add(child);
            return true;
        }
        public virtual bool AddCellChild(UControl child)
        {
            if (child == this) return false;
            if (!child.IsIndependent)
            {
                CLog.Error($"错误! 无法添非独立组件,Name:{GOName}");
                return false;
            }
            child.Parent = this;
            child.PScroll = this as UScroll;
            CellChilds.Add(child);
            return true;
        }
        public virtual void Cleanup()
        {
            IsInited = false;
            ClearChilds();
        }
        protected void ClearChilds()
        {
            StaticChilds.Clear();
            CellChilds.Clear();
        }
        #endregion

        #region Get
        // 空间的路径
        public string Path => UIUtil.GetPath(GO);
        // 名称
        public string GOName => gameObject.name;
        // 获得翻译
        protected string GetStr(string key, params object[] ps) => BaseLangMgr.Get(key, ps);
        public int GetRow(int totalCol) => Mathf.CeilToInt(Index / totalCol);
        public int GetCol(int totalCol) => Index % totalCol;
        #endregion

        #region Dirty
        public virtual void SetDirtyRefresh()
        {
            if (!IsIndependent)
            {
                if (Parent != null) Parent.SetDirtyRefresh();
                else if (PMutexer != null) PMutexer.SetDirtyRefresh();
                else CLog.Error("非独立组件无法调用SetDirtyRefresh,请通过顶级组件调用:{0}", GOName);
                return;
            }
            IsDirtyRefresh = true;
            PUIView?.ActiveControlFixedUpdate(this);
        }
        public virtual void SetDirtyCell()
        {
            if (!IsIndependent)
            {
                if (Parent != null) Parent.SetDirtyCell();
                else if (PMutexer != null) PMutexer.SetDirtyCell();
                else CLog.Error("非独立组件无法调用SetDirtyCell,请通过顶级组件调用:{0}", GOName);
                return;
            }
            IsDirtyCell = true;
            PUIView?.ActiveControlFixedUpdate(this);
        }
        public virtual void SetDirtyData()
        {
            if (!IsIndependent)
            {
                if (Parent != null) Parent.SetDirtyData();
                else if (PMutexer != null) PMutexer.SetDirtyData();
                else CLog.Error("非独立组件无法调用SetDirtyData,请通过顶级组件调用:{0}", GOName);
                return;
            }
            IsDirtyData = true;
            IsDirtyRefresh = true;
            IsDirtyCell = true;
            PUIView?.ActiveControlFixedUpdate(this);
        }
        public virtual void SetDirtyShow()
        {
            if (!IsIndependent)
            {
                if (Parent != null) Parent.SetDirtyShow();
                else if (PMutexer != null) PMutexer.SetDirtyShow();
                else CLog.Error("非独立组件无法调用SetDirtyShow,请通过顶级组件调用:{0}", GOName);
                return;
            }
            IsDirtyShow = true;
            PUIView?.ActiveControlFixedUpdate(this);
        }
        public virtual void SetDirtyAll()
        {
            if (!IsIndependent)
            {
                if (Parent != null) Parent.SetDirtyAll();
                else if (PMutexer != null) PMutexer.SetDirtyAll();
                else CLog.Error("非独立组件无法调用SetDirtyAll,请通过顶级组件调用:{0}", GOName);
                return;
            }
            IsDirtyShow = true;
            IsDirtyData = true;
            IsDirtyRefresh = true;
            IsDirtyCell = true;
            PUIView?.ActiveControlFixedUpdate(this);
        }
        public void SetDirtyAll(float delay)
        {
            Util.Invoke(() => SetDirtyAll(), delay);
        }
        public bool IsDirtyRefresh { get; protected set; } = false;
        public bool IsDirtyCell { get; protected set; } = false;
        public bool IsDirtyData { get; protected set; } = false;
        public bool IsDirtyShow { get; protected set; } = false;
        public virtual void Refresh()
        {
            IsDirtyRefresh = false;
            if (PUIView == null) return;
            if (IsShow)
            {
                foreach (var child in StaticChilds)
                {
                    if (child.IsCanAutoRefresh(true, false))
                        child.Refresh();
                    //有时候当一个组件处于关闭状态的时候也需要处理一些逻辑
                    else if (child.IsClose)
                    {
                        //CheckBox特殊处理
                        if(child is UCheck checkBox)
                            checkBox.ForceCloseLink();
                    }
                }
            }
        }
        public virtual void RefreshCell()
        {
            IsDirtyCell = false;
            if (PUIView == null) return;
            if (IsShow)
            {
                foreach (var child in StaticChilds)
                {
                    if (child.IsCanAutoRefresh(true, false))
                        child.RefreshCell();
                }
            }
        }
        public virtual void RefreshData()
        {
            IsDirtyData = false;
            if (PUIView == null) return;
            if (IsShow)
            {
                foreach (var child in StaticChilds)
                {
                    if (child.IsCanAutoRefresh(true, false))
                        child.RefreshData();
                }
            }
        }
        public virtual void RefreshShow()
        {
            IsDirtyShow = false;
            if (PUIView == null) return;
            if (IsShow)
            {
                foreach (var child in StaticChilds)
                {
                    if (child.IsCanAutoRefresh(false, false))
                        child.RefreshShow();
                }
            }
        }
        public virtual void RefreshAll()
        {
            RefreshShow();
            RefreshData();
            Refresh();
        }
        #endregion

        #region 工具函数
        protected static UModalBoxView BaseModalBoxView => UModalBoxView.Default;
        protected static UTooltipView BaseTooltipView => UTooltipView.Default;
        protected static void ShowTip(string key, params string[] ps) => BaseTooltipView.Show(key, ps);
        protected static void ShowTipStr(string str) => BaseTooltipView.ShowStr(str);
        protected void ShowOKCancleTitle(string key, Callback BntOK, Callback BntCancle, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(PUIView);
            BaseModalBoxView.ShowOKCancleTitle(key, BntOK, BntCancle, paras);
        }
        protected void ShowOKCancle(string descKey, Callback BntOK, Callback BntCancle, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(PUIView);
            BaseModalBoxView.ShowOKCancle(descKey, BntOK, BntCancle, paras);
        }
        protected void ShowOKTitle(string key, Callback BntOK, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(PUIView);
            BaseModalBoxView.ShowOKTitle(key, BntOK, paras);
        }
        protected void ShowOK(string descKey, Callback BntOK, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(PUIView);
            BaseModalBoxView.ShowOK(descKey, BntOK, paras);
        }
        UUIView GetParent(Transform self)
        {
            if (self.parent == null)
                return null;
            UUIView view = self.parent.GetComponent<UUIView>();
            if (view == null)
            {
                return GetParent(self.parent);
            }
            else
            {
                return view;
            }
        }
        #endregion

        #region Callback
        public virtual void OnShow(bool isShow)
        {
            if (isShow) { }
        }
        protected virtual void OnClose()
        {
            if (IsRecord) BaseUIMgr.RemoveRecordControl(this);
        }
        protected virtual void OnOpen()
        {
            if (IsRecord) BaseUIMgr.AddRecordControl(this);
            if (LayoutGroup != null)
            {
                PUIView?.SetDirtyLayout(LayoutGroup);
            }
        }
        public virtual void OnViewShow(bool b)
        {
            foreach (var item in StaticChilds)
                item.OnViewShow(b);
            if (IsRecord) BaseUIMgr.RemoveRecordControl(this);
        }
        protected void OnFadeIn()
        {
            IsShowComplete = true;
            Callback_OnShowComplete?.Invoke(true);
        }
        protected void OnFadeOut()
        {
            IsShowComplete = true;
            Callback_OnShowComplete?.Invoke(false);
        }
        #endregion

        #region Edtor
        public bool Inspector_AlphaShow() => IsAlphaShow;
        public bool Inspector_ColorShow() => IsColorShow;
        public bool Inspector_PosShow() => IsPosShow;
        public bool Inspector_ScaleShow() => IsScaleShow;
        public bool Inspector_AlphaSimpleShow() => IsSimpleShow;
        // 自动设置
        [Button("AutoSetup")]
        public virtual void AutoSetup()
        {
            if (UIConfig.Ins.NormalFont != null)
            {
                Text[] allTexts = GO.GetComponentsInChildren<Text>();
                if (allTexts != null)
                {
                    foreach (var item in allTexts)
                    {
                        if (item.font == null)
                        {
                            var size = item.fontSize;
                            item.font = UIConfig.Ins.NormalFont.Default;
                            item.fontSize = size;
                        }
                    }
                }
            }
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            AutoSetup();
        }
#endif
        #endregion

        #region 系统组建
        public Vector3 Pos { get { return RectTrans.anchoredPosition3D; } set { RectTrans.anchoredPosition3D = value; } }
        private Transform _cacheTransform;
        public Transform Trans
        {
            get
            {
                if (Application.isPlaying)
                    return _cacheTransform ?? (_cacheTransform = transform);
                return transform;
            }
        }
        private GameObject _cacheGO;
        public GameObject GO
        {
            get
            {
                if (Application.isPlaying)
                    return _cacheGO ?? (_cacheGO = gameObject);
                return gameObject;
            }
        }
        private RectTransform _cacheRectTrans;
        public RectTransform RectTrans
        {
            get
            {
                if (Application.isPlaying)
                    return _cacheRectTrans ?? (_cacheRectTrans = transform as RectTransform);
                return transform as RectTransform;
            }
        }
        #endregion
    }
}