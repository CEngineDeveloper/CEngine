//------------------------------------------------------------------------------
// BaseAlertMgr.cs
// Copyright 2019 2019/3/1 
// Created by CYM on 2019/3/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace CYM.Unit
{
    public enum AlertType
    {
        Continue,         //持续
        Interaction,      //交互
        Disposable,       //一次性
    }
    public class BaseAlertMgr<TData> : BaseMgr, IAlertMgr<TData>, IDBListConverMgr<DBBaseAlert> 
        where TData : TDBaseAlertData, new()
    {
        #region Callback
        public event Callback<TData> Callback_OnAdded;
        public event Callback<TData> Callback_OnRemoved;
        public event Callback<TData> Callback_OnMerge;
        public event Callback<TData> Callback_OnCommingTimeOut;
        public event Callback<TData> Callback_OnInteractionChange;
        public event Callback<TData> Callback_DisposableChange;
        public event Callback<TData> Callback_ContinueChange;
        #endregion

        #region public
        public IList RawData => Data;
        public List<TData> Data { get; private set; } = new List<TData>();
        public List<TData> InteractionData { get; private set; } = new List<TData>();
        public List<TData> DisposableData { get; private set; } = new List<TData>();
        public IDDicList<TData> ContinueData { get; private set; } = new IDDicList<TData>();
        public List<TData> TDContinueData { get; private set; } = new List<TData>();
        #endregion

        #region prop
        protected BaseUnit Player => BaseGlobal.ScreenMgr.Player;
        protected List<TData> ClearData = new List<TData>();
        const string CommonAlert = "Alert_Common";
        protected bool IsDirtyUpdateContinueAlert = false;
        Timer UpdateAlertTimer = new Timer(1.0f);
        ITDConfig ITDConfig;
        TData CommonAlertData = new TData {
            TDID = CommonAlert,
            Icon = "Empty",
            Bg = "",
            TotalTurn = 30,
            Type = AlertType.Disposable,
        };
        #endregion

        #region life
        protected virtual bool NeedUpdateAlertTimer => false;
        public sealed override MgrType MgrType => MgrType.Unit;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedTurnbase = true;
            NeedFixedUpdate = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
            foreach (var item in ITDConfig.ListObjValues)
            {
                var data = item as TData;
                if (data.Type == AlertType.Continue)
                    TDContinueData.Add(data);
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
            SelfBaseUnit.Callback_OnBeSetPlayer += OnBeSetPlayer;
            SelfBaseUnit.Callback_OnUnBeSetPlayer += OnUnBeSetPlayer;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            SelfBaseUnit.Callback_OnBeSetPlayer -= OnBeSetPlayer;
            SelfBaseUnit.Callback_OnUnBeSetPlayer -= OnUnBeSetPlayer;
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (SelfBaseUnit.IsPlayer())
            {
                if (IsDirtyUpdateContinueAlert ||
                    (UpdateAlertTimer.CheckOver() && NeedUpdateAlertTimer))
                {
                    OnUpdateContinueAlert();
                    IsDirtyUpdateContinueAlert = false;
                }
            }
        }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
            if (SelfBaseUnit.IsPlayer())
                SetContinueAlertDirty();
        }
        public override void OnTurnbase(bool day, bool month, bool year)
        {
            base.OnTurnbase(day,month,year);
            if (day)
            {
                if (SelfBaseUnit.IsPlayer())
                    SetContinueAlertDirty();
            }
        }
        protected virtual void OnUpdateContinueAlert()
        {
            foreach (var item in TDContinueData)
            {
                SetContinueAlert(item.TDID, item.IsActiveContinue);
            }
        }
        public void SetContinueAlertDirty()
        {
            IsDirtyUpdateContinueAlert = true;
        }
        #endregion

        #region Manual Update
        protected void ManualUpdateAlert()
        {
            ClearData.Clear();
            foreach (var item in Data)
            {
                item.OnTurnbase();
                if (!item.IsValid())
                {
                    ClearData.Add(item);
                }
                if (item.IsCommingTimeOut())
                {
                    item.OnCommingTimeOut();
                    Callback_OnCommingTimeOut?.Invoke(item);
                }
            }
            for (int i = 0; i < ClearData.Count; ++i)
            {
                ClearData[i].OnTimeOut();
                Remove(ClearData[i]);
            }
        }
        #endregion

        #region add common
        public TData AddScenario(string baseKey, string illustration, string sfx, params object[] descPS)
        {
            var data = AddCommon((x)=> {
                x.SetAuto(baseKey, illustration, sfx, descPS);
                x.OnClick = (alertData) =>
                {
                    UScenarioDlgView.Default?.Show(alertData);
                };
            }, 
            null, true);

            return data;
        }
        public TData AddAutoTrigger(Callback<TData> action = null)
        {
            return AddCommon(action, null,true);
        }
        public TData AddAutoTrigger(string baseKey, string illustration, string sfx, params object[] descPS)
        {
            return AddAutoTrigger((x) => x.SetAuto(baseKey, illustration, sfx, descPS));
        }
        public TData AddCommon(string baseKey, string illustration, string sfx, params object[] descPS)
        {
            return AddCommon((x) => x.SetAuto(baseKey, illustration, sfx, descPS));
        }
        public TData AddCommon(Callback<TData> action = null)
        {
            return AddCommon(action,null, false);
        }
        public TData AddCommon(Callback<TData> action = null, BaseUnit cast = null, bool isAutoTrigger = false)
        {
            if (!IsCanAddDisposableAlert()) return null;
            return Add(CommonAlertData, cast,action,isAutoTrigger);
        }
        public TData AddInteraction(string tdid, BaseUnit cast = null, Callback<TData> action = null)
        {
            return Add(tdid, cast, action);
        }
        #endregion

        #region set
        public void TriggerFirstDisposable()
        {
            var data = GetDisposable();
            data.DoLeftClickTrigger();
        }
        protected void SetContinueAlert(string tdid, Func<bool> isTrigger)
        {
            if (isTrigger == null) return;
            if (isTrigger.Invoke())
            {
                if (!ContinueData.ContainsTDID(tdid))
                    Add(tdid);
            }
            else Remove(tdid);
        }
        private TData Add(string tdid, BaseUnit cast = null, Callback<TData> action = null, bool isAutoTrigger = false)
        {
            if (CommonAlert == tdid)
            {
                return Add(CommonAlertData, cast, action, isAutoTrigger);
            }
            else
            {
                if (!ITDConfig.Contains(tdid))
                {
                    CLog.Error("没有:{0},请手动添加Alert", tdid);
                    return null;
                }
                TData sourceAlert = ITDConfig.Get<TData>(tdid);
                return Add(sourceAlert, cast, action, isAutoTrigger);
            }
        }
        private TData Add(TData sourceAlert, BaseUnit cast = null, Callback<TData> action = null, bool isAutoTrigger = false)
        {
            sourceAlert.Cast = cast ? cast : Player;
            sourceAlert.AlertMgr = this;

            //判断通知是否可以被合并
            var finalAlert = CanMerge(sourceAlert);
            if (finalAlert != null)
            {
                finalAlert.OnMerge();
                Callback_OnMerge?.Invoke(finalAlert);
            }
            else
            {
                finalAlert = sourceAlert.Copy<TData>();
                action?.Invoke(finalAlert);
                finalAlert.ID = IDUtil.Gen();
                finalAlert.OnBeAdded(SelfBaseUnit);
                Data.Add(finalAlert);
                if (finalAlert.Type == AlertType.Interaction)
                {
                    InteractionData.Add(finalAlert);
                    Callback_OnInteractionChange?.Invoke(finalAlert);
                }
                else if (finalAlert.Type == AlertType.Disposable)
                {
                    DisposableData.Add(finalAlert);
                    Callback_DisposableChange?.Invoke(finalAlert);
                }
                else if (finalAlert.Type == AlertType.Continue)
                {
                    ContinueData.Add(finalAlert);
                    Callback_ContinueChange?.Invoke(finalAlert);
                }
                if (BaseGlobal.IsUnReadData)
                {
                    Callback_OnAdded?.Invoke(finalAlert);
                }
            }
            if (finalAlert.IsAutoTrigger || isAutoTrigger)
            {
                finalAlert.DoLeftClickTrigger();
            }
            return finalAlert;
        }
        public void Remove(TData alert)
        {
            if (alert == null) return;
            alert.OnBeRemoved();
            Data.Remove(alert);
            if (alert.Type == AlertType.Interaction)
            {
                InteractionData.Remove(alert);
                Callback_OnInteractionChange?.Invoke(alert);
            }
            else if (alert.Type == AlertType.Disposable)
            {
                DisposableData.Remove(alert);
                Callback_DisposableChange?.Invoke(alert);
            }
            else if (alert.Type == AlertType.Continue)
            {
                ContinueData.Remove(alert);
                Callback_ContinueChange?.Invoke(alert);
            }
            if (BaseGlobal.IsUnReadData)
                Callback_OnRemoved?.Invoke(alert);
        }
        public void Remove(long id) => Remove(Data.Find((x) => { return id == x.ID; }));
        public void Remove(string tdid) => Remove(Data.Find((x) => { return tdid == x.TDID; }));
        public TData GetAlert(long id) => Data.Find((x) => { return id == x.ID; });
        #endregion

        #region get
        public TData GetInteraction()
        {
            if (!IsHaveInteraction()) return null;
            return InteractionData[0];
        }
        public TData GetDisposable()
        {
            if (!IsHaveDisposable()) return null;
            return DisposableData[0];
        }
        #endregion

        #region is
        private bool IsCanAddDisposableAlert()
        {
            if (SelfBaseUnit == null) return false;
            if (SysConsole.Ins.IsAllAlert) return true;
            if (!SelfBaseUnit.IsPlayer()) return false;
            return true;
        }
        public bool IsHaveInteraction()
        {
            return InteractionData.Count > 0;
        }
        public bool IsHaveDisposable()
        {
            return DisposableData.Count > 0;
        }

        #endregion

        #region Cache
        // 是否可以被合并，相同的Alert将会被合并
        private TData CanMerge(TData alert)
        {
            for (int i = 0; i < Data.Count; ++i)
            {
                var item = Data[i];
                //普通的通知id相同就合并
                if (alert.Type == AlertType.Continue &&
                    item.Type == AlertType.Continue)
                {
                    if (alert.TDID == item.TDID)
                        return item;
                }
                //外交alert要国家相同才行
                else if (alert.Type == AlertType.Interaction &&
                    item.Type == AlertType.Interaction)
                {
                    if (alert.TDID == item.TDID &&
                        alert.Cast == item.Cast)
                        return item;
                }
                //回信Alert不做合并
                else if (alert.Type == AlertType.Disposable &&
                    item.Type == AlertType.Disposable)
                {
                    return null;
                }
            }
            return null;
        }
        #endregion

        #region Callback
        protected virtual void OnBeSetPlayer()
        {
            SetContinueAlertDirty();
            BaseUIMgr.Callback_OnControlClick += OnControlClick;
        }
        private void OnUnBeSetPlayer()
        {
            BaseUIMgr.Callback_OnControlClick -= OnControlClick;
        }

        private void OnControlClick(UControl arg1, PointerEventData arg2)
        {
            SetContinueAlertDirty();
        }
        #endregion

        #region DB
        public void SaveDBData(ref List<DBBaseAlert> ret)
        {
            ret = new List<DBBaseAlert>();
            foreach (var item in Data)
            {
                if (item.Type == AlertType.Continue)
                    continue;
                DBBaseAlert temp = new DBBaseAlert();
                temp.ID = item.ID;
                temp.TDID = item.TDID;
                temp.Cast = item.Cast.ID;
                temp.CurTurn = item.CurTurn;
                temp.IsCommingTimeOutFalg = item.IsCommingTimeOutFalg;
                temp.TipStr = item.TipStr;
                temp.DetailStr = item.DetailStr;
                temp.TitleStr = item.TitleStr;
                temp.Illustration = item.Illustration;
                temp.Type = item.Type;
                temp.StartSFX = item.StartSFX;
                temp.Bg = item.Bg;
                temp.Icon = item.Icon;
                temp.IsAutoTrigger = item.IsAutoTrigger;
                ret.Add(temp);
            }
        }
        public void LoadDBData(ref List<DBBaseAlert> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                if (item.Type == AlertType.Continue)
                    continue;
                var alert = Add(item.TDID, GetEntity(item.Cast));
                alert.TipStr = item.TipStr;
                alert.DetailStr = item.DetailStr;
                alert.TitleStr = item.TitleStr;
                alert.Illustration = item.Illustration;
                alert.CurTurn = item.CurTurn;
                alert.ID = item.ID;
                alert.IsCommingTimeOutFalg = item.IsCommingTimeOutFalg;
                alert.StartSFX = item.StartSFX;
                alert.IsAutoTrigger = item.IsAutoTrigger;
                alert.Bg = item.Bg;
                alert.Icon = item.Icon;
            }
        }
        #endregion
    }
}