//------------------------------------------------------------------------------
// BaseTDMgr.cs
// Copyright 2019 2019/5/14 
// Created by CYM on 2019/5/14
// Owner: CYM
// TableData 数据管理器,用来管理表格数据对象(产生继承于BaseConfig的对象)
// 只能作为Unit组件使用,需要配合BaseTDSpawnMgr
// 适合既需要被Unit管理又需要被Global管理的数据使用,比如(人物系统),全局有一套数据,本地也有一套数据
//------------------------------------------------------------------------------

using System;
using UnityEngine;
namespace CYM
{
    public class BaseTDMgr<TData, TUnit> : BaseUFlowMgr<TUnit>, ITDMgr
        where TUnit : BaseUnit
        where TData : TDBaseData, new()
    {

        #region Callback
        public event Callback<TData> Callback_OnDataChanged;
        #endregion

        #region prop
        public Type DataType { get; private set; }
        public IDDicList<TData> Data { get; protected set; } = new IDDicList<TData>();
        #endregion

        #region life
        protected virtual BaseTDSpawnMgr<TData> GMgr { get; private set; }
        public override void OnCreate()
        {
            base.OnCreate();
            DataType = typeof(TData);
            GMgr = BaseGlobal.GetTDSpawnerMgr(DataType) as BaseTDSpawnMgr<TData>;
            if (GMgr == null)
            {
                CLog.Error("错误! GMgr为空:{0},{1}", DataType.ToString(), DataType.ToString());
            }
        }
        #endregion

        #region set
        public virtual TDBaseData Spawn(string id)
        {
            TData temp = GMgr.Spawn(id,new UnitSpawnParam { spwanPoint = Vector3.zero,quaternion = Quaternion.identity,team = 0 });
            OnSpawned(temp);
            Add(temp);
            GMgr.Callback_OnUnitMgrAdded?.Invoke(temp);
            return temp;
        }
        public virtual void Despawn(TDBaseData data)
        {
            if (data == null) return;
            Remove(data as TData);
            GMgr.Despawn(data);
            GMgr.Callback_OnUnitMgrRemoved?.Invoke(data as TData);
        }
        public virtual TData Add(TData data)
        {
            data.TDMgr = this;
            Data.Add(data);
            Callback_OnDataChanged?.Invoke(data);
            OnDataChanged(data);
            return data;
        }
        public virtual void Remove(TData data)
        {
            Data.Remove(data);
            Callback_OnDataChanged?.Invoke(data);
            OnDataChanged(data);
        }
        #endregion

        #region occupie
        public virtual void Occupied(TData data)
        {
            if (data == null) 
                return;
            if (IsHave(data))
            {
                CLog.Error("{0},已经拥有此数据:{1}", SelfBaseUnit.TDID, data.TDID);
            }
            else
            {
                var targetMgr = data.TDMgr as BaseTDMgr<TData, TUnit>;
                if (targetMgr == null)
                {
                    CLog.Error("错误! Data.TDMgr 为 null:{0}", DataType);
                    return;
                }
                targetMgr.Remove(data);
                Add(data);
                GMgr.Callback_OnUnitMgrOccupied?.Invoke(data);
            }
        }
        #endregion

        #region is
        public bool IsHave(TData data)
        {
            if (data == null)
                return false;
            return Data.ContainsID(data.ID);
        }
        #endregion

        #region Callback
        protected virtual void OnSpawned(TData data) { }
        public virtual void OnDataChanged(TData data) { }
        #endregion
    }
}