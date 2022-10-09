//------------------------------------------------------------------------------
// BaseEntityMgr.cs
// Copyright 2019 2019/4/8 
// Created by CYM on 2019/4/8
// Owner: CYM
// 附加在Entity下面地组件,用来管理子Entity对象,
// 比如附加再国家下面地军团管理器,需要和全局实体管理器配合使用(BaseGEntitySpawnMgr)
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CYM
{
    public class BaseEntityMgr<TUnit, TConfig, TDBData, TOwner> : BaseUnitMgr<TUnit,TConfig,TOwner>
        where TUnit     : BaseEntity<TUnit, TConfig, TDBData, TOwner>
        where TConfig   : TDBaseData, new()
        where TDBData   : DBBaseUnit, new()
        where TOwner    : BaseUnit
    {

        #region Callback
        // 占领实体,TUnit:实体
        public event Callback<TUnit> Callback_OnOccupied;
        //失去所有的实体,TOwner:造成方
        public event Callback<TOwner> Callback_OnLoseAll;
        #endregion

        #region prop
        TUnit capital;
        public TUnit Capital
        {
            get
            {
                if (capital == null)
                    return GMgr.Gold;
                return capital;
            }
        }
        #endregion

        #region mgr
        public BaseEntitySpawnMgr<TUnit, TConfig, TDBData, TOwner> GEntityMgr => GMgr as BaseEntitySpawnMgr<TUnit, TConfig, TDBData, TOwner>;
        #endregion

        #region is
        public bool IsOwn(TUnit unit)
        {
            if (unit == null) 
                return false;
            return Data.ContainsID(unit.ID);
        }
        public bool IsHaveCapital() => capital != null;
        //失去了所有的城市(玩家除外)
        public bool IsDestroyed() => !SelfUnit.IsPlayerCtrl() && !SelfUnit.IsSystem && !IsHave();
        #endregion

        #region is can
        public virtual bool IsCanSetCapital(TUnit castle) => true;
        #endregion

        #region set
        public virtual void Occupied(TUnit unit)
        {
            if (unit == null) return;
            if (unit.Owner == null) return;
            if (IsHave(unit))
                CLog.Error("{0},已经拥有此实体:{1}", SelfBaseUnit.TDID, unit.TDID);
            else
            {
                if (unit.IsHaveOwner())
                {
                    var oldOwner = unit.Owner;
                    var targetMgr = unit.Owner.GetUnitMgr(UnitType) as BaseEntityMgr<TUnit, TConfig, TDBData, TOwner>;
                    if (targetMgr == null)
                    {
                        CLog.Error("错误!GetEntityMgr 为null.{0}", UnitType);
                        return;
                    }
                    targetMgr.RemoveFromData(unit);
                    targetMgr.OnBeOccupied(unit);
                    AddToData(unit);
                    OnOccupied(unit);
                    Callback_OnOccupied?.Invoke(unit);
                    unit.Callback_OnBeOccupied?.Invoke(oldOwner, unit.Owner);
                    GMgr.Callback_OnUnitMgrOccupied?.Invoke(unit);
                    if (targetMgr.Count <= 0)
                    {
                        targetMgr.Callback_OnLoseAll?.Invoke(SelfUnit);
                        GEntityMgr.Callback_OnLoseAll?.Invoke(SelfUnit, targetMgr.SelfUnit);
                    }
                    targetMgr.OnBePostOccupied(unit);
                    OnPostOccupied(unit);
                }
            }
        }
        public void SetCapital(TUnit castle, bool isForce = false)
        {
            if (!isForce && !IsCanSetCapital(castle) && castle != null)
                return;
            //原先的城市失去首都标记
            if (IsHaveCapital())
                Capital.Callback_OnBeCapital?.Invoke(false);

            //取消首都
            if (castle == null)
            {
                capital = null;
            }
            //设置新的首都
            else if (IsOwn(castle))
            {
                capital = castle;
                Capital.Callback_OnBeCapital?.Invoke(true);
                if (BaseGlobal.IsUnReadData || !isForce)
                {
                    OnPostSetCapital();
                }
            }
            else
            {
                CLog.Error("{0},并不拥有此城市:{1}", SelfBaseUnit.GetName(), castle.GetName());
            }
        }
        #endregion

        #region callback
        //设置首都之后的操作
        protected virtual void OnPostSetCapital()
        { 
        
        }
        #endregion

        #region DB
        public void LoadDBCapital(ref long capital)
        {
            if (capital.IsInv())
                return;
            var unit = GMgr.GetUnit(capital);
            if (IsOwn(unit))
                SetCapital(unit, true);
        }
        public void SaveDBCapital(ref long capital)
        {
            if (IsHaveCapital())
                capital = Capital.ID;
            else
                capital = SysConst.LONG_Inv;
        }
        public void LoadDBCapital(ref string capital)
        {
            if (capital.IsInv())
                return;
            var unit = GMgr.GetUnit(capital);
            if (IsOwn(unit))
                SetCapital(unit, true);
        }
        public void SaveDBCapital(ref string capital)
        {
            if (IsHaveCapital())
                capital = Capital.TDID;
            else
                capital = SysConst.STR_Inv;
        }
        #endregion
    }
}