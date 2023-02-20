//------------------------------------------------------------------------------
// BaseTDMgr.cs
// Copyright 2018 2018/11/27 
// Created by CYM on 2018/11/27
// Owner: CYM
// TableData 数据管理器,用来产生虚拟对象(产生继承于BaseConfig的对象)
// 全局和本地都可以用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CYM
{
    public class BaseTDSpawnMgr<TData> : BaseMgr,  ITDSpawnMgr<TData>, IDBListConvertMgr<TData>
        where TData : TDBaseData, new()
    {
        #region ISpawnMgr
        public TData Gold { get; protected set; }
        public IDDicList<TData> Data { get; protected set; } = new IDDicList<TData>();
        public event Callback<TData> Callback_OnAdd;
        public event Callback<TData> Callback_OnSpawnGold;
        public event Callback<TData> Callback_OnSpawn;
        public event Callback<TData> Callback_OnDespawn;
        public event Callback<TData> Callback_OnDataChanged;
        #endregion

        #region Callback 
        public Callback<TData> Callback_OnUnitMgrAdded { get; set; }
        public Callback<TData> Callback_OnUnitMgrRemoved { get; set; }
        public Callback<TData> Callback_OnUnitMgrOccupied { get; set; }
        #endregion

        #region prop
        public int Count => Data.Count;
        protected ITDConfig ITDConfig { get; private set; }
        #endregion

        #region life
        protected virtual bool IsNoConfig => false;
        public virtual bool IsAddToGlobalSpawnerMgr => true;
        public override MgrType MgrType => MgrType.All;
        public Type UnitType { get; private set; }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
            UnitType = typeof(TData);
            if (IsGlobal)
            {
                BaseGlobal.BattleMgr.Callback_OnLoaded += OnBattleLoaded;
                BaseGlobal.BattleMgr.Callback_OnUnLoaded += OnBattleUnLoaded;
            }
        }
        public override void OnTurnbase(bool day, bool month, bool year)
        {
            base.OnTurnbase(day,month,year);
            foreach (var item in Data)
                item.OnTurnbase();
        }
        public override void OnTurnframe(int gameFramesPerSecond)
        {
            base.OnTurnframe(gameFramesPerSecond);
            foreach (var item in Data)
                item.OnTurnframe(gameFramesPerSecond);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var item in Data)
                item.ManualUpdate();
        }
        public override void OnDeath()
        {
            Clear();
            base.OnDeath();
        }
        #endregion

        #region set
        public virtual void Sort()
        {
            throw new NotImplementedException();
        }
        //加载到现有得对象中
        public void LoadAdd(TData config, DBBase data)
        {
            SpawnAdd(config,new UnitSpawnParam { tdid = data.TDID, rtid = data.ID });
        }
        //游戏开始的时候创建一个新的对象
        public virtual TData SpawnNew(string tdid, [DefaultValue(nameof(UnitSpawnParam.Default))] UnitSpawnParam param)
        {
            var ret = Spawn(tdid, param);
            ret.OnNewSpawn();
            return ret;
        }
        //创建对象
        public virtual TData Spawn(string tdid, [DefaultValue(nameof(UnitSpawnParam.Default))] UnitSpawnParam param)
        {
            if (tdid.IsInv()) return null;
            TData data;
            if (IsNoConfig || param.isNoConfig)
            {
                data = new TData();
                data.TDID = tdid;
            }
            else
            {
                TData temp = ITDConfig.Get<TData>(tdid);
                if (temp == null)
                {
                    CLog.Error($"没有这个配置:{tdid}");
                    return null;
                }
                data = temp.Copy<TData>();
            }
            SpawnAdd(data, param);
            Callback_OnSpawn?.Invoke(data);
            return data;
        }
        //添加,触发整个SpawnAdd流程
        public virtual void SpawnAdd(TData data, UnitSpawnParam param)
        {
            if (!param.tdid.IsInv()) data.TDID = param.tdid;
            else param.tdid = data.TDID;

            if (param.rtid != null)
            {
                data.ID = param.rtid.Value;
            }
            else
            {
                param.rtid = data.ID = IDUtil.Gen();
            }
            data.OnBeAdded(SelfMono);
            OnSpawned(data, param);
            AddToData(data);
        }
        // despawn
        public virtual void Despawn(TDBaseData data,float delay=0)
        {
            data.OnBeRemoved();
            RemoveFromData(data);
        }
        // 清空数据
        public virtual void Clear()=> Data.Clear();
        public virtual void OnSpawned(TData unit, [DefaultValue(nameof(UnitSpawnParam.Default))] UnitSpawnParam param) { }

        public void AddToData(TData data)
        {
            data.SetOwnerBaseUnit(SelfBaseUnit);
            Data.Add(data);
            OnDataChanged(data);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnAdd?.Invoke(data);
                Callback_OnDataChanged?.Invoke(data);
            }
        }
        public void RemoveFromData(TDBaseData data)
        {
            Data.Remove(data.ID);
            OnDataChanged(data as TData);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnDespawn?.Invoke(data as TData);
                Callback_OnDataChanged?.Invoke(data as TData);
            }
        }
        #endregion

        #region is
        public bool IsHave() => Data.Count > 0;
        public bool IsHave(TDBaseData unit)
        {
            if (unit == null) return false;
            return Data.ContainsID(unit.ID);
        }
        #endregion

        #region get
        public virtual TData GetUnit(long rtid) => Data.Get(rtid);
        public virtual TData GetUnit(string tdid) => Data.Get(tdid);
        #endregion

        #region Callback
        protected virtual void OnBattleUnLoaded() => Clear();
        protected virtual void OnBattleLoaded() { }
        public virtual void OnDataChanged(TData data) { }
        #endregion

        #region DB
        // 加载对象
        public void LoadDBData<TDBData>(ref List<TDBData> datas,Callback<TData, TDBData> action) 
            where TDBData : DBBase, new()
        {
            foreach (var data in datas)
            {
                TData temp;
                if (IsNoConfig)
                {
                    temp = new TData();
                }
                else
                {
                    temp = ITDConfig.Get<TData>(data.TDID);
                }
                if (temp == null)
                {
                    CLog.Error($"错误,没有此TDID:{data.TDID}");
                    continue;
                }
                TData ret = temp.Copy<TData>();
                Util.CopyToConfig(data, ret);
                action(ret, data);
                SpawnAdd(ret, new UnitSpawnParam { tdid = data.TDID, rtid = data.ID });
            }
        }
        //保存对象
        public void SaveDBData<TDBData>(ref List<TDBData> datas, Callback<TData, TDBData> action) 
            where TDBData : DBBase, new()
        {
            foreach (var config in Data)
            {
                TDBData ret = new TDBData();
                Util.CopyToData(config, ret);
                action(config, ret);
                datas.Add(ret);
            }
        }
        #endregion
    }
}