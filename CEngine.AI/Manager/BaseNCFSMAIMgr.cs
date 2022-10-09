//------------------------------------------------------------------------------
// BaseNCFSMAIMgr.cs
// Created by CYM on 2022/7/6
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using NodeCanvas.StateMachines;
using System.Collections.Generic;
using NodeCanvas.Framework;
namespace CYM.AI
{
    public class BaseNCFSMAIMgr<TBlackboard> : BaseAIMgr where TBlackboard: new()
    {
        #region prop
        protected FSMOwner FSMOwner { get; private set; }       
        protected FSM FSM { get; private set; }
        protected Dictionary<string, Variable> Variables => IBlackboard.variables;
        protected Blackboard Blackboard { get; private set; }
        IBlackboard IBlackboard => Blackboard;
        public IState CurTask { get; private set; }
        public TBlackboard MyBlackboard { get; private set; } = new TBlackboard();
        #endregion

        #region life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            FSMOwner = SelfMono.SetupMonoBehaviour<FSMOwner>();
            FSMOwner.updateMode = Graph.UpdateMode.Manual;
            Blackboard = SelfMono.SetupMonoBehaviour<Blackboard>();
            FSMOwner.blackboard = Blackboard;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (FSM)
            {
                GameObject.Destroy(FSM);
            }
        }
        #endregion

        #region Manual update
        protected void ManualUpdateAI()
        {
            if (!IsActiveAI)
                return;
            FSMOwner?.UpdateBehaviour();
            CurTask = FSMOwner.GetCurrentState(false);
        }
        #endregion

        #region Set
        public IState TriggerState(string stateName) => FSMOwner.TriggerState(stateName);
        public void SwitchBehaviour(FSM newFSM)
        {
            FSM old = FSMOwner.behaviour;
            if (FSMOwner.behaviour == newFSM)
                return;
            if (old)
            {
                GameObject.Destroy(old);
            }
            FSMOwner.SwitchBehaviour(newFSM);       
        }
        public void SwitchBehaviour(string name)
        {
            FSM = GameObject.Instantiate<FSM>(Resources.Load<FSM>(name));
            SwitchBehaviour(FSM);
        }
        public Variable<T> AddVariable<T>(string varName, T value = default)
        {
            return Blackboard.AddVariable<T>(varName, value);
        }
        public void SetValue(string key,Variable variable)
        {
            Variables[key] = variable;
        }
        #endregion
    }
}