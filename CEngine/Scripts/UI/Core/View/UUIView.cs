using DG.Tweening;
using Invoke;
using MoonSharp.Interpreter;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UUIView : UView
    {
        #region Callback Value
        public Callback<bool> Callback_OnShowComplete { get; set; }
        #endregion

        #region Inspector
        [FoldoutGroup("ViewGroup"), SerializeField,Tooltip("是否将界面置为到最前方")] 
        public bool IsFocus = true;

        [FoldoutGroup("Prop"), SerializeField, Tooltip("是否Rebuild整个界面的Layout")]
        protected bool IsLayoutRebuild = false;

        [FoldoutGroup("Animator")]
        public List<UIAlphaAnimator> AlphaAnimator = new List<UIAlphaAnimator>();
        [FoldoutGroup("Animator")]
        public List<UIPosAnimator> PosAnimator = new List<UIPosAnimator>();
        [FoldoutGroup("Animator")]
        public List<UIScaleAnimator> ScaleAnimator = new List<UIScaleAnimator>();

        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        public UButton BntInfo;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        public UButton BntClose;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        public UText Title;
        #endregion

        #region Data
        [FoldoutGroup("Data"), SerializeField]
        string LuaScript;
        #endregion

        #region 内部
        Table LuaTable;
        protected UITween[] UITweens;
        protected List<UIAnimator> EffectShows = new List<UIAnimator>();
        protected HashList<RectTransform> LayoutsDirty { get; private set; } = new HashList<RectTransform>();
        protected Graphic[] graphics { get; private set; }
        protected CanvasGroup canvasGroup { get; private set; }
        protected Vector3 sourceAnchoredPosition3D;
        protected Vector3 sourceLocalScale;
        protected Vector2 sourceAnchorMax;
        protected Vector2 sourceAnchorMin;
        protected Vector2 sourceAnchoredPosition;
        protected Vector2 sourceSizeData;
        protected Vector3 sourcePivot;
        //界面顶层自动排版
        protected LayoutGroup LayoutGroup { get; private set; }
        //用于子界面Panel互斥
        protected UMutexer PanelMutexer { get; private set; } = new UMutexer();
        IJob openDelayJob;
        #endregion

        #region life
        protected virtual string TitleKey => BaseLuaMgr.GetTableStr(LuaTable, nameof(TitleKey));
        protected virtual string CloseKey
        {
            get
            { 
                var ret = BaseLuaMgr.GetTableStr(LuaTable, nameof(CloseKey));
                if (ret.IsInv()) return "Close";
                return ret;
            }
        }
        protected virtual string GetTitle()
        {
            var val = BaseLuaMgr.SafeCallTableFunc(LuaTable, nameof(GetTitle));
            if (val == null)
            {
                if (TitleKey.IsInv())
                    return "None";
                return GetStr(TitleKey);
            }
            return val.String;
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            if (LuaScript.IsInv())
            {
                LuaScript = GOName;
                LuaScript = LuaScript.Replace("(Clone)", "");
            }
            LuaTable = BaseLuaMgr.GetTable(LuaScript);
            if (LuaTable!=null)
            {
                LuaTable[nameof(GetControl)] = (Func<string, UControl>)GetControl;
                LuaTable[nameof(Get)] = (Func<string, UControl>)Get;
                LuaTable[nameof(GetControlByPath)] = (Func<string, UControl>)GetControlByPath;
                LuaTable[nameof(Close)] = (Action<float>)Close;
            }
            BaseLuaMgr.SafeCallTableFunc(LuaTable, nameof(OnSetNeedFlag));
        }
        public override void Awake()
        {
            base.Awake();
            graphics = GetComponentsInChildren<Graphic>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && RectTrans != null)
                canvasGroup = GO.AddComponent<CanvasGroup>();
            LayoutGroup = GetComponent<LayoutGroup>();
            UITweens = GetComponentsInChildren<UITween>();
            //初始化变化组件
            EffectShows.AddRange(AlphaAnimator);
            EffectShows.AddRange(PosAnimator);
            EffectShows.AddRange(ScaleAnimator);
            foreach (var item in EffectShows)
                item.Init(this);
            sourceAnchorMax = RectTrans.anchorMax;
            sourceAnchorMin = RectTrans.anchorMin;
            sourceLocalScale = RectTrans.localScale;
            sourceSizeData = RectTrans.sizeDelta;
            sourceAnchoredPosition = RectTrans.anchoredPosition;
            sourceAnchoredPosition3D = RectTrans.anchoredPosition3D;
            sourcePivot = RectTrans.pivot;
            //lua 调用
            BaseLuaMgr.SafeCallTableFunc(LuaTable, nameof(Awake), this);
        }
        public override void Start()
        {
            base.Start();
            //lua调用
            BaseLuaMgr.SafeCallTableFunc(LuaTable, nameof(Start), this);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            //lua调用
            BaseLuaMgr.SafeCallTableFunc(LuaTable, nameof(OnEnable), this);
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            if (BntInfo != null) BntInfo.Init(new UButtonData { OnClick = OnClickInfo });
            if (BntClose != null) BntClose.Init(new UButtonData { NameKey = CloseKey, OnClick = OnClickClose });
            if (Title != null) Title.Init(new UTextData { Name = GetTitle, IsTrans = false });
            //lua调用
            BaseLuaMgr.SafeCallTableFunc(LuaTable, nameof(OnCreatedView), this);
        }

        // 将界面挂到其他界面下
        public sealed override void Attach(ViewLevel viewLevel, UView beAttchedView)
        {
            base.Attach(viewLevel, beAttchedView);
            ResetSourcePosData();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (CanvasScaler != null)
            {
                HUDItemOffset = (Screen.width / CanvasScaler.referenceResolution.x) * (1 - CanvasScaler.matchWidthOrHeight) + (Screen.height / CanvasScaler.referenceResolution.y) * CanvasScaler.matchWidthOrHeight;
            }
            foreach (var item in FixedUpdateControls)
            {
                if (item.IsDirtyShow) item.RefreshShow();
                if (item.IsDirtyData) item.RefreshData();
                if (item.IsDirtyRefresh) item.Refresh();
                if (item.IsDirtyCell) item.RefreshCell();
                item.OnFixedUpdate();
            }
            foreach (var item in Mutexers)
                item.OnFixedUpdate();
            PanelMutexer.OnFixedUpdate();
            PanelMutexer.Current?.OnFixedUpdate();

            if (LayoutsDirty.Count > 0)
            {
                foreach (var item in LayoutsDirty)
                    item.RebuildLayout();
                LayoutsDirty.Clear();
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var item in UpdateControls)
                item.OnUpdate();
            foreach (var item in Mutexers)
                item.OnUpdate();
            PanelMutexer.OnUpdate();
            PanelMutexer.Current?.OnUpdate();
        }
        // 销毁UI
        public override void DoDestroy()
        {
            base.DoDestroy();
            Panels.Clear();
            Mutexers.Clear();
            DicMutexers.Clear();
            UpdateControls.Clear();
            FixedUpdateControls.Clear();
            //lua调用
            BaseLuaMgr.SafeCallTableFunc(LuaTable, nameof(OnDestroy), this);
        }
        // 获取控件
        protected sealed override void FetchSubjects()
        {
            if (GO == null) return;
            if (IsRootView) return;

            //获得所有的控件
            var tempControls = GO.GetComponentsInChildren<UControl>(true);
            if (tempControls != null)
            {
                foreach (var item in tempControls)
                {
                    item.OnBeFetched();
                    if (item is UPanel panel)
                    {
                        AddPanel(panel);
                    }
                    else
                    {
                        if(item.IsCanBeViewFetch)
                            AddControl(item);
                        if (item.MutexGroup != MutexGroup.None)
                            AddMutexer(item.MutexGroup.ToString(), false,true, item);                       
                    }                    
                }
            }
        }
        #endregion

        #region set
        public void ResetSourcePosData()
        {
            RectTrans.localScale = sourceLocalScale;
            RectTrans.anchorMax = sourceAnchorMax;
            RectTrans.anchorMin = sourceAnchorMin;
            RectTrans.sizeDelta = sourceSizeData;
            Trans.localPosition = sourceLocalPos;
            RectTrans.anchoredPosition = sourceAnchoredPosition;
            RectTrans.anchoredPosition3D = sourceAnchoredPosition3D;
            RectTrans.pivot = sourcePivot;
        }
        // 显示或者关闭界面
        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            if (IsShow == b && 
                !force && 
                ShowCount>0) 
                return;
            base.Show(b, useGroup, force);

            //执行界面的显示逻辑
            if (IsShow)
            {
                //触发DOTween
                if (UITweens != null)
                {
                    foreach (var item in UITweens)
                    {
                        item.DoTween();
                    }
                }
                //刷新数据和组件
                SetDirtyAll();
                //设置焦点
                if (IsFocus) SetFocus();
                OnOpen(this, useGroup);
            }
            else
            {
                OnClose(useGroup);
            }
            OnShow();

            //设置时间
            float mainShowTime = 0;
            if (ShowTime >= 0) mainShowTime = ShowTime;
            else mainShowTime = IsSameTime ? Duration : (b ? InTime : OutTime);
            if (IsInShowDefault)
                mainShowTime = 0;

            //停止之前的tween
            if (alphaTween != null) alphaTween.Kill();
            if (scaleTween != null) scaleTween.Kill();
            if (moveTween != null) moveTween.Kill();

            //Alpha效果
            if (IsReset) canvasGroup.alpha = IsShow ? 0.0f : 1.0f;
            alphaTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, b ? 1.0f : 0.0f, mainShowTime);
            alphaTween.SetDelay(Delay);
            if (IsShow) alphaTween.OnComplete(OnFadeIn);
            else alphaTween.OnComplete(OnFadeOut);

            //缩放效果
            if (IsScale) OnShowScaleEffect(mainShowTime);
            //位移效果
            if (IsMove) OnShowMoveEffect(mainShowTime);
            //屏蔽/取消屏蔽 UI点击
            if (canvasGroup != null) canvasGroup.blocksRaycasts = IsShow;
            //触发控件的ViewShow事件
            foreach (var item in Controls) item.OnViewShow(b);
            //触发Panel的ViewShow事件
            foreach (var item in Panels) item.Value.OnViewShow(b);
            //触发动画特效
            foreach (var item in EffectShows) item.OnShow(b);
        }
        public override void Enable(bool b)
        {
            IsEnable = b;
            if (Canvas != null) Canvas.enabled = IsEnable;
            if (GraphicRaycaster != null) GraphicRaycaster.enabled = IsEnable;
            if (CanvasScaler != null) CanvasScaler.enabled = IsEnable;
        }
        public override void Interactable(bool b)
        {
            if (canvasGroup != null)
                canvasGroup.interactable = b;
        }
        public void SetDirtyLayout(LayoutGroup layout)
        {
            RectTransform rectTrans = layout.transform as RectTransform;
            SetDirtyLayout(rectTrans);
        }
        public void SetDirtyLayout(RectTransform rectTrans)
        {
            if (LayoutsDirty.Contains(rectTrans)) return;
            LayoutsDirty.Add(rectTrans);
        }
        public void SetDirtyLayout()
        {
            SetDirtyLayout(RectTrans);
        }
        public void SetDirtyLayout(UControl control)
        {
            SetDirtyLayout(control.RectTrans);
        }
        #endregion

        #region Refresh
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh())
                    item.Refresh();
            }
            //lua调用
            BaseLuaMgr.SafeCallTableFunc(LuaTable, nameof(Refresh), this);
        }
        public override void RefreshCell()
        {
            base.RefreshCell();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh())
                    item.RefreshCell();
            }
        }
        public override void RefreshData()
        {
            base.RefreshData();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh())
                    item.RefreshData();
            }
        }
        public override void RefreshShow()
        {
            base.RefreshShow();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh(false))
                    item.RefreshShow();
            }
        }
        public override void RefreshAll()
        {
            base.RefreshAll();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh(false))
                    item.RefreshAll();
            }
        }
        #endregion

        #region dirty
        public sealed override void SetDirtyCell()
        {
            base.SetDirtyCell();
        }
        public sealed override void SetDirtyShow()
        {
            base.SetDirtyShow();
            PanelMutexer.Current?.SetDirtyShow();
            foreach (var item in Mutexers)
                item.SetDirtyShow();
        }
        public sealed override void SetDirtyData()
        {
            base.SetDirtyData();
            PanelMutexer.Current?.SetDirtyData();
            foreach (var item in Mutexers)
                item.SetDirtyData();
        }
        public sealed override void SetDirtyRefresh()
        {
            base.SetDirtyRefresh();
            PanelMutexer.Current?.SetDirtyRefresh();
            foreach (var item in Mutexers)
                item.SetDirtyRefresh();
        }
        public sealed override void SetDirtyAll()
        {
            base.SetDirtyAll();
            PanelMutexer.Current?.SetDirtyAll();
            foreach (var item in Mutexers)
                item.SetDirtyAll();
        }
        #endregion

        #region control
        [PropertyOrder(-1),ReadOnly,ShowInInspector,ShowIf("@UnityEngine.Application.isPlaying")] 
        protected HashList<UControl> FixedUpdateControls { get; private set; } = new HashList<UControl>();
        [PropertyOrder(-1), ReadOnly, ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        protected HashList<UControl> UpdateControls { get; private set; } = new HashList<UControl>();
        [PropertyOrder(-1), ReadOnly, ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        protected HashList<UControl> Controls { get; private set; } = new HashList<UControl>();
        protected Dictionary<string, UControl> DicControls { get; private set; } = new Dictionary<string, UControl>();
        public void ActiveControlFixedUpdate(UControl control)
        {
            
            if (!control.IsCanBeViewFetch)
            {
                CLog.Error("错误:{0},无法作为View的控件", control.GOName);
                return;
            }
            FixedUpdateControls.Add(control);
        }
        public void ActiveControlUpdate(UControl control)
        {
            if (!control.IsCanBeViewFetch)
            {
                CLog.Error("错误:{0},无法作为View的控件", control.GOName);
                return;
            }
            UpdateControls.Add(control);
        }
        protected void AddControl(UControl item)
        {
            item.PUIView = this;
            Controls.Add(item);
            if (!DicControls.ContainsKey(item.GOName))
                DicControls.Add(item.GOName,item);
            if (item.NeedFixedUpdate)
                ActiveControlFixedUpdate(item);
            if (item.NeedUpdate)
                ActiveControlUpdate(item);
        }
        public UControl GetControlByPath(string path)
        {
            var ts = Trans.Find(path);
            return ts.GetComponent<UControl>();
        }
        public UControl GetControl(string key)
        {
            if (DicControls.ContainsKey(key))
                return DicControls[key];
            return null;
        }
        //获取这个组件的时候自动Init
        public UControl Get(string key)
        {
            var ret = GetControl(key);
            if(!ret.IsInited)
                ret.Init();
            return ret;
        }
        #endregion

        #region Panel
        [PropertyOrder(-1), ReadOnly, ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        protected Dictionary<string, UPanel> Panels { get; private set; } = new Dictionary<string, UPanel>();
        //默认的MainPanel,用户需要将其命名为Main,会自动赋值给这个变量
        [PropertyOrder(-1), ReadOnly, ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        protected UPanel MainPanel { get; private set; }
        // 创建Panel
        protected T CreatePanel<T>(string path) where T : UPanel
        {
            T panel = UIMgr.CreateUIGO<T>(path);
            if (panel == null) return null;
            panel.Trans.SetParent(Trans);
            panel.ResetSourcePosData();
            panel.Trans.SetAsLastSibling();
            panel.GO.name = path;
            panel.PUIView = this;
            AddPanel(panel);
            return panel;
        }
        public void AddPanel(UPanel panel)
        {
            panel.PUIView = this;
            if (MainPanel == null)
                MainPanel = panel;
            Panels.Add(panel.GOName, panel);
            PanelMutexer.Add(panel);
        }
        public void RemovePanel(UPanel panel)
        {
            if (MainPanel == panel)
                MainPanel = null;
            Panels.Remove(panel.GOName);
            PanelMutexer.Remove(panel);
        }
        // 获得Panel
        protected T GetPanel<T>(string name) where T : UPanel
        {
            if (Panels.ContainsKey(name))
                return Panels[name] as T;
            return null;
        }
        #endregion

        #region Mutexer 组件互斥组,一次只能显示一个组件
        //默认用户自定义添加的第一个Mutex对象
        protected UMutexer MainMutexer { get; private set; } = null;
        protected List<UMutexer> Mutexers { get; private set; } = new List<UMutexer>();
        protected Dictionary<string,UMutexer> DicMutexers { get; private set; } = new Dictionary<string, UMutexer>();
        /// <summary>
        /// 添加互斥
        /// </summary>
        /// <param name="isNeedReset">是否重置</param>
        /// <param name="isShowOne">是否至少选择一个</param>
        /// <param name="controls"></param>
        /// <returns></returns>
        protected UMutexer AddMutexerMain(bool isNeedReset, bool isShowOne, params UControl[] controls)
        {
            if (controls == null) return null;
            //如果有默认Panel的话,添加到默认Panel
            if (MainPanel != null)
            {
                foreach (var item in controls)
                {
                    if (item.IsCanBeViewFetch)
                    {
                        CLog.Error("错误:{0}MainPanel的控件", item.GOName);
                        return null;
                    }
                }
                var ret = MainPanel.AddMutexer(isNeedReset, isShowOne, controls);
                if (MainMutexer == null)
                    MainMutexer = ret;
                return ret;
            }
            //没有默认Panel的话,添加到主界面
            else
            {
                var ret = AddMutexer(isNeedReset, isShowOne, controls);
                if (MainMutexer == null)
                    MainMutexer = ret;
                return ret;
            }
        }
        protected UMutexer AddMutexer(bool isNeedReset, bool isShowOne, params UControl[] controls)
        {
            if (controls == null) return null;
            foreach (var item in controls)
            {
                if (!item.IsCanBeViewFetch)
                {
                    CLog.Error("错误:{0}不能作为View的控件", item.GOName);
                    return null;
                }
            }
            var temp = new UMutexer(controls, isNeedReset, isShowOne);
            Mutexers.Add(temp);
            foreach (var item in controls)
                Controls.Remove(item);
            return temp;
        }
        protected void AddMutexer(string name,bool isNeedReset, bool isShowOne, UControl item)
        {
            if (item == null) 
                return;
            if (!item.IsCanBeViewFetch)
            {
                CLog.Error("错误:{0}不能作为View的控件", item.GOName);
                return;
            }
            if (!DicMutexers.ContainsKey(name))
            {
                var temp = new UMutexer(isNeedReset, isShowOne);
                Mutexers.Add(temp);
                DicMutexers.Add(name, temp);
                temp.Add(item);
            }
            else
            {
                var temp = DicMutexers[name];
                temp.Add(item);
            }
            Controls.Remove(item);
        }
        protected UMutexer GetMutexer(string name)
        {
            if (DicMutexers.ContainsKey(name))
            {
                return DicMutexers[name];
            }
            return null;
        }
        protected UMutexer GetMutexer(MutexGroup mutexGroup)
        {
            return GetMutexer(mutexGroup.ToString());
        }
        #endregion

        #region Callback
        public override void OnGameStarted2()
        {
            base.OnGameStarted2();
            SetDirtyAll();
        }
        protected virtual void OnClickClose(UControl control, PointerEventData data) => Show(false);
        protected virtual void OnClickInfo(UControl arg1, PointerEventData arg2)
        {
            
        }
        protected void OnShowScaleEffect(float mainShowTime)
        {
            if (scaleTween != null) scaleTween.Kill();
            Vector3 minScale = sourceLocalScale * 0.001f;
            if (IsShow && TweenMove.IsReset) Trans.localScale = minScale;
            if (mainShowTime >= 0)
            {
                scaleTween = Trans.DOScale(
                IsShow ? sourceLocalScale : minScale,
                mainShowTime)
                .SetEase(IsShow ? TweenScale.InEase : TweenScale.OutEase)
                .SetDelay(Delay);
            }
        }
        protected void OnShowMoveEffect(float mainShowTime)
        {
            if (IsDragged) return;
            if (moveTween != null) moveTween.Kill();
            if (TweenMove.StartPos.x == 0)
                TweenMove.StartPos.x = RectTrans.anchoredPosition.x;
            if (TweenMove.StartPos.y == 0)
                TweenMove.StartPos.y = RectTrans.anchoredPosition.y;
            if (IsShow && TweenMove.IsReset) RectTrans.anchoredPosition = TweenMove.StartPos;
            if (mainShowTime >= 0)
            {
                moveTween = DOTween.To(
                    () => RectTrans.anchoredPosition,
                    (x) => RectTrans.anchoredPosition = x,
                    IsShow ? sourceAnchoredPosition : TweenMove.StartPos,
                    mainShowTime)
                    .SetEase(IsShow ? TweenMove.InEase : TweenMove.OutEase)
                    .SetDelay(Delay);
            }
        }
        #endregion

        #region open & close
        protected override void OnOpenDelay(UView baseView, bool useGroup)
        {
            base.OnOpenDelay(baseView, useGroup);
            if (IsLayoutRebuild)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(RectTrans);
            }
            else
            {
                LayoutGroup?.RebuildLayout();
            }
        }
        protected override void OnOpen(UView baseView, bool useGroup)
        {
            base.OnOpen(baseView, useGroup);
            openDelayJob?.Kill();
            openDelayJob = Util.Invoke(() => OnOpenDelay(baseView,useGroup), 0.01f);
        }
        protected override void OnClose(bool useGroup)
        {
            PanelMutexer.ShowDefault();
            foreach (var item in Mutexers)
                item.TestReset();
            base.OnClose(useGroup);
        }
        protected override void OnFadeIn()
        {
            base.OnFadeIn();
            Callback_OnShowComplete?.Invoke(true);
        }
        protected override void OnFadeOut()
        {
            base.OnFadeOut();
            if (IsScale) Trans.localScale = Vector3.one * 0.001f;
            if (IsMove) RectTrans.anchoredPosition = TweenMove.StartPos;
            Callback_OnShowComplete?.Invoke(false);
        }
        #endregion

        #region 工具函数包装
        protected static UModalBoxView BaseModalBoxView => UModalBoxView.Default;
        protected static UTooltipView BaseTooltipView => UTooltipView.Default;
        public static void ShowTip(string key, params string[] ps) => BaseTooltipView.Show(key, ps);
        public static void ShowTipStr(string str) => BaseTooltipView.ShowStr(str);
        protected void ShowOKCancleTitle(string key, Callback BntOK, Callback BntCancle, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(this);
            BaseModalBoxView.ShowOKCancleTitle(key,BntOK, BntCancle, paras);
        }
        protected void ShowOKCancle(string descKey, Callback BntOK, Callback BntCancle, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(this);
            BaseModalBoxView.ShowOKCancle(descKey, BntOK, BntCancle, paras);
        }
        protected void ShowOKTitle(string key,  Callback BntOK, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(this);
            BaseModalBoxView.ShowOKTitle(key, BntOK, paras);
        }
        protected void ShowOK(string descKey, Callback BntOK, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(this);
            BaseModalBoxView.ShowOK(descKey, BntOK, paras);
        }
        #endregion

        #region Show tip
        protected UTooltipView ShowAutoTip(string key, string second) => BaseTooltipView.ShowAuto(key, second);
        protected UTooltipView ShowAutoTip(string key, string second, string third) => BaseTooltipView.ShowAuto(key, second, third);
        protected UTooltipView ShowAutoTip(string key, string second, string third, string fourth) => BaseTooltipView.ShowAuto(key, second, third, fourth);
        protected UTooltipView ShowAutoTip(string key, string second, string third, string fourth, string fifth) => BaseTooltipView.ShowAuto(key, second, third, fourth, fifth);
        protected UTooltipView ShowAutoTip(string key, string second, string third, string fourth, string fifth, string sixth) => BaseTooltipView.ShowAuto(key, second, third, fourth, fifth, sixth);

        protected UTooltipView ShowAutoTipStr(string first, string second) => BaseTooltipView.ShowAutoStr(first, second);
        protected UTooltipView ShowAutoTipStr(string first, string second, string third) => BaseTooltipView.ShowAutoStr(first, second, third);
        protected UTooltipView ShowAutoTipStr(string first, string second, string third, string fourth) => BaseTooltipView.ShowAutoStr(first, second, third, fourth);
        protected UTooltipView ShowAutoTipStr(string first, string second, string third, string fourth, string fifth) => BaseTooltipView.ShowAutoStr(first, second, third, fourth, fifth);
        protected UTooltipView ShowAutoTipStr(string first, string second, string third, string fourth, string fifth, string sixth) => BaseTooltipView.ShowAutoStr(first, second, third, fourth, fifth, sixth);
        #endregion
    }
}
