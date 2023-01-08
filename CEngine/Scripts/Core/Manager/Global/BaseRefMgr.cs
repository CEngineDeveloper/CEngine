//------------------------------------------------------------------------------
// BaseReferenceMgr.cs
// Copyright 2020 2020/5/31 
// Created by CYM on 2020/5/31
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CYM
{
    public class BaseRefMgr : BaseGFlowMgr
    {
        #region prop
        public Dictionary<string, Func<BaseUnit, float>> UnitFloat { get; private set; } = new Dictionary<string, Func<BaseUnit, float>>();
        public Dictionary<string, Func<BaseUnit, string>> UnitString { get; private set; } = new Dictionary<string, Func<BaseUnit, string>>();
        public Dictionary<string, Func<string>> String { get; private set; } = new Dictionary<string, Func<string>>();
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            UnitFloat.Clear();
            UnitString.Clear();
            String.Clear();
        }
        #endregion

        #region set
        public void AddUnitFloat(string key, Func<BaseUnit, float> data)
        {
            if (UnitFloat.ContainsKey(key)) return;
            UnitFloat.Add(key, data);
        }
        public void AddUnitString(string key, Func<BaseUnit, string> data)
        {
            if (UnitString.ContainsKey(key)) return;
            UnitString.Add(key, data);
        }
        public void AddString(string key, Func<string> data)
        {
            if (String.ContainsKey(key)) return;
            String.Add(key, data);
        }
        #endregion

        #region get
        public float GetUnitFloat(string key, BaseUnit unit)
        {
            if (unit == null)
            {
                CLog.Error("BaseRefMgr:GetUnitFloat:Unit为Null");
                return 0;
            }
            if (!UnitFloat.ContainsKey(key))
            {
                CLog.Error("BaseRefMgr:GetUnitFloat:没有RefKey:{0}", key);
                return 0;
            }
            return UnitFloat[key].Invoke(unit);
        }
        public string GetUnitString(string key, BaseUnit unit)
        {
            if (unit == null)
            {
                CLog.Error("BaseRefMgr:GetUnitFloat:Unit为Null");
                return SysConst.STR_Inv;
            }
            if (!UnitString.ContainsKey(key))
            {
                CLog.Error("BaseRefMgr:GetUnitString:没有RefKey:{0}", key);
                return SysConst.STR_Inv;
            }
            return UnitString[key].Invoke(unit);
        }
        public string GetString(string key)
        {
            if (!String.ContainsKey(key))
            {
                CLog.Error("BaseRefMgr:GetString:没有RefKey:{0}", key);
                return SysConst.STR_Inv;
            }
            return String[key].Invoke();
        }
        #endregion
    }
}