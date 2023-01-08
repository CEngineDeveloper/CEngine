//------------------------------------------------------------------------------
// TDBaseAlert.cs
// Copyright 2019 2019/3/1 
// Created by CYM on 2019/3/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.Unit
{
    [Serializable]
    public class TDBaseAlertData : TDBaseData
    {
        #region lua
        //自动显示
        public bool IsAutoTrigger { get; set; } = false;
        // 通知类型
        public AlertType Type { get; set; } = AlertType.Continue;
        // 背景
        public string Bg { get; set; }
        //打开音效
        public string StartSFX { get; set; }
        // 插图
        public string Illustration { get; set; }
        // 存在天数
        public float TotalTurn { get; set; }
        //提示Key
        public string TipKey { get; set; }
        // 提示文字
        public string TipStr { get; set; }
        // 详细文字
        public string DetailStr { get; set; }
        // 标题文字
        public string TitleStr { get; set; }
        //当前天数
        public float CurTurn { get; set; }
        //是否时间快结束了
        public bool IsCommingTimeOutFalg { get; set; }
        #endregion

        #region pub
        //来源
        public BaseUnit Cast { get; set; }
        //管理器
        public IAlertMgr<TDBaseAlertData> AlertMgr { get; set; }
        #endregion

        #region prop
        //额外参数
        public object[] AddtionParam { get; protected set; }
        #endregion

        #region Callback Val
        //点击的时候触发
        public Callback<TDBaseAlertData> OnClick = (x) => { };
        public Callback<TDBaseAlertData> OnEnter = null;
        //提示
        public Func<TDBaseAlertData, string> OnGetTip { get; set; }
        //详细信息
        public Func<TDBaseAlertData, string> OnGetDetail { get; set; }
        //点击后链接的界面
        public Func<UView> LinkView { get; set; }
        public Func<TDBaseAlertData, bool> OnIsValid = (x) => true;
        public Func<bool> IsActiveContinue { get; set; }
        #endregion

        #region life
        public override void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            base.OnBeAdded(selfMono, obj);
            CurTurn = 0;
            if (TipStr.IsInv())
            {
                if (!TipKey.IsInv()) 
                    TipStr = GetStr(TipKey);
                else if (OnGetTip != null) 
                    TipStr = OnGetTip?.Invoke(this);
                else 
                    TipStr = GetStr(TDID);
            }
            if (DetailStr.IsInv())
            {
                if (OnGetDetail != null)
                    DetailStr = OnGetDetail?.Invoke(this);
                else
                    DetailStr = GetDesc();
            }
            if (TitleStr.IsInv()) 
                TitleStr = GetStr(TDID);

            //播放音效
            if (SelfBaseUnit == null) return;
            if (SelfBaseUnit.IsPlayer())
            {
                if (!SFX.IsInv())
                {
                    AudioMgr.PlaySFX2D(SFX);
                }
            }
        }
        public override void OnTurnbase()
        {
            base.OnTurnbase();
            CurTurn++;
        }
        #endregion

        #region get
        public Sprite GetBg() =>BaseGlobal.RsIcon.Get(Bg,false);
        public Sprite GetIllustration() => BaseGlobal.RsIllustration.Get(Illustration);
        public T GetParam<T>(int index)
        {
            if (AddtionParam == null || AddtionParam.Length <= 0) return default;
            return (T)AddtionParam[index];
        }
        #endregion

        #region set
        public void RemoveSelf() => AlertMgr.Remove(ID);
        public override TClass Copy<TClass>()
        {
            var ret = base.Copy<TClass>();
            var v2 = ret as TDBaseAlertData;
            v2.AlertMgr = AlertMgr;
            v2.Cast = Cast;
            return ret;
        }
        public TDBaseAlertData SetTipStr(string key, params object[] ps)
        {
            TipStr = Util.GetStr(key, ps);
            return this;
        }
        public TDBaseAlertData SetTitleStr(string key, params object[] ps)
        {
            TitleStr = Util.GetStr(key, ps);
            return this;
        }
        public TDBaseAlertData SetDetailStr(string key, params object[] ps)
        {
            DetailStr = Util.GetStr(key, ps);
            return this;
        }
        public TDBaseAlertData SetParam(params object[] ps)
        {
            AddtionParam = ps;
            return this;
        }
        public TDBaseAlertData SetIllustration(string str)
        {
            Illustration = str;
            return this;
        }
        public TDBaseAlertData SetSFX(string str)
        {
            StartSFX = str;
            return this;
        }
        public TDBaseAlertData SetIcon(string str)
        {
            Icon = str;
            return this;
        }
        /// <summary>
        /// 自动设置以下内容
        /// SetTitleStr,SetTipStr,SetDetailStr(ps),SetIllustration
        /// </summary>
        /// <param name="baseKey"></param>
        /// <param name="illustration"></param>
        /// <param name="descPS"></param>
        /// <returns></returns>
        public TDBaseAlertData SetAuto(string baseKey, string illustration, string sfx, params object[] descPS)
        {
            string tipKey = SysConst.Prefix_Tip + baseKey;
            string detailKey = SysConst.Prefix_Desc + baseKey;
            if (!BaseLangMgr.IsContain(tipKey))
                tipKey = baseKey;
            SetTitleStr(baseKey);
            SetTipStr(tipKey, descPS);
            SetDetailStr(detailKey, descPS);
            SetIllustration(illustration);
            SetSFX(sfx);
            if (BaseGlobal.RsIcon.IsHave(baseKey))
                SetIcon(baseKey);
            return this;
        }
        #endregion

        #region is
        // 是否结束
        public virtual bool IsOver()
        {
            if (TotalTurn <= 0) return false;
            if (CurTurn > TotalTurn) return true;
            return false;
        }
        // 判断时间是否快结束了,执行一次
        public virtual bool IsCommingTimeOut()
        {
            if (IsCommingTimeOutFalg) return false;
            if (TotalTurn <= 0) return false;
            if (CurTurn >= (int)(TotalTurn * 0.8f)) return true;
            return false;
        }
        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;
            if (Type == AlertType.Continue) 
                return true;
            if (IsOver()) 
                return false;
            if (OnIsValid == null) 
                return true;
            return OnIsValid.Invoke(this);
        }
        #endregion

        #region Callback
        // 通知超时后未处理
        public virtual void OnTimeOut() { }
        // 通知即将超时
        public virtual void OnCommingTimeOut() => IsCommingTimeOutFalg = true;
        // 通知合并
        public virtual void OnMerge()
        {
            CurTurn = 0;
            IsCommingTimeOutFalg = false;
        }
        #endregion

        #region do
        public void DoLeftClickTrigger()
        {
            OnClick?.Invoke(this);
            LinkView?.Invoke().Show();
            if (Type == AlertType.Disposable)
                RemoveSelf();
            else if (Type == AlertType.Interaction)
            {
                if (!IsValid())
                    RemoveSelf();
            }
        }
        public virtual void DoClick(UControl c, PointerEventData p)
        {
            if (p.button == PointerEventData.InputButton.Left)
            {
                DoLeftClickTrigger();
            }
            else if (p.button == PointerEventData.InputButton.Right)
            {
                if (Type == AlertType.Disposable)
                    RemoveSelf();
            }
        }
        public virtual void DoEnter()
        {
            if (OnEnter != null) OnEnter?.Invoke(this);
            else UTooltipView.Default?.ShowStr(TipStr);
        }
        #endregion
    }
}