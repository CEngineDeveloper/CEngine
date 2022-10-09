//------------------------------------------------------------------------------
// BaseEntitySpawnMgr.cs
// Copyright 2019 2019/1/17 
// Created by CYM on 2019/1/17
// Owner: CYM
// 全局的实体生成管理器,可以和本地实体管理器配合使用(BaseEntityMgr)
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public class BaseEntitySpawnMgr<TUnit, TConfig, TDBData, TOwner> : BaseUnitSpawnMgr<TUnit, TConfig>,IDBListConverMgr<TDBData>
        where TUnit : BaseEntity<TUnit, TConfig, TDBData, TOwner>
        where TConfig : TDBaseData, new()
        where TDBData : DBBaseUnit, new()
        where TOwner : BaseUnit
    {
        #region Callback val
        //失去所有实体,TOwner1:造成方,TOwner2:承受放
        public Callback<TOwner, TOwner> Callback_OnLoseAll { get; set; }
        #endregion

        #region life
        public override MgrType MgrType => MgrType.All;
        public override bool IsGlobal => true;
        public override void OnEnable()
        {
            base.OnEnable();
            Callback_OnLoseAll += OnLoseAll;
        }
        public override void OnDisable()
        {
            Callback_OnLoseAll -= OnLoseAll;
            base.OnDisable();
        }
        #endregion

        #region set
        //不会创建新的,加载地图上以有的对象,类似于SpawnAdd
        public void SpawnAddDB(TUnit unit, TDBData dbData)
        {
            if (unit == null || dbData == null)
                return;
            SpawnAdd(unit,new UnitSpawnParam { tdid = dbData.TDID, rtid = dbData.ID });
            unit.SetDBData(dbData);
        }
        //加载,并且创建
        public void SpawnDB(TDBData dbData)
        {
            if (dbData == null)
                return;
            //Gold单位不加载
            if (dbData.TDID == GoldTDID)
                return;
            var unit = Spawn(dbData.TDID,new UnitSpawnParam { spwanPoint = dbData.Position.V3,quaternion = dbData.Rotation.Q,team =0,rtid = dbData.ID });
            unit.SetDBData(dbData);
        }
        #endregion

        #region Callback
        protected virtual void OnLoseAll(TOwner caster, TOwner underParty) { }
        #endregion

        #region DB
        public void LoadAddDBData(ref List<TDBData> data,Func<string,TUnit> getUnit)
        {
            data.ForEach((x) =>
            {
                TUnit temp = getUnit(x.TDID);
                SpawnAddDB(temp, x);
            });
        }
        public void LoadDBData(ref List<TDBData> data)
        {
            data.ForEach((x) => SpawnDB(x));
        }
        public void SaveDBData(ref List<TDBData> data)
        {
            var temp = data;
            Data.ForEach(x => {
                if(!x.IsSystem)
                    temp.Add(x.DBData); 
            });
        }
        #endregion
    }
}