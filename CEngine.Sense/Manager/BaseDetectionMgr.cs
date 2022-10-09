//------------------------------------------------------------------------------
// BaseDetectionMgr.cs
// Copyright 2020 2020/8/24 
// Created by CYM on 2020/8/24
// Owner: CYM
// 其他单位的视野检测到自己的单位，需要和SenseMgr配合使用
// 不能和DetectionMgr合并，一个DetectionMgr可以对应多个SenseMgr.
//------------------------------------------------------------------------------
using System.Collections.Generic;
namespace CYM.Sense
{
    public class BaseDetectionMgr : BaseMgr
    {
        #region prop
        public HashList<BaseUnit> Units { get; private set; } = new HashList<BaseUnit>();
        public Dictionary<BaseUnit, HashList<string>> UnitsSense { get; private set; } = new Dictionary<BaseUnit, HashList<string>>();
        public Dictionary<string, HashList<BaseUnit>> TypedSenseUnits { get; private set; } = new Dictionary<string, HashList<BaseUnit>>();
        public HashList<BaseUnit> SenseUnits
        {
            get
            {
                if (!TypedSenseUnits.ContainsKey(SelfBaseUnit.SenseMgr.SenseName))
                    TypedSenseUnits.Add(SelfBaseUnit.SenseMgr.SenseName, new HashList<BaseUnit>());
                return TypedSenseUnits[SelfBaseUnit.SenseMgr.SenseName];
            }
        }
        #endregion

        #region Callback
        public event Callback<BaseUnit> Callback_OnDetectionAdd;
        public event Callback<BaseUnit> Callback_OnDetectionRemove;
        public event Callback<BaseUnit> Callback_OnDetectionChange;
        #endregion

        #region life
        public override void OnDisable()
        {
            base.OnDisable();
            Units.Clear();
            UnitsSense.Clear();
            TypedSenseUnits.Clear();
        }
        #endregion

        #region set
        public void Add(ISenseMgr sense, BaseUnit unit)
        {
            if (!UnitsSense.ContainsKey(unit)) 
                UnitsSense.Add(unit, new HashList<string>());
            if (!TypedSenseUnits.ContainsKey(sense.SenseName)) 
                TypedSenseUnits.Add(sense.SenseName, new HashList<BaseUnit>());
            Units.Add(unit);
            UnitsSense[unit].Add(sense.SenseName);
            TypedSenseUnits[sense.SenseName].Add(unit);

            OnDetectionChange(unit);
            OnDetectionAdd(unit);
        }
        public void Remove(ISenseMgr sense, BaseUnit unit)
        {
            if (!UnitsSense.ContainsKey(unit)) 
                UnitsSense.Add(unit, new HashList<string>());
            if (!TypedSenseUnits.ContainsKey(sense.SenseName)) 
                TypedSenseUnits.Add(sense.SenseName, new HashList<BaseUnit>());
            UnitsSense[unit].Remove(sense.SenseName);
            TypedSenseUnits[sense.SenseName].Remove(unit);
            if (UnitsSense[unit].Count < 0)
            {
                Units.Remove(unit);
            }

            OnDetectionChange(unit);
            OnDetectionRemove(unit);
        }
        #endregion

        #region is
        //是否被指定单位检测到了
        public bool IsInDetection(BaseUnit unit) => Units.Contains(unit);
        public bool IsInDetection(string type, BaseUnit unit) => TypedSenseUnits[type].Contains(unit);
        #endregion

        #region Callback
        protected virtual void OnDetectionAdd(BaseUnit unit)
        {
            Callback_OnDetectionAdd?.Invoke(unit);
        }
        protected virtual void OnDetectionRemove(BaseUnit unit)
        {
            Callback_OnDetectionRemove?.Invoke(unit);
        }
        protected virtual void OnDetectionChange(BaseUnit unit)
        {
            Callback_OnDetectionChange?.Invoke(unit);
        }
        #endregion
    }
}