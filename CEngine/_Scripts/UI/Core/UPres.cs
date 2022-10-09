//------------------------------------------------------------------------------
// Presenter.cs
// Copyright 2019 2019/7/19 
// Created by CYM on 2019/7/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

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
    public class UData
    {
        #region prop
        public UIBehaviour Behaviour;
        #endregion

        //触发
        public Callback<UControl, PointerEventData> OnEnter;
        public Callback<UControl, PointerEventData> OnExit;
        public Callback<UControl, PointerEventData> OnClick;
        public Callback<UControl, PointerEventData> OnTempClick; //用于替换原生的Click事件
        public Callback<UControl, PointerEventData> OnDelayClick;
        public Callback<UControl, PointerEventData> OnPlotClick;
        public Callback<UControl, PointerEventData> OnDown;
        public Callback<UControl, PointerEventData> OnUp;
        public Callback<UControl, PointerEventData> OnLongPress;
        public Callback<UControl, PointerEventData> OnLongEnter;
        public Callback<UControl, bool> OnInteractable;
        public Callback<UControl, bool> OnSelected;
        public Callback<UControl, bool> OnShow;
        public Callback<UControl, bool> OnShowActive;
        public Callback<UControl> OnRefresh;
        //拖拽
        public Callback<UControl, PointerEventData> OnBeginDrag;
        public Callback<UControl, PointerEventData> OnEndDrag;
        public Callback<UControl, PointerEventData> OnDrag;
        //判断
        public Func<int, bool> IsInteractable = null;
        public Func<int, bool> IsSelected = null;
        public Func<int, bool> IsCanClick = null;
        public Func<int, bool> IsShow = null;

        //值
        public string ClickClip { get; set; } = "AppTab";
        public string HoverClip { get; set; } = "";
        public string OpenClip { get; set; } = "";
        public string CloseClip { get; set; } = "";
        public string TipKey { get; set; } = SysConst.STR_Inv;
        public string CursorKey { get; set; } = SysConst.STR_Inv;
    }

    public class UPres<TData> : UControl, 
        IPointerEnterHandler, 
        IPointerExitHandler, 
        IPointerClickHandler, 
        IPointerDownHandler, 
        IPointerUpHandler
        where TData : UData, new()
    {
        #region Inspector 
        [FoldoutGroup("Base"), Tooltip("是否可以点击,优先级低")]
        public bool IsCanClick = true;
        [FoldoutGroup("Base"), Tooltip("是否可以长按,优先级低")]
        public bool IsCanLongPress = false;
        [FoldoutGroup("Base"), Tooltip("是否可以长按,优先级低")]
        public bool IsCanLongEnter = false;
        [FoldoutGroup("Base"), Tooltip("字体类型"),PropertyOrder(-2)]
        [ValueDropdown("Inspector_FontPresets"),ShowIf("Inspector_IsShowFontType")]
        public string FontType =nameof(CYM.FontType.Normal);

        [FoldoutGroup("Trans")]
        public List<UIColorTransition> ColorTrans = new List<UIColorTransition>();
        [FoldoutGroup("Trans")]
        public List<UIPosTransition> PosTrans = new List<UIPosTransition>();
        [FoldoutGroup("Trans")]
        public List<UIScaleTransition> ScaleTrans = new List<UIScaleTransition>();
        [FoldoutGroup("Trans")]
        public List<UIEffectColorTransition> EffectColorTrans = new List<UIEffectColorTransition>();
        [FoldoutGroup("Trans")]
        public List<UIAnimTransition> AnimTrans = new List<UIAnimTransition>();
        [FoldoutGroup("Trans")]
        public List<UISpriteTransition> SpriteTrans = new List<UISpriteTransition>();
        #endregion

        #region Callback Val
        //控件事件响应,可以直接赋值
        public TData Data { get; protected set; } = new TData();
        #endregion

        #region prop
        private IJob longPressJob;
        private IJob longEnterJob;
        protected Text[] Texts { get; private set; }
        protected List<UITransition> EffectTrans = new List<UITransition>();
        protected BaseCursorMgr CursorMgr => BaseGlobal.CursorMgr;
        protected IPlotMgr PlotMgr => BaseGlobal.PlotMgr;
        #endregion

        #region life
        public override void OnBeFetched()
        {
            base.OnBeFetched();
            FetchTexts();
            //初始化变化组件
            EffectTrans.Clear();
            EffectTrans.AddRange(ColorTrans);
            EffectTrans.AddRange(PosTrans);
            EffectTrans.AddRange(ScaleTrans);
            EffectTrans.AddRange(EffectColorTrans);
            EffectTrans.AddRange(AnimTrans);
            EffectTrans.AddRange(SpriteTrans);
            foreach (var item in EffectTrans)
                item.Init(this);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            DefetchTexts();
        }
        #endregion

        #region Init
        public void SetData(TData data)
        {
            Data = data;
            Data.Behaviour = this;
            IsInited = true;
        }
        public void ReInit(TData data)
        {
            CancleInit();
            Init(data);
        }
        // 初始化,使用这个函数初始化,将会通过View或者父级Pressenter的Refresh自动刷新
        public virtual void Init(TData data)
        {
            if (Parent != null && !Parent.IsAutoInit && !Parent.IsInited)
            {
                CLog.Error("Parent 没有被初始化:" + Parent.Path + " , Self:" + Path);
                return;
            }
            if (IsInited)
            {
                CLog.Error("一个Presenter 不能初始化2次" + Path);
                return;
            }
            if (data == null)
            {
                CLog.Error("Presenter 的 data为空!!" + Path);
                return;
            }
            SetData(data);
        }
        public override void Init(Callback<UControl> onInit)
        {
            Init(new TData());
            base.Init(onInit);
        }
        public override void CancleInit()
        {
            base.CancleInit();
            if (!IsInited) return;
            Data = new TData();
            Data.Behaviour = null;
        }
        #endregion

        #region set
        public override void SetActive(bool b)
        {
            if (GO == null) return;
            if (GO.activeSelf == b) return;
            base.SetActive(b);
            Data?.OnShowActive?.Invoke(this, b);
        }
        // 刷新:手动调用的刷新,传入临时的Data对象
        public void Refresh(TData data)
        {
            TData preData = Data;
            Data = data;
            Refresh();
            Data = preData;
        }
        // 清除
        public override void Cleanup()
        {
            base.Cleanup();
            Data = null;
        }
        // 设置点击状态
        public void SetInteractable(bool b, bool isCacheAC = true,bool effect=true)
        {
            if (!IsShow) return;
            if (isCacheAC)
            {
                if (Data.OnEnter != null)
                    ACTip = BaseGlobal.ACM.GetAll();
            }
            if (IsInteractable == b) return;
            IsInteractable = b;
            OnInteractable(b, effect);
        }
        public void SetCanClick(bool b)
        {
            IsCanClick = b;
        }
        // 设置选择状态
        public void SetSelected(bool b)
        {
            if (IsSelected == b)
                return;
            IsSelected = b;
            OnSelected(b);
            foreach (var item in EffectTrans)
            {
                item.OnSelected(b);
            }
        }
        protected void PlayClickAudio()
        {
            if (!Data.ClickClip.IsInv())
                PlayClip(Data.ClickClip);
        }
        #endregion

        #region get
        public bool GetIgnoreBlockClick()
        {
            if (BaseGlobal.PlotMgr != null)
            {
                if(BaseGlobal.PlotMgr.IsInIgnoreBlockClick(this))
                    return true;
                if (BaseGlobal.PlotMgr.IsInIgnoreBlockClickView(PUIView))
                    return true;
            }
            //if (PUIView && PUIView.IsIgnoreBlockClick) return true;
            if (PDupplicate && PDupplicate.IsIgnoreBlockClick) return true;
            if (PScroll && PScroll.IsIgnoreBlockClick) return true;
            if (Parent && Parent.IsIgnoreBlockClick) return true;
            return IsIgnoreBlockClick;
        }
        public TData NewData()
        {
            return new TData();
        }
        #endregion

        #region text
        void FetchTexts()
        {
            Texts = GO.GetComponentsInChildren<Text>(true);
            if (Texts == null) return;
            if (BaseGlobal.Ins == null) return;

            foreach (var item in Texts)
            {
                if (item != null)
                {
                    GlobalUITextMgr.AddText(item, FontType);
                }
            }
        }
        void DefetchTexts()
        {
            if (Texts == null) return;
            if (BaseGlobal.Ins == null) return;
            foreach (var item in Texts)
            {
                if (item != null)
                {
                    GlobalUITextMgr.RemoveText(item);
                }
            }
        }
        #endregion

        #region is
        public bool CheckCanClick()
        {
            if (IsBlockClick() && 
                !GetIgnoreBlockClick())
                return false;
            if (!IsCanClick)
                return false;
            if (Data != null && 
                Data.IsCanClick != null
                )
            {
                if (Data.IsCanClick.Invoke(Index))
                    return true;
                else return false;
            }
            return true;
        }
        public bool IsBlockClick()
        {
            if (BaseGlobal.PlotMgr == null) return false;
            return BaseGlobal.PlotMgr.IsBlockClick;
        }
        bool CheckIgnoreTrigger(PointerEventData eventData)
        {
            return false;
        }
        #endregion

        #region Refresh
        public override void Refresh()
        {
            base.Refresh();
            if (IsShow)
            {
                if (Data != null)
                {
                    if (Data.IsInteractable != null)
                        SetInteractable(Data.IsInteractable.Invoke(Index));
                    if (Data.IsSelected != null)
                        SetSelected(Data.IsSelected.Invoke(Index));
                    if (Data.OnRefresh != null)
                        Data.OnRefresh(this);
                }
            }
        }
        public override void RefreshShow()
        {
            if (Data != null && Data.IsShow != null)
                ShowDirect(Data.IsShow.Invoke(Index), false, false, false);
            base.RefreshShow();
        }
        #endregion 

        #region callback
        // 鼠标进入
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (CheckIgnoreTrigger(eventData))
                return;

            if (Data != null)
            {
                //设置Presenter的鼠标样式
                if (Data.CursorKey.IsInv()) CursorMgr?.SetNormal();
                else CursorMgr?.SetCursor(Data.CursorKey);
                //现实提示
                if (Data.OnEnter != null)
                {
                    Data.OnEnter.Invoke(this, eventData);
                }
                else if (!Data.TipKey.IsInv())
                {
                    UTooltipView.Default?.Show(Data.TipKey);
                }
            }

            if (IsCanLongEnter)
            {
                longEnterJob = SuperInvoke.Run(() => {
                    Data?.OnLongEnter?.Invoke(this, eventData);
                },SysConst.LongPressDuration);
            }

            if (!CheckCanClick()) return;
            if (!IsInteractable) return;
            if (!Data.HoverClip.IsInv())
                PlayClip(Data.HoverClip);

            if (!Application.isMobilePlatform)
            {
                foreach (var item in EffectTrans)
                    item.OnPointerEnter(eventData);
            }
        }
        // 鼠标退出
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (CheckIgnoreTrigger(eventData))
                return;

            Data?.OnExit?.Invoke(this, eventData);
            UTooltipView.CurShow?.Show(false);

            if (IsCanLongEnter)
            {
                longEnterJob?.Kill();
            }

            if (!Application.isMobilePlatform)
            {
                foreach (var item in EffectTrans)
                    item.OnPointerExit(eventData);
            }
        }
        // 鼠标点击
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (Data == null)
                return;
            if (BaseInputMgr.IsLongPressTime()) return;
            if (CheckIgnoreTrigger(eventData)) return;
            if (!CheckCanClick()) return;
            if (!IsInteractable)
            {
                if (Application.isMobilePlatform)
                {
                    if (!ACTip.IsInv())
                    {
                        UTipView.Default?
                        .ShowStr(ACTip)
                        .SetTextAlign(TextAnchor.MiddleLeft);
                    }
                }
                return;
            }
            PScroll?.SelectItem(this);
            PDupplicate?.SelectItem(this);
            //替换点击
            if (Data.OnTempClick != null)
            {
                Data.OnTempClick.Invoke(this, eventData);
                Data.OnTempClick = null;
            }
            else
            {
                Data.OnClick?.Invoke(this, eventData);
            }
            //剧情模式相关点击事件
            if (UIMgr!=null &&
                PlotMgr !=null && 
                PlotMgr.IsInPlot())
            {
                if(PlotMgr.IsIgnoreBlockClickOnce(this))
                    UGuideView.Default?.CloseWhenMaskOnce();
                Data.OnPlotClick?.Invoke(this,eventData);
                Data.OnPlotClick = null;
            }
            //延迟点击
            if (Data.OnDelayClick != null)
            {
                Util.Invoke(() =>
                {
                    Data.OnDelayClick.Invoke(this, eventData);
                    Data.OnDelayClick = null;
                }, 0.05f);
            }
            PlotMgr?.RemIgnoreBlockClickOnce(this);
            PlayClickAudio();
            BaseUIMgr.Callback_OnControlClick?.Invoke(this, eventData);
        }
        // 鼠标按下
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (CheckIgnoreTrigger(eventData))
                return;

            if (IsCanLongPress)
            {
                longPressJob = SuperInvoke.Run(() => {
                    Data?.OnLongPress?.Invoke(this, eventData);
                }, 0.5f);
            }

            if (!CheckCanClick()) return;
            if (!IsInteractable) return;
            Data?.OnDown?.Invoke(this, eventData);

            foreach (var item in EffectTrans)
                item.OnPointerDown(eventData);
        }
        // 鼠标按下
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (CheckIgnoreTrigger(eventData))
                return;

            if (IsCanLongPress)
            {
                longPressJob?.Kill();
            }

            if (!CheckCanClick()) return;
            if (!IsInteractable) return;
            Data?.OnUp?.Invoke(this, eventData);

            foreach (var item in EffectTrans)
                item.OnPointerUp(eventData);
        }
        // 点击状态变化
        public virtual void OnInteractable(bool b, bool effect = true)
        {
            Data?.OnInteractable?.Invoke(this, b);
            if (effect)
            {
                foreach (var item in EffectTrans)
                    item.OnInteractable(b);
            }
        }
        public virtual void OnSelected(bool b)
        {
            Data?.OnSelected?.Invoke(this, b);
            if (!Data.HoverClip.IsInv())
                PlayClip(Data.HoverClip);

            foreach (var item in EffectTrans)
                item.OnSelected(b);
        }
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Data?.OnBeginDrag?.Invoke(this, eventData);
        }
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Data?.OnEndDrag?.Invoke(this, eventData);
        }
        public virtual void OnDrag(PointerEventData eventData)
        {
            Data?.OnDrag?.Invoke(this, eventData);
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            if (isShow) PlayClip(Data?.OpenClip);
            else PlayClip(Data?.CloseClip);
            Data?.OnShow?.Invoke(this, isShow);
        }
        #endregion

        #region Editor
        protected string[] Inspector_FontPresets()
        {
            List<string> data = new List<string>();
            data.Add(nameof(CYM.FontType.None));
            data.Add(nameof(CYM.FontType.Normal));
            data.Add(nameof(CYM.FontType.Title));
            data.Add(nameof(CYM.FontType.Dynamic));
            data.AddRange(UIConfig.Ins.ExtraFonts.Keys);
            return data.ToArray();
        }
        protected virtual bool Inspector_IsShowFontType()
        {
            return true;
        }
        #endregion
    }
}