//------------------------------------------------------------------------------
// BaseUnitMgr.cs
// Copyright 2021 2021/4/2 
// Created by CYM on 2021/4/2
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CYM
{
    public class BaseUnitMgr<TUnit, TConfig, TOwner> : BaseUFlowMgr<TOwner>, IUnitMgr
        where TUnit :BaseUnit
        where TConfig : TDBaseData, new()
        where TOwner : BaseUnit
    {
        #region Callback val
        // 增加实体
        public event Callback<TUnit> Callback_OnAddData;
        // 减少实体
        public event Callback<TUnit> Callback_OnRemoveData;
        //数据变化
        public event Callback<TUnit> Callback_OnDataChanged;
        public event Callback Callback_OnCalcAveragePos;
        #endregion

        #region prop
        public Type UnitType { get; private set; }
        public Type ConfigType { get; private set; }
        public BaseUnitSpawnMgr<TUnit, TConfig> GMgr { get; private set; }
        public Vector3 AveragePos { get; private set; }
        public IDDicList<TUnit> Data { get; protected set; } = new IDDicList<TUnit>();
        public IDDicList<BaseUnit> BaseData { get; protected set; } = new IDDicList<BaseUnit>();
        public int Count => Data.Count;
        public TUnit Gold => GMgr.Gold;
        protected ITDConfig ITDConfig { get; private set; }
        #endregion

        #region life
        protected virtual List<TUnit> AvgPosData => Data;
        public override void OnCreate()
        {
            base.OnCreate();
            UnitType = typeof(TUnit);
            ConfigType = typeof(TConfig);
            ITDConfig = BaseLuaMgr.GetTDConfig(ConfigType);
            GMgr = BaseGlobal.GetUnitSpawnMgr(UnitType) as BaseUnitSpawnMgr<TUnit, TConfig>;
            if (GMgr == null)
            {
                CLog.Error("错误! GMgr(BaseUnitSpawnMgr)为空:{0},{1}", UnitType.ToString(), ConfigType.ToString());
            }
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            CalcAveragePos();
        }
        #endregion

        #region set
        public TUnit Add(string tdid)
        {
            TUnit ret = GMgr.GetUnit(tdid);
            AddToData(ret);
            return ret;
        }
        public TUnit Add(long rtid)
        {
            TUnit ret = GMgr.GetUnit(rtid);
            AddToData(ret);
            return ret;
        }
        //创建实体，并且添加数据
        public virtual TUnit SpawnNew(string tdid, UnitSpawnParam param)
        {
            TUnit temp = GMgr.SpawnNew(tdid, param);
            AddToData(temp);
            OnSpawned(temp);
            GMgr.Callback_OnUnitMgrAdded?.Invoke(temp);
            return temp;
        }
        //并不删除实体，而是删除数据
        public virtual void Despawn(TUnit unit)
        {
            if (unit == null)
            {
                CLog.Error("unit 为 null");
                return;
            }
            if (!Data.ContainsValue(unit)) 
                return;
            RemoveFromData(unit);
            GMgr.Callback_OnUnitMgrRemoved?.Invoke(unit);
        }
        protected virtual void AddToData(TUnit unit)
        {
            if (GMgr.Sets.Contains(unit))
            {
                if (IsNewGame)
                    CLog.Error("GMgr,错误!重复添加数据:" + unit.GetName());
                return;
            }
            if (Data.Contains(unit))
            {
                CLog.Error("Data,错误!重复添加数据:" + unit.GetName());
                return;
            }
            GMgr.Sets.Add(unit);
            BaseData.Add(unit);
            Data.Add(unit);
            unit.SetOwner(SelfUnit);
            unit.UnitMgr = this;
            OnDataChanged(unit);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnAddData?.Invoke(unit);
                Callback_OnDataChanged?.Invoke(unit);
            }
        }
        protected virtual void RemoveFromData(TUnit unit)
        {
            GMgr.Sets.Remove(unit);
            BaseData.Remove(unit);
            Data.Remove(unit);
            OnDataChanged(unit);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnRemoveData?.Invoke(unit);
                Callback_OnDataChanged?.Invoke(unit);
            }
        }
        #endregion

        #region is
        public bool IsHave(TUnit unit)
        {
            if (unit == null) return false;
            return Data.ContainsID(unit.ID);
        }
        public bool IsHave() => Count > 0;
        #endregion

        #region get
        public TUnit Get(string tdid)=> Data.Get(tdid);
        public TUnit Get(long id)=> Data.Get(id);
        public TConfig GetTableConfig(string tdid) => ITDConfig.Get<TConfig>(tdid);
        public TConfig CopyTableConfig(string tdid) => ITDConfig.Get<TConfig>(tdid).Copy<TConfig>();
        #endregion

        #region Callback
        protected virtual void OnSpawned(TUnit unit) { }
        protected virtual void OnDataChanged(TUnit unit) { }
        protected virtual void OnOccupied(TUnit unit) { }
        protected virtual void OnPostOccupied(TUnit unit)=> CalcAveragePos();
        protected virtual void OnBeOccupied(TUnit unit) { }
        protected virtual void OnBePostOccupied(TUnit unit)=> CalcAveragePos();
        protected virtual void OnCalcAveragePos() { }
        #endregion

        #region interface
        public Vector3 CalcAveragePos()
        {
            AveragePos = Util.AveragePos(AvgPosData);
            Callback_OnCalcAveragePos?.Invoke();
            OnCalcAveragePos();
            return AveragePos;
        }
        BaseUnit IUnitMgr.Add(string tdid) => Add(tdid);
        BaseUnit IUnitMgr.Add(int rtid) => Add(rtid);
        BaseUnit IUnitMgr.SpawnNew(string id, UnitSpawnParam param) => SpawnNew(id, param);
        void IUnitMgr.Despawn(BaseUnit legion) => Despawn(legion as TUnit);
        void IUnitMgr.Occupied(BaseUnit unit)=> throw new NotImplementedException();
        void IUnitMgr.SortByScore() => Data = new IDDicList<TUnit>(Data.OrderByDescending((x) => x.Score).ToList());
        bool IUnitMgr.IsHave() => Count > 0;
        bool IUnitMgr.IsHave(BaseUnit unit) => IsHave(unit as TUnit);
        bool IUnitMgr.IsHave(string tdid) => Data.ContainsTDID(tdid);
        #endregion

        #region DB
        public void LoadDBData(ref List<long> datas)
        {
            datas.ForEach(x => Add(x));
        }
        public void SaveDBData(ref List<long> datas)
        {
            datas = Data.Select((x) => x.ID).ToList();
        }
        public void LoadDBData(ref List<string> datas)
        {
            datas.ForEach(x => Add(x));
        }
        public void SaveDBData(ref List<string> datas)
        {
            datas = Data.Select((x) => x.TDID).ToList();
        }
        #endregion
    }
}