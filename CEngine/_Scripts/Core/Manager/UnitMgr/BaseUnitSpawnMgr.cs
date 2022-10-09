//**********************************************
// Class Name	: BaseSpawnMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
namespace CYM
{
    public struct UnitSpawnParam
    {
        public static UnitSpawnParam Default { get; private set; } = new UnitSpawnParam ();
        public Vector3? spwanPoint;
        public Quaternion? quaternion;
        public int? team;
        public string tdid;
        public long? rtid;
        public string prefab;
        public bool isNoConfig;
    };
    /// <summary>
    /// 此类可以给单位使用也可以给全局对象使用
    /// </summary>
    /// <typeparam name="TUnit"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public class BaseUnitSpawnMgr<TUnit,TConfig> : BaseMgr, IUnitSpawnMgr<TUnit>
        where TUnit : BaseUnit
        where TConfig :  TDBaseData, new()
    {
        #region prop
        GameObject TempSpawnTrans = new GameObject("TempSpawnTrans");
        protected BasePoolMgr PoolMgr => BaseGlobal.PoolMgr;
        //重复数据检测
        public HashSet<TUnit> Sets { get; private set; } = new HashSet<TUnit>();
        public IList IListData => Data;
        #endregion

        #region type
        public Type UnitType { get; private set; }
        public Type ConfigType { get; private set; }
        #endregion

        #region mgr
        protected ITDConfig ITDConfig { get; private set; }
        #endregion

        #region ISpawnMgr
        public TUnit Gold { get; protected set; }
        public IDDicList<TUnit> Data { get; protected set; } = new IDDicList<TUnit>();
        public event Callback<TUnit> Callback_OnAdd;
        public event Callback<TUnit> Callback_OnSpawnGold;
        public event Callback<TUnit> Callback_OnSpawn;
        public event Callback<TUnit> Callback_OnDespawn;
        public event Callback<TUnit> Callback_OnDataChanged;
        public Callback<TUnit> Callback_OnUnitMgrAdded { get; set; }
        public Callback<TUnit> Callback_OnUnitMgrRemoved { get; set; }
        public Callback<TUnit> Callback_OnUnitMgrOccupied { get; set; }
        #endregion

        #region life
        public virtual bool IsAddToGlobalSpawnerMgr => true;
        //不读取配置，适用于动态创建的军团，读取配置则适用于配置的城市，国家等
        protected virtual bool IsNoConfig => false;
        protected virtual HideFlags HideFlags => HideFlags.None;
        public override MgrType MgrType => MgrType.All;
        protected virtual bool IsCopyConfig => true;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedTurnbase = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            TempSpawnTrans.hideFlags = HideFlags.HideInHierarchy;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            UnitType = typeof(TUnit);
            ConfigType = typeof(TConfig);
            ITDConfig = BaseLuaMgr.GetTDConfig(ConfigType);
            if (IsGlobal)
            {
                BaseGlobal.BattleMgr.Callback_OnUnLoaded += OnBattleUnLoaded;
                BaseGlobal.BattleMgr.Callback_OnLoadedScene += OnBattleLoadedScene;
                BaseGlobal.LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
            }
        }
        public override void OnTurnbase(bool day, bool month, bool year)
        {
            foreach (var item in Data)
            {
                if (item.IsSystem)
                    continue;
                item.OnTurnbase(day,month,year);
            }
            base.OnTurnbase(day,month,year);
        }
        public override void OnTurnframe(int gameFramesPerSecond)
        {
            base.OnTurnframe(gameFramesPerSecond);
            foreach (var item in Data)
            {
                item.OnTurnframe(gameFramesPerSecond);
            }
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            foreach (var item in Data)
                item.OnGameStart1();
        }
        public override void OnGameStart2()
        {
            base.OnGameStart2();
            foreach (var item in Data)
                item.OnGameStart2();
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            foreach (var item in Data)
                item.OnGameStarted1();
        }
        public override void OnGameStarted2()
        {
            base.OnGameStarted1();
            foreach (var item in Data)
                item.OnGameStarted2();
        }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
            foreach (var item in Data)
                item.OnGameStartOver();
        }
        public override void OnDeath()
        {
            base.OnDeath();
            Clear();
        }
        #endregion 

        #region set
        //和Spawn一样,只不过会触发OnBeNewSpawned回调,通常用于游戏里的新招募的单位，新招募的单位有时候需要处理一些事情，通常为手动调用
        public virtual TUnit SpawnNew(string tdidOrPrefab, UnitSpawnParam param)
        {
            var ret = Spawn( tdidOrPrefab, param);
            ret?.OnBeNewSpawned();
            return ret;
        }
        //通常的Spawn
        public virtual TUnit Spawn(string tdidOrPrefab, [DefaultValue("UnitSpawnParam.Default")]UnitSpawnParam param)
        {
            if (tdidOrPrefab.IsInv()) 
                return null;
            param.tdid = tdidOrPrefab;

            //获得prefab
            GameObject goPrefab;
            if (IsNoConfig || param.isNoConfig)
            {
                goPrefab = BundleCacher.Get(param.tdid);
            }
            else
            {
                //获得配置数据
                TConfig config = ITDConfig?.Get<TConfig>(param.tdid);
                if (config == null)
                {
                    CLog.Error("配置错误，没有这个配置TDID：" + param.tdid);
                    return null;
                }
                if (!param.prefab.IsInv())
                {
                    goPrefab = BundleCacher.Get(param.prefab);
                    if (goPrefab == null)
                    {
                        if (param.prefab != null)
                        {
                            CLog.Error("没有这个CustomPrefab:{0}", param.prefab);
                        }
                        return null;
                    }
                }
                else
                {
                    goPrefab = config?.GetPrefab();
                    if (goPrefab == null)
                    {
                        CLog.Error("没有这个:{0},Prefab:{1}",config.TDID, config.Prefab);
                        return null;
                    }
                }
            }

            GameObject charaGO = PoolItem.Spawn(goPrefab, param.spwanPoint, param.quaternion);
            charaGO.name = tdidOrPrefab;
            charaGO.hideFlags = HideFlags;
            TUnit unitChara = BaseCoreMono.GetUnityComponet<TUnit>(charaGO);
            SpawnAdd(unitChara, param);
            Callback_OnSpawn?.Invoke(unitChara);
            return unitChara;
        }
        public virtual TUnit OnSpawSystem()
        {
            Gold = GetUnit(GoldTDID);
            if (Gold == null)
            {
                TempSpawnTrans.transform.position = GoldPos == null ? SysConst.VEC_FarawayPos : GoldPos.Value;
                Gold = Spawn(GoldTDID,new UnitSpawnParam { spwanPoint = TempSpawnTrans.transform.position,quaternion = Quaternion.identity,team = int.MaxValue,isNoConfig=true });
                Gold.IsSystem = true;
            }
            Gold.BaseConfig.Name = "黄金国";
            Callback_OnSpawnGold?.Invoke(Gold);
            return Gold;
        }
        // 执行Add操作,但是也会触发Spawn流程,适用于对已经存在于地图上的对象使用
        public virtual void SpawnAdd(TUnit chara, [DefaultValue("UnitSpawnParam.Default")] UnitSpawnParam param)
        {
            if (chara == null) return;
            if (!param.rtid.HasValue)chara.SetRTID(IDUtil.Gen());
            else chara.SetRTID(param.rtid.Value);

            TConfig config = ITDConfig?.Get<TConfig>(param.tdid);
            if (config == null)
            {
                config = new TConfig();
            }
            else if (IsCopyConfig)
            {
                config = config.Copy<TConfig>();
            }

            chara.SpawnMgr = this;
            chara.SetTDID(param.tdid);
            chara.SetTeam(param.team);
            chara.SetConfig(config);
            chara.OnInit();
            chara.OnBeSpawned();
            OnSpawned(chara, param);
            AddToData(chara);
        }
        public virtual void Despawn(BaseUnit chara,float delay=0)
        {
            if (chara == null)
            {
                CLog.Error("BaseSpawnMgr.Despawn:chara==null");
                return;
            }
            if (!IsHave(chara))
                return;
            PoolItem.Despawn(chara, delay);
            RemoveFromData(chara as TUnit);
        }
        // 清空数据
        public virtual void Clear()
        {
            Data.Clear();
            Sets.Clear();
        }
        #endregion

        #region is
        public bool IsHave(BaseUnit unit)=> Data.Contains(unit as TUnit);
        #endregion

        #region get
        public TUnit GetUnit(string tdid) => Data.Get(tdid);
        public TUnit GetUnit(long rtid) => Data.Get(rtid);
        #endregion

        #region op data
        protected virtual void AddToData(TUnit chara)
        {
            Data.Add(chara);
            OnDataChanged(chara);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnAdd?.Invoke(chara);
                Callback_OnDataChanged?.Invoke(chara);
            }
        }
        protected virtual void RemoveFromData(TUnit chara)
        {
            Data.Remove(chara);
            OnDataChanged(chara);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnDespawn?.Invoke(chara);
                Callback_OnDataChanged?.Invoke(chara);
            }
        }
        #endregion

        #region virtual
        protected virtual string GoldTDID => throw new NotImplementedException();
        protected virtual Vector3? GoldPos => null;
        protected virtual PoolItem PoolItem => BaseGlobal.PoolUnit;
        protected virtual RsCacher<GameObject> BundleCacher => BaseGlobal.RsPrefab;
        public virtual void OnSpawned(TUnit unit,UnitSpawnParam param) { }
        #endregion

        #region Callback
        public void OnDataChanged(TUnit data) { }
        protected virtual void OnBattleUnLoaded()
        {
            Clear();
        }
        protected virtual void OnBattleLoadedScene()
        {
        }
        protected virtual void OnAllLoadEnd2()
        {
        }
        #endregion

        #region DB
        public override void OnRead1(DBBaseGame data)
        {
            OnSpawSystem();
            base.OnRead1(data);
            foreach (var item in Data)
            {
                if (item.IsSystem || item.IsWild)
                    continue;
                item.OnRead1(data);
            }
        }

        public override void OnRead2(DBBaseGame data)
        {
            base.OnRead2(data);
            foreach (var item in Data)
            {
                if (item.IsSystem || item.IsWild)
                    continue;
                item.OnRead2(data);
            }
        }

        public override void OnRead3(DBBaseGame data)
        {
            base.OnRead3(data);
            foreach (var item in Data)
            {
                if (item.IsSystem || item.IsWild)
                    continue;
                item.OnRead3(data);
            }
        }
        public override void OnReadEnd(DBBaseGame data)
        {
            base.OnReadEnd(data);
            foreach (var item in Data)
            {
                if (item.IsSystem || item.IsWild)
                    continue;
                item.OnReadEnd(data);
            }
        }

        public override void OnWrite(DBBaseGame data)
        {
            base.OnWrite(data);
            foreach (var item in Data)
            {
                if (item.IsSystem || item.IsWild)
                    continue;
                item.OnWrite(data);
            }
        }
        #endregion
    }
}