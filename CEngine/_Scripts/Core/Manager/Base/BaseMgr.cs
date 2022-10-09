//**********************************************
// Discription	：CYMBaseCoreComponent
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    /// <summary>
    /// 组建启动顺序:
    /// OnCreate
    /// OnBeAdded
    /// OnBeAttachedParent(可选)
    /// OnEnable
    /// OnStart
    /// OnUpdate
    /// OnFixUpdate
    /// OnLateUpdate
    /// OnDisable
    /// OnDestroy
    /// </summary>
    public class BaseMgr : IBase, IDBMono, IUnit
    {

        #region member variable
        public long ID { get; set; }
        public string TDID { get; set; }
        public BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        public bool IsSubComponent { get; private set; }
        public bool IsEnable { protected set; get; }
        public bool NeedUpdate { protected set; get; }
        public bool NeedGUI { protected set; get; }
        public bool NeedFixedUpdate { protected set; get; }
        public bool NeedJobUpdate { protected set; get; }
        public bool NeedLateUpdate { protected set; get; }
        public bool NeedTurnbase { protected set; get; }
        public virtual MgrType MgrType => MgrType.All;
        #endregion

        #region property
        public bool IsInited { get; private set; } = false;
        public BaseUnit SelfBaseUnit { get; protected set; }
        protected BaseMgr parentComponet { get; set; }
        protected List<BaseMgr> subComponets = new List<BaseMgr>();
        protected BaseCoreMono SelfMono { get; private set; }
        protected BaseSceneRoot BaseSceneObject => BaseSceneRoot.Ins;
        protected List<DirtyAction> DirtyActions = new List<DirtyAction>();
        private bool isDirty = false;
        #endregion

        #region mgr
        IDBMgr<DBBaseGame> DBMgr => BaseGlobal.DBMgr;
        DBBaseGame CurBaseGameData => DBMgr.CurGameData;
        #endregion

        #region wrapper
        public virtual Vector3 Pos
        {
            get
            { 
                if(SelfBaseUnit!=null)
                    return SelfBaseUnit.Pos;
                return Vector3.zero;
            }
        }
        public virtual Vector3 LocalPos
        {
            get
            { 
                if(SelfBaseUnit!=null)
                    return SelfBaseUnit.LocalPos;
                return Vector3.zero;
            }
        }
        #endregion

        #region methon
        public virtual void Enable(bool b)
        {
            IsEnable = b;
        }

        public T AddSubComponent<T>() where T : BaseMgr, new()
        {
            BaseMgr component = Create<T>();
            if (!Util.IsFit(this, component))
            {
                CLog.Error("AddSubComponent {0},{1}:{2}", ToString(), component.ToString(), "组件类型不一致");
            }
            subComponets.Add(component);
            component.IsSubComponent = true;
            component.parentComponet = this;
            component.NeedTurnbase = true;
            component.OnBeAttachedToParentComponet(this);
            return (T)component;
        }
        // 组建创建的时候
        public virtual void OnCreate()
        {
            DirtyActions.Clear();
            IsEnable = true;
            OnSetNeedFlag();
        }
        protected virtual void OnSetNeedFlag() { }
        // 组建被关联到伏组件的时候
        protected virtual void OnBeAttachedToParentComponet(BaseMgr parentComponet)
        {
            if (parentComponet.SelfMono != null)
                OnBeAdded(parentComponet.SelfMono);
        }
        // 组建被添加到mono的时候
        public virtual void OnBeAdded(IMono mono)
        {
            SelfMono = (BaseCoreMono)mono;
            if (mono is BaseUnit) SelfBaseUnit = (BaseUnit)mono;
            foreach (var item in subComponets)
                item.OnBeAdded(SelfMono);
        }
        public virtual void OnAffterAwake()
        {
            foreach (var item in subComponets)
                item.OnAffterAwake();
        }
        // mono的OnEnable
        public virtual void OnEnable()
        {
            foreach (var item in subComponets)
                item.OnEnable();
        }
        // mono的OnStart
        public virtual void OnStart()
        {
            foreach (var item in subComponets)
                item.OnStart();
        }
        public virtual void OnAffterStart()
        {
            foreach (var item in subComponets)
                item.OnAffterStart();
        }
        // mono的渲染帧
        public virtual void OnUpdate()
        {
            foreach (var item in subComponets)
                if (item.IsEnable)
                    item.OnUpdate();
        }
        // mono的逻辑帧
        public virtual void OnFixedUpdate()
        {
            foreach (var item in subComponets)
                if (item.IsEnable)
                    item.OnFixedUpdate();

            foreach (var item in DirtyActions)
                item.OnUpdate();
            if (isDirty)
            {
                Refresh();
            }
        }
        public virtual void OnJobUpdate()
        {
            foreach (var value in subComponets)
            {
                if (value.IsEnable)
                    value.OnJobUpdate();
            }
        }
        // mono的回合帧
        public virtual void OnLateUpdate()
        {
            foreach (var item in subComponets)
                if (item.IsEnable)
                    item.OnLateUpdate();
        }
        // mono的日期帧
        public virtual void OnTurnbase(bool day,bool month,bool year)
        {
            foreach (var item in subComponets)
            {
                if (item.NeedTurnbase && item.IsEnable)
                    item.OnTurnbase(day,month,year);
            }
        }
        // 帧同步
        public virtual void OnTurnframe(int gameFramesPerSecond)
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnTurnframe(gameFramesPerSecond);
            }
        }
        // mono的OnGui
        public virtual void OnGUIPaint() { }
        // mono的OnDisable
        public virtual void OnDisable()
        {
            foreach (var item in subComponets)
                item.OnDisable();
        }
        // mono的OnDestroy
        public virtual void OnDestroy()
        {
            foreach (var item in subComponets)
                item.OnDestroy();
        }
        // 组建被移除的时候
        public virtual void OnBeRemoved()
        {
            foreach (var item in subComponets)
                item.OnBeRemoved();
        }
        public virtual void OnClear()
        {
            foreach (var item in subComponets)
                item.OnClear();
        }
        // 创建组建的时候
        public static T Create<T>() where T : BaseMgr, new()
        {
            T t = new T();
            t.OnCreate();
            return t;
        }
        public static BaseMgr Create(Type type)
        {
            BaseMgr t = type.Assembly.CreateInstance(type.FullName) as BaseMgr;
            t.OnCreate();
            return t;
        }
        // 刷新
        public virtual void Refresh()
        {
            isDirty = false;
            foreach (var item in subComponets)
                item.Refresh();
        }
        // dirty
        public void SetDirty()
        {
            if (!NeedFixedUpdate)
            {
                CLog.Error("有个组件没有设置NeedFixedUpdate 导致 SetDirty无法生效:" + GetType().Name);
            }
            isDirty = true;
        }
        // 创建一个DirtyAction,会在GameLogicTurn和GameStart的时候自动调用
        public DirtyAction CreateDirtyAction(Callback action)
        {
            var temp = new DirtyAction(action);
            DirtyActions.Add(temp);
            if (!NeedFixedUpdate)
            {
                CLog.Error("有个组件没有设置NeedFixedUpdate 导致 CreateDirtyAction无法生效:" + GetType().Name);
            }
            return temp;
        }
        #endregion

        #region unit life
        public virtual void OnBeSpawned()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnBeSpawned();
            }
        }
        public virtual void OnBeNewSpawned()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnBeNewSpawned();
            }
        }
        public virtual void OnInit()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnInit();
            }
            IsInited = true;
        }

        public virtual void OnBirth()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnBirth();
            }
        }

        public virtual void OnBirth2()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnBirth2();
            }
        }

        public virtual void OnBirth3()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnBirth3();
            }
            SetDirtyAllAction();
        }


        public virtual void OnReBirth()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnReBirth();
            }
        }
        public virtual void OnDissolve()       
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnDissolve();
            }
        }
        public virtual void OnDeath()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnDeath();
            }
            IsInited = false;
        }

        public virtual void OnRealDeath()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnRealDeath();
            }
        }
        public virtual void OnGameStart1()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnGameStart1();
            }
        }
        public virtual void OnGameStart2()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnGameStart2();
            }
        }
        public virtual void OnGameStarted1()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnGameStarted1();
            }
            SetDirtyAllAction();
        }
        public virtual void OnGameStarted2()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnGameStarted2();
            }
        }
        public virtual void OnGameStartOver()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnGameStartOver();
            }
        }
        public virtual void OnCloseLoadingView()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.OnCloseLoadingView();
            }
        }

        #endregion

        #region get
        protected string GetStr(string key,params object[] ps) => BaseLangMgr.Get(key, ps);
        protected string JointStr(string key, params object[] ps) => BaseLangMgr.Joint(key, ps);
        protected T Cast<T>(BaseMono obj) where T : BaseMono
        {
            return obj as T;
        }
        protected T SelfGlobal<T>() where T : BaseGlobal
        {
            return SelfBaseGlobal as T;
        }
        protected TType GetAddedObjData<TType>(object[] ps, int index, TType defaultVal = default(TType))
        {
            if (ps == null || ps.Length <= index)
                return defaultVal;
            return (TType)(ps[index]);
        }
        public TUnit GetEntity<TUnit>(long id, bool isLogError = true) where TUnit : BaseUnit => BaseGlobal.GetUnit<TUnit>(id, isLogError);
        public TUnit GetEntity<TUnit>(string id, bool isLogError = true) where TUnit : BaseUnit => BaseGlobal.GetUnit<TUnit>(id, isLogError);
        public BaseUnit GetEntity(long id, Type unitType = null, bool isLogError = true) => BaseGlobal.GetUnit(id, unitType, isLogError);
        public BaseUnit GetEntity(string id, Type unitType = null, bool isLogError = true) => BaseGlobal.GetUnit(id, unitType, isLogError);
        public HashList<BaseUnit> GetEntity(List<long> ids) => BaseGlobal.GetUnit(ids);
        public List<long> GetEntityIDs(HashList<BaseUnit> entity) => BaseGlobal.GetUnitIDs(entity);
        #endregion

        #region set
        public void SetDirtyAllAction(bool isImmidiate = false)
        {
            foreach (var item in DirtyActions)
                item.SetDirty(isImmidiate);
        }
        #endregion

        #region is
        public virtual bool IsGlobal => SelfBaseGlobal == SelfMono;
        //新游戏或者挂载对象是新添加的
        public bool IsNew
        {
            get
            {
                bool custom = false;
                if (SelfBaseUnit!=null && SelfBaseUnit.DBBaseData != null)
                    custom = SelfBaseUnit.DBBaseData.IsNewAdd;
                return IsNewGame || custom;
            }
        }
        public bool IsNewGame=> CurBaseGameData.IsNewGame();
        public bool IsLoadGame=> CurBaseGameData.IsLoadGame();
        #endregion

        #region DB
        public virtual void OnRead1(DBBaseGame data)
        {
            foreach (var item in subComponets)
                item.OnRead1(data);
        }
        public virtual void OnRead2(DBBaseGame data)
        {
            foreach (var item in subComponets)
                item.OnRead2(data);
        }
        public virtual void OnRead3(DBBaseGame data)
        {
            foreach (var item in subComponets)
                item.OnRead3(data);
        }
        public virtual void OnReadEnd(DBBaseGame data)
        {
            foreach (var item in subComponets)
                item.OnReadEnd(data);
        }
        public virtual void OnWrite(DBBaseGame data)
        {
            foreach (var item in subComponets)
                item.OnWrite(data);
        }
        #endregion

        #region login
        public virtual void OnLoginInit1(object data)
        {
            foreach (var item in subComponets)
                item.OnLoginInit1(data);
        }
        public virtual void OnLoginInit2(object data)
        {
            foreach (var item in subComponets)
                item.OnLoginInit2(data);
        }
        public virtual void OnLoginInit3(object data)
        {
            foreach (var item in subComponets)
                item.OnLoginInit3(data);
        }
        #endregion

        #region other
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion
    }
}
