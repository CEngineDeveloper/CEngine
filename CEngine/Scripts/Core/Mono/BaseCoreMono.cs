using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    /// <summary>
    /// Mono生命周期:
    /// Awake
    /// AttachComponet
    /// AffterAwake
    /// OnEnable
    /// Init
    /// Birth,Rebirth
    /// OnSpawned(GlobalMgr)
    /// OnNewSpawn(GlobalMgr)
    /// OnSpawned(EntityMgr)
    /// Start
    /// AffterStart
    /// OnUpdate
    /// OnFixedUpdate
    /// OnLateUpdate
    /// Death
    /// RealDeath
    /// OnDisable
    /// OnDestroy
    /// </summary>
    /// 此类不能和BaseUnit类合并,因为UI和BaseGlobal对象继承此类
    /// 作为 BaseUnit 和 BaseView 的基类
    [HideMonoScript]
    public class BaseCoreMono : BaseMono, IBase, IMono, IDBMono, IUnit, IOnAnimTrigger
    {
        #region callback Val
        /// OnInit:会在Disable的时候自动注销
        public event Callback Callback_OnInit;
        /// OnBirth:会在Disable的时候自动注销
        public event Callback Callback_OnBirth;
        /// OnRebirth:会在Disable的时候自动注销
        public event Callback Callback_OnRebirth;
        /// OnDeath:会在Disable的时候自动注销
        public event Callback Callback_OnDeath;
        /// OnRealDeath:会在Disable的时候自动注销
        public event Callback Callback_OnRealDeath;
        #endregion

        #region member variable
        List<IOnAnimTrigger> triggersComponets = new List<IOnAnimTrigger>();
        List<BaseMgr> componets = new List<BaseMgr>();
        List<BaseMgr> updateComponets = new List<BaseMgr>();
        List<BaseMgr> fixedUpdateComponets = new List<BaseMgr>();
        List<BaseMgr> jobUpdateComponets = new List<BaseMgr>();
        List<BaseMgr> lateUpdateComponets = new List<BaseMgr>();
        List<BaseMgr> guiComponets = new List<BaseMgr>();
        List<BaseMgr> turnbaseComponets = new List<BaseMgr>();
        public bool NeedJobUpdate { get; protected set; }
        public bool NeedUpdate { get; protected set; }
        public bool NeedGUI { get; protected set; }
        public bool NeedFixedUpdate { get; protected set; }
        public bool NeedLateUpdate { get; protected set; }
        public virtual bool IsEnable { get; set; }
        public virtual MonoType MonoType => MonoType.Normal;
        #endregion

        #region prop
        public long ID { get; set; } = 0;
        public string TDID { get; set; } = SysConst.STR_Inv;
        public int INID { get; set; } = SysConst.INT_Inv;
        public virtual LayerData LayerData => null;
        public bool IsInited { get; private set; } = false;
        #endregion

        #region unit life   
        public virtual void OnBeSpawned()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnBeSpawned();
            }
        }
        public virtual void OnBeNewSpawned()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnBeNewSpawned();
            }
        }
        public virtual void OnInit()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnInit();
            }
            Callback_OnInit?.Invoke();
            OnBirth();
            OnBirth2();
            OnBirth3();
            IsInited = true;
        }

        public virtual void OnBirth()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnBirth();
            }
            Callback_OnBirth?.Invoke();
        }

        public virtual void OnBirth2()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnBirth2();
            }
        }

        public virtual void OnBirth3()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnBirth3();
            }
        }

        public virtual void OnReBirth()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnReBirth();
            }
            OnBirth();
            Callback_OnRebirth?.Invoke();
        }
        public virtual void OnDissolve() 
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnDissolve();
            }
        }
        public virtual void OnDeath()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnDeath();
            }
            Callback_OnDeath?.Invoke();
            IsInited = false;
        }

        public virtual void OnRealDeath()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnRealDeath();
            }
            Callback_OnRealDeath?.Invoke();
        }
        public virtual void OnGameStart1()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnGameStart1();
            }
        }
        public virtual void OnGameStart2()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnGameStart2();
            }
        }
        public virtual void OnGameStarted1()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnGameStarted1();
            }
        }
        public virtual void OnGameStarted2()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnGameStarted2();
            }
        }
        public virtual void OnGameStartOver()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnGameStartOver();
            }
        }
        public virtual void OnCloseLoadingView()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.OnCloseLoadingView();
            }
        }
        #endregion

        #region methon
        public override void OnEnable()
        {
            //如果LayerData和GameObject的Layer不相等,才会被设置
            if (LayerData!=null && (int)LayerData != GO.layer)
                SetLayer(LayerData, false);
            foreach (var value in componets)
            {
                value.OnEnable();
            }
            GlobalMonoManager.ActiveMono(this);
        }
        public virtual void OnSetNeedFlag()
        {
        }
        public override void Awake()
        {
            //如果LayerData没有被重载,则不会被设置
            if (LayerData!=null && LayerData != SysConst.Layer_Default)
                SetLayer(LayerData, true);
            OnSetNeedFlag();
            OnAttachComponet();
            OnAffterAwake();
        }

        public override void Start()
        {
            foreach (var value in componets)
            {
                value.OnStart();
            }
            OnAffterStart();
        }
        // 增加组建
        protected virtual void OnAttachComponet() { }
        public virtual void OnAffterAwake()
        {
            foreach (var value in componets)
                value.OnAffterAwake();
        }
        public virtual void OnAffterStart()
        {
            foreach (var value in componets)
            {
                value.OnAffterStart();
            }
        }
        public virtual void OnUpdate()
        {
            foreach (var value in updateComponets)
            {
                if (value.IsEnable)
                    value.OnUpdate();
            }
        }
        public virtual void OnFixedUpdate()
        {
            foreach (var value in fixedUpdateComponets)
            {
                if (value.IsEnable)
                    value.OnFixedUpdate();
            }
        }
        public virtual void OnJobUpdate()
        {
            foreach (var value in jobUpdateComponets)
            {
                if (value.IsEnable)
                    value.OnJobUpdate();
            }
        }
        public virtual void OnLateUpdate()
        {
            foreach (var value in lateUpdateComponets)
            {
                if (value.IsEnable)
                    value.OnLateUpdate();
            }
        }
        public virtual void OnGUIPaint()
        {
            foreach (var value in guiComponets)
            {
                if (value.IsEnable)
                    value.OnGUIPaint();
            }
        }
        public override void OnDisable()
        {
            foreach (var value in componets)
            {
                value.OnDisable();
            }
            GlobalMonoManager.DeactiveMono(this);
            Callback_OnInit = null;
            Callback_OnBirth = null;
            Callback_OnRebirth = null;
            Callback_OnDeath = null;
            Callback_OnRealDeath = null;
            if (Rigidbody != null)
                Rigidbody.Sleep();
        }
        public override void OnDestroy()
        {
            foreach (var value in componets)
            {
                value.OnDestroy();
            }
        }

        public virtual T AddComponent<T>() where T : BaseMgr, new()
        {
            var component = BaseMgr.Create<T>();
            OnPostCreateComponent(component);
            return component;
        }
        public virtual BaseMgr AddComponent(Type type)
        {
            var component = BaseMgr.Create(type);
            OnPostCreateComponent(component);
            return component;
        }
        void OnPostCreateComponent(BaseMgr component)
        {
            if (!Util.IsFit(this, component))
            {
                CLog.Error("AddComponent=>{0},{1}:{2}", GOName, component.ToString(), "组件类型不一致");
            }
            componets.Add(component);
            if (component is IOnAnimTrigger)
                triggersComponets.Add(component as IOnAnimTrigger);
            if (component.NeedUpdate)
                updateComponets.Add(component);
            if (component.NeedLateUpdate)
                lateUpdateComponets.Add(component);
            if (component.NeedFixedUpdate)
                fixedUpdateComponets.Add(component);
            if (component.NeedJobUpdate)
                jobUpdateComponets.Add(component);
            if (component.NeedGUI)
                guiComponets.Add(component);
            if (component.NeedTurnbase)
                turnbaseComponets.Add(component);
            component.OnBeAdded(this);
        }

        public virtual void RemoveComponent(BaseMgr component)
        {
            if (component != null)
            {
                component.OnBeRemoved();
                if (component is IOnAnimTrigger)
                    triggersComponets.Remove(component as IOnAnimTrigger);
                if (component.NeedUpdate)
                    updateComponets.Remove(component);
                if (component.NeedLateUpdate)
                    lateUpdateComponets.Remove(component);
                if (component.NeedFixedUpdate)
                    fixedUpdateComponets.Remove(component);
                if (component.NeedJobUpdate)
                    jobUpdateComponets.Remove(component);
                if (component.NeedGUI)
                    guiComponets.Remove(component);
                if (component.NeedTurnbase)
                    turnbaseComponets.Remove(component);
                componets.Remove(component);
            }
        }
        //用于清空数据
        public void Clear()
        {
            foreach (var item in componets)
                item.OnClear();
        }
        #endregion

        #region 手动调用
        // 手动调用(简单回合制,更新数值,适用于回合制游戏)
        public virtual void OnTurnbase(bool day, bool month, bool year)
        {
            foreach (var value in turnbaseComponets)
            {
                if (value.IsEnable)
                    value.OnTurnbase(day,month,year);
            }
        }
        // 手动调用(帧同步回合)
        public virtual void OnTurnframe(int gameFramesPerSecond)
        {
            foreach (var value in componets)
            {
                if (value.IsEnable)
                    value.OnTurnframe(gameFramesPerSecond);
            }
        }
        // 手动调用(动画触发)
        public virtual void OnAnimTrigger(int param)
        {
            foreach (var item in triggersComponets)
            {
                item.OnAnimTrigger(param);
            }
        }
        #endregion

        #region DB
        public virtual void OnRead1(DBBaseGame data)
        {
            foreach (var item in componets)
                item.OnRead1(data);
        }
        public virtual void OnRead2(DBBaseGame data)
        {
            foreach (var item in componets)
                item.OnRead2(data);
        }
        public virtual void OnRead3(DBBaseGame data)
        {
            foreach (var item in componets)
                item.OnRead3(data);
        }
        public virtual void OnReadEnd(DBBaseGame data)
        {
            foreach (var item in componets)
                item.OnReadEnd(data);
        }

        public virtual void OnWrite(DBBaseGame data)
        {
            foreach (var item in componets)
                item.OnWrite(data);
        }
        #endregion

        #region login
        public virtual void OnLoginInit1(object data)
        {
            foreach (var item in componets)
                item.OnLoginInit1(data);
        }
        public virtual void OnLoginInit2(object data)
        {
            foreach (var item in componets)
                item.OnLoginInit2(data);
        }
        public virtual void OnLoginInit3(object data)
        {
            foreach (var item in componets)
                item.OnLoginInit3(data);
        }
        #endregion

        #region inspector
        bool firstDrawGizmos = true;
        protected virtual void OnDrawGizmos()
        {
            if (firstDrawGizmos)
            {
                OnFirstDrawGizmos();
                firstDrawGizmos = false;
            }
        }
        protected virtual void OnFirstDrawGizmos()
        {

        }
        #endregion

        #region operate
        public static explicit operator long(BaseCoreMono data)
        {
            return data.ID;
        }
        public static explicit operator string(BaseCoreMono data)
        {
            return data.TDID;
        }
        #endregion

        #region static methon
        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            Transform[] trans = obj.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; ++i)
            {
                trans[i].gameObject.layer = layer;
            }
        }
        //设置tag
        public static void SetTagRecursively(GameObject obj, string name)
        {
            Transform[] trans = obj.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; ++i)
            {
                trans[i].gameObject.tag = name;
            }
        }
        public void SetDirtyAllAction(bool isImmidiate = false)
        {
            foreach (var item in componets)
                item.SetDirtyAllAction(isImmidiate);
        }
        #endregion

        //#region Inspector
        //[Button("CheckMissing")]
        //void CheckMissing()
        //{
        //    Transform[] trans = GetComponentsInChildren<Transform>(true);
        //    foreach (var item in trans)
        //    {

        //        foreach (var com in item.gameObject.GetComponents<Component>())
        //        {

        //            if (com == null)
        //                Debug.LogError("Missing:" + GetPath(item.gameObject), item.gameObject);
        //        }
        //    }
        //    static string GetPath(GameObject go)
        //    {
        //        return go.transform.parent == null ? "/" + go.name : GetPath(go.transform.parent.gameObject) + "/" + go.name;
        //    }
        //}
        //#endregion
    }
}
