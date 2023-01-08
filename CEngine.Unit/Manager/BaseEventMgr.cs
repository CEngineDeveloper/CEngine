//------------------------------------------------------------------------------
// BaseEventMgr.cs
// Copyright 2019 2019/8/5 
// Created by CYM on 2019/8/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Unit;
using System.Collections.Generic;

namespace CYM.Unit
{
    public class BaseEventMgr<TData> : BaseMgr, IDBListConverMgr<DBBaseEvent> , IEventMgr<TData>
        where TData : TDBaseEventData, new()
    {
        #region Callback
        public event Callback<TData> Callback_OnEventAdded;
        public event Callback<TData> Callback_OnEventRemoved;
        public event Callback<TData> Callback_OnEventChange;
        #endregion

        #region prop
        BaseConditionMgr ACMgr => BaseGlobal.ACM;
        public IDDicList<TData> Data { get; private set; } = new IDDicList<TData>();
        public Dictionary<string, CD> EventCD { get; protected set; } = new Dictionary<string, CD>();
        static int MaxRandCount = 5;
        int CurRandCount = 0;
        ITDConfig ITDConfig;
        List<string> clearEventss = new List<string>();
        protected virtual float GlobalProp => 1.0f;
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        #endregion

        #region Manual Update
        protected void ManualUpdateEvent()
        {
            Rand();
            clearEventss.Clear();
            foreach (var item in EventCD)
            {
                item.Value.Update();
                if (item.Value.IsOver())
                    clearEventss.Add(item.Key);
            }
            foreach (var item in clearEventss)
                EventCD.Remove(item);
        }
        #endregion

        #region get
        public TData Get(int id)
        {
            if (!Data.ContainsID(id))
                return null;
            return Data[id] as TData;
        }
        public TData First()
        {
            if (Data.Count > 0)
                return Data[0] as TData;
            return null;
        }
        public int Count()
        {
            return Data.Count;
        }
        #endregion

        #region set
        int CurEventIndex = 0;
        public TData TestNext()
        {
            if (ITDConfig.ListKeys.Count == 0) return null;
            if (CurEventIndex >= ITDConfig.ListKeys.Count) return null;
            string key = ITDConfig.ListKeys[CurEventIndex];
            CurEventIndex++;
            return Add(key);
        }
        public TData Rand()
        {
            if (!RandUtil.Rand(GlobalProp))
                return null;
            if (CurRandCount >= MaxRandCount)
            {
                CurRandCount = 0;
                return null;
            }
            CurRandCount++;
            if (SysConsole.Ins.IsNoEvent) return null;
            if (!SelfBaseUnit.IsPlayerCtrl()) return null;
            if (ITDConfig.ListKeys.Count == 0) return null;

            string key = ITDConfig.ListKeys.Rand();
            TData config = ITDConfig.Get<TData>(key);
            //判断事件的触发条件
            if (IsInTarget(config))
            {
                //判断事件的概率
                if (IsInProp(config))
                {
                    CurRandCount = 0;
                    if (config.CD > 0)
                    {
                        if (!EventCD.ContainsKey(key))
                            EventCD.Add(key, new CD());
                        EventCD[key] = new CD(config.CD);
                    }
                    return Add(config.TDID);
                }
                else
                {
                    return null;
                }
            }
            return Rand();
        }
        public TData Add(string eventDlgName)
        {
            if (!ITDConfig.Contains(eventDlgName)) return null;
            TData tempEventDlg = ITDConfig.Get<TData>(eventDlgName).Copy<TData>();
            if (tempEventDlg == null)
            {
                CLog.Error("未找到EventDlg errorId=" + eventDlgName);
                return null;
            }
            tempEventDlg.ID = IDUtil.Gen();
            tempEventDlg.OnBeAdded(SelfBaseUnit);
            Data.Add(tempEventDlg);
            Callback_OnEventAdded?.Invoke(tempEventDlg);
            Callback_OnEventChange?.Invoke(tempEventDlg);
            return tempEventDlg;
        }
        public void Remove(TDBaseEventData eventDlg)
        {
            if (eventDlg == null) return;
            Data.Remove(eventDlg as TData);
            Callback_OnEventRemoved?.Invoke(eventDlg as TData);
            Callback_OnEventChange?.Invoke(eventDlg as TData);
        }
        public void SelOption(TDBaseEventData eventData, EventOption option)
        {
            SelOption(eventData, option.Index);
        }
        public void SelOption(TDBaseEventData eventData, int index)
        {
            eventData.DoSelOption(index);
            Remove(eventData);
        }
        #endregion

        #region is
        public bool IsHave() => Data.Count > 0;
        // 是否可以触发
        bool IsInProp(TData eventData)
        {
            if (SysConsole.Ins.IsMustEvent) return true;
            if (RandUtil.Rand(eventData.Prob)) return true;
            return false;
        }
        bool IsInTarget(TData eventData)
        {
            if (eventData.Targets == null) return false;
            if (EventCD.ContainsKey(eventData.TDID))
            {
                if (!EventCD[eventData.TDID].IsOver())
                    return false;
            }

            if (eventData.Targets.Count > 0)
            {
                ACMgr.Reset(SelfBaseUnit);
                ACMgr.Add(eventData.Targets);
                if (!ACMgr.IsTrue())
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region DB
        public void SaveDBData(ref List<DBBaseEvent> ret)
        {
            ret = new List<DBBaseEvent>();
            foreach (var item in Data)
            {
                DBBaseEvent temp = new DBBaseEvent();
                temp.ID = item.ID;
                temp.TDID = item.TDID;
                temp.CD = item.CD;
                ret.Add(temp);
            }
        }
        public void LoadDBData(ref List<DBBaseEvent> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                var temp = Add(item.TDID);
                temp.TDID = item.TDID;
                temp.ID = item.ID;
                temp.CD = item.CD;
            }
        }
        public void SaveEventCDDBData(ref Dictionary<string, CD> ret)
        {
            ret = EventCD;
        }
        public void LoadEventCDDBData(ref Dictionary<string, CD> eventCD)
        {
            EventCD = eventCD;
        }
        #endregion
    }
}