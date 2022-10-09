//------------------------------------------------------------------------------
// BaseLoggerMgr.cs
// Copyright 2020 2020/2/29 
// Created by CYM on 2020/2/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [Serializable]
    public class BaseLogData
    {
        public long ID;
        public string Date;
        public string Desc;
        public float CurTime = 0;

        public string GetDesc()
        {
            return string.Format("{0}", Desc);
        }
    }
    public class BaseLoggerMgr : BaseGFlowMgr
    {
        IScreenMgr<BaseUnit> ScreenMgr => BaseGlobal.ScreenMgr;

        public Timer UpdateTimer = new Timer(0.05f);
        public readonly int MaxCount = int.MaxValue;
        public event Callback<BaseLogData> Callback_OnAddedLog;
        public event Callback<BaseLogData> Callback_OnRemoveLog;
        public List<BaseLogData> Data = new List<BaseLogData>();

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (Data.Count > 0)
            {
                var temp = Data[Data.Count-1];
                temp.CurTime += Time.fixedDeltaTime;
                if (temp.CurTime >= 1.5f)
                    RemLog(temp);
            }
        }
        #endregion

        #region set
        public void AddLog(string key, params object[] objs)
        {
            AddLog(key,ScreenMgr.Player, objs);
        }
        public void AddLog(string key, BaseUnit nation = null, params object[] objs)
        {
            if (!BaseGlobal.IsUnReadData) return;
            if (nation != null)
            {
                if (!nation.IsPlayer())
                    return;
            }
            BaseLogData tempData = new BaseLogData();
            tempData.Date = BaseGlobal.DateTimeMgr.GetCurDateStr();
            tempData.Desc = Util.GetStr(key, objs);
            tempData.ID = IDUtil.Gen();
            tempData.CurTime = 0;
            Data.Insert(0, tempData);
            if (Data.Count > MaxCount) RemLastLog();
            Callback_OnAddedLog?.Invoke(tempData);
        }
        public void RemLog(BaseLogData data)
        {
            if (!BaseGlobal.IsUnReadData) return;
            var lastIndex = Data.Count - 1;
            if (lastIndex < 0) return;
            Data.Remove(data);
            Callback_OnRemoveLog?.Invoke(data);
            UpdateTimer.Restart();
        }
        public void RemLastLog()
        {
            if (!BaseGlobal.IsUnReadData) return;
            var lastIndex = 0;
            if (Data.Count < 0) return;
            var last = Data[lastIndex];
            Data.RemoveAt(lastIndex);
            Callback_OnRemoveLog?.Invoke(last);
        }
        #endregion
    }
}