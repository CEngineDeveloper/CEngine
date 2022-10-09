//**********************************************
// Class Name	: CYMStateMathine
// Discription	：State Mathine. Useful calss for AI
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public interface IStateMachine
    {
        BaseUnit SelfBaseUnit { get; }
        void Init(BaseUnit owner);
        void OnUpdate();
        void OnFixedUpdate();
        void RevertToPreState();
    }
    public class StateMachine<TStateData> : IStateMachine 
        where TStateData : State
    {
        #region prop
        public TStateData CurStateData { get; private set; }
        public TStateData PreStateData { get; private set; }
        public TStateData GlobalStateData { get; private set; }
        public BaseUnit SelfBaseUnit { get; private set; }
        #endregion

        public StateMachine()
        {
            SelfBaseUnit = null;
            CurStateData = null;
            PreStateData = null;
            GlobalStateData = null;
        }
        public virtual void Init(BaseUnit self)
        {
            this.SelfBaseUnit = self;
        }
        public bool IsInState(TStateData state)
        {
            return CurStateData == state;
        }
        public virtual void OnUpdate()
        {
            if (GlobalStateData != null) GlobalStateData.Update();
            if (CurStateData != null) CurStateData.Update();
        }
        public virtual void OnFixedUpdate()
        {
            if (GlobalStateData != null) GlobalStateData.OnFixedUpdate();
            if (CurStateData != null) CurStateData.OnFixedUpdate();
        }
        public void SetPreState(TStateData state) { PreStateData = state; }
        public void SetGlobalState(TStateData state) { GlobalStateData = state; }
        public virtual void SetCurState(TStateData state, bool isManual = true)
        {
            if (state == null) return;
            state.BaseStateMachine = this;
            CurStateData = state;
            CurStateData.IsManual = isManual;
            CurStateData.Enter();
        }
        public virtual void ChangeState(TStateData state, bool isForce = true, bool isManual = true)
        {
            if (state == null) return;
            state.BaseStateMachine = this;
            PreStateData = CurStateData;
            if (CurStateData != null) CurStateData.Exit();
            CurStateData = state;
            CurStateData.IsForce = isForce;
            CurStateData.IsManual = isManual;
            CurStateData.Enter();
        }
        public void RevertToPreState()
        {
            ChangeState(PreStateData);
        }
    }

    public class CharaStateMachine<TState, TUnit, TStateData> : StateMachine<TStateData> 
        where TState : Enum 
        where TUnit : BaseUnit 
        where TStateData : CharaState<TState, TUnit>, new()
    {
        #region Callback
        public event Callback<TState, TState> Callback_OnChangeState;
        #endregion

        #region prop
        public Dictionary<int, TStateData> States = new Dictionary<int, TStateData>();
        public TUnit SelfUnit { get; protected set; }
        #endregion

        #region Val
        public TState CurState { get; private set; }
        public TState PreState { get; private set; }
        #endregion

        #region life
        public override void Init(BaseUnit selfUnit)
        {
            if (selfUnit == null)
            {
                CLog.Error("CharaStateMachine.Init:selfUnit 不能为空");
                return;
            }
            ClearState();
            base.Init(selfUnit);
            SelfUnit = selfUnit as TUnit;
            EnumTool<TState>.For((x) =>
            {
                AddState(x, new TStateData());
            });
        }
        #endregion

        #region set
        /// <summary>
        ///直接改变一个状态
        ///target:目标
        ///isForce:是否为强制性
        ///isManual:是否为手动改变(非系统性操作)
        /// </summary>
        /// <param name="state"></param>
        public virtual TStateData ChangeState(TState state, bool isForce = false, bool isManual = true)
        {
            int intCurState = EnumTool<TState>.Int(CurState);
            int intNextState = EnumTool<TState>.Int(state);
            if (!isForce && intCurState == intNextState) return null;
            PreState = CurState;
            CurState = state;
            var newStateData = States[intNextState];
            newStateData.SelfUnit = SelfUnit;
            base.ChangeState(newStateData, isForce, isManual);
            Callback_OnChangeState?.Invoke(CurState, PreState);
            return newStateData;
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        public void SetCurState(TState state, bool isManual = true)
        {
            int intstate = EnumTool<TState>.Int(state);
            if (!States.ContainsKey(intstate)) return;
            var newStateData = States[intstate];
            newStateData.SelfUnit = SelfUnit;
            SetCurState(newStateData, isManual);
            CurState = state;
        }
        /// <summary>
        /// 添加一个状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="state"></param>
        public void AddState(TState type, TStateData state)
        {
            int key = EnumTool<TState>.Int(type);
            state.SelfUnit = SelfUnit;
            state.State = CurState;
            state.OnBeAdded();
            if (States.ContainsKey(key))
            {
                States[key] = state;
            }
            else
            {
                States.Add(key, state);
            }

        }
        /// <summary>
        /// 清空状态
        /// </summary>
        public void ClearState()
        {
            States.Clear();
        }
        #endregion

        #region get
        public TStateData GetState(TState state)
        {
            int key = EnumTool<TState>.Int(state);
            if (States.ContainsKey(key))
                return States[key];
            return null;
        }
        #endregion

        #region is
        // 是否在指定状态
        public bool IsIn(TState state)
        {
            int intState = EnumTool<TState>.Int(CurState);
            int intstate = EnumTool<TState>.Int(state);
            return intState == intstate;
        }
        // 判断上一个状态
        public bool IsInPreState(TState state, int index)
        {
            int intState = EnumTool<TState>.Int(CurState);
            int intstate = EnumTool<TState>.Int(state);
            return intState == intstate;
        }
        #endregion
    }

    #region base state
    public class State
    {
        public IStateMachine BaseStateMachine { get; set; }
        public bool IsManual { get; set; } = false;
        public bool IsForce { get; set; } = false;
        public float UpdateTime { get; set; } = 0.0f;
        public virtual void OnFixedUpdate() { }
        public virtual void UpdatePhysical() { }
        public virtual void Update()
        {
            UpdateTime += Time.deltaTime;
        }
        public virtual void Enter()
        {
            UpdateTime = 0;
        }
        public virtual void Exit() { }
    }
    public class CharaState<TState, TUnit> : State where TState : Enum where TUnit : BaseUnit
    {
        public float Wait { get; set; } = 0.0f;
        public TUnit SelfUnit { get; set; }
        public TState State { get; set; }
        public CharaState() : base() { }
        public override void Update()
        {
            base.Update();
            if (UpdateTime >= Wait) { }
        }
        public virtual void OnBeAdded() { }

        #region is
        protected bool IsLocalPlayer()
        {
            return SelfUnit.IsPlayer();
        }
        #endregion

    }
    #endregion
}
