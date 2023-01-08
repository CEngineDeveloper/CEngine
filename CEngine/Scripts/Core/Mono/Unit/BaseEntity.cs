//------------------------------------------------------------------------------
// Unit.cs
// Copyright 2018 2018/11/3 
// Created by CYM on 2018/11/3
// Owner: CYM
// 游戏的基础对象
// 实体对象,用于模拟具体得实物对象,比如国家,城市,军团,完整角色
//------------------------------------------------------------------------------

using System;

namespace CYM
{
    public class BaseEntity<TUnit, TConfig, TDBData, TOwner> : BaseUnit
        where TUnit : BaseEntity<TUnit, TConfig, TDBData, TOwner>
        where TConfig : TDBaseData, new()
        where TDBData : DBBaseUnit, new()
        where TOwner : BaseUnit
    {
        #region Callback
        /// <summary>
        /// 设置玩家的时候
        /// T1:oldPlayer
        /// T2:newPlayer
        /// </summary>
        public new event Callback<TOwner, TOwner> Callback_OnSetOwner;
        /// <summary>
        /// 被占领的时候
        /// T1:oldPlayer
        /// T2:newPlayer
        /// </summary>
        public Callback<TOwner, TOwner> Callback_OnBeOccupied { get; set; }
        public Callback<bool> Callback_OnBeCapital { get; set; }
        #endregion

        #region prop
        //OnSpawned 赋值
        public TConfig Config => BaseConfig as TConfig;
        public TDBData DBData => DBBaseData as TDBData;
        public TOwner PreOwner => BasePreOwner as TOwner;
        //AddToData 赋值
        public TOwner Owner => BaseOwner as TOwner;
        protected ITDConfig ITDConfig { get; private set; }
        //是否为MainlyUnit,如果此单位类型和Owner类型一样就视为MainlyUnit
        public bool IsMainlyUnit { get; private set; } = false;
        #endregion

        #region type
        public Type OwnerType { get; private set; }
        public Type UnitType { get; private set; }
        public Type ConfigType { get; private set; }
        #endregion

        #region life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            OwnerType = typeof(TOwner);
            UnitType = typeof(TUnit);
            ConfigType = typeof(TConfig);
            ITDConfig = BaseLuaMgr.GetTDConfig(ConfigType);
            if (UnitType == OwnerType)
            {
                BaseOwner = this;
                IsMainlyUnit = true;
            }
        }
        public override void OnDeath()
        {
            base.OnDeath();
        }
        #endregion

        #region set
        // 设置父对象
        public void SetOwner(TOwner unit)
        {
            base.SetOwner(unit);
            if (unit == null)
            {
                CLog.Error("错误SetOwner:Unit为null,{0}", GetName());
                return;
            }
            //只有非读取数据阶段才能触发回调
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnSetOwner?.Invoke(PreOwner, Owner);
            }
        }
        #endregion

        #region is
        public bool IsHaveOwner() => Owner != null;
        public override bool IsPlayer() => BaseGlobal.ScreenMgr.Player == Owner;
        public bool IsCapital
        {
            get
            {
                if (Owner == null)
                    return false;
                var targetMgr = Owner.GetUnitMgr(UnitType) as BaseEntityMgr<TUnit, TConfig, TDBData, TOwner>;
                return targetMgr.Capital == this;
            }
        }
        public override bool IsSelf(BaseUnit other)
        {
            return BaseOwner == other.BaseOwner;
        }
        public override bool IsFriend(BaseUnit other)
        {
            if (IsMainlyUnit)
            {
                return false;
            }
            else
            {
                return Owner.IsFriend(other.BaseOwner);
            }
        }
        public override bool IsEnemy(BaseUnit other)
        {
            if (IsMainlyUnit)
            {
                return false;
            }
            else
            {
                return Owner.IsEnemy(other.BaseOwner);
            }
        }
        public override bool IsWild
        {
            get
            {
                if (BaseOwner == null || BaseOwner.BaseConfig == null)
                    return true;
                return BaseOwner.BaseConfig.IsWild;
            }
        }
        public override bool IsPlayerCtrl()
        {
            if (Owner == this)
                return base.IsPlayerCtrl();
            return BaseOwner.IsPlayerCtrl();
        }
        #endregion

        #region DB
        public override void OnRead1(DBBaseGame data)
        {
            if (DBData == null)
                DBBaseData = new TDBData();
            if (Config == null)
                BaseConfig = new TConfig();
            Config.CustomName = DBData.CustomName;
            base.OnRead1(data);
        }
        public override void OnWrite(DBBaseGame data)
        {
            DBBaseData = new TDBData();
            DBData.ID = ID;
            DBData.TDID = TDID;
            DBData.Position.Fill(Pos);
            DBData.Rotation.Fill(Rot);
            DBData.IsNewAdd = false;
            if (Config != null)
            {
                DBData.CustomName = Config.CustomName;
            }
            base.OnWrite(data);
        }
        #endregion

        #region operate
        public static explicit operator BaseEntity<TUnit, TConfig, TDBData, TOwner>(long data)
        {
            return BaseGlobal.GetUnit<TUnit>(data);
        }
        public static explicit operator BaseEntity<TUnit, TConfig, TDBData, TOwner>(string data)
        {
            return BaseGlobal.GetUnit<TUnit>(data);
        }
        #endregion
    }
}