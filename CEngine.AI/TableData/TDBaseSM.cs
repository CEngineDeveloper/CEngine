//------------------------------------------------------------------------------
// TDBaseFSM.cs
// Copyright 2021 2021/1/29 
// Created by CYM on 2021/1/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using System;
using CYM.AI.FStateMachine;

namespace CYM.AI
{
    [Serializable]
    public class TDBaseSMData : TDBaseData
    {
        #region config
        public string DefaultState { get; set; } = null;
        #endregion

        #region 属性
        public IState RootState { get;private set; }
        #endregion

        #region 生命周期
        public override void OnBeAdded(BaseCoreMono mono, params object[] obj)
        {
            base.OnBeAdded(mono, obj);
            if (RootState == null)
            {
                RootState = CreateRootState();
                if (!DefaultState.IsInv())
                    RootState.ChangeState(new StrHash(DefaultState));
                else
                    RootState.ChangeDefaultState();
            }
            else
            {
                CLog.Error("没有移除:RootState");
            }
        }
        public override void OnBeRemoved()
        {
            base.OnBeRemoved();
            RootState = null;
        }
        public override void ManualUpdate()
        {
            base.ManualUpdate();
            RootState?.Update(Time.deltaTime);
        }
        #endregion

        #region is
        public bool IsInState(StrHash stateName)
        {
            if (!IsHaveState(stateName))
                return false;
            return RootState.IsInState(stateName);
        }
        public bool IsHaveState(StrHash stateName)
        {
            return RootState.IsHaveState(stateName);
        }
        #endregion

        #region set
        public void ChangeState(StrHash stateName,Callback callback=null)
        {
            if (RootState.IsInState(stateName))
                return;
            callback?.Invoke();
            RootState?.ChangeState(stateName);
        }
        public void TriggerEvent(string name)
        {
            RootState.TriggerEvent(name);
        }
        public virtual IState CreateRootState()
        {
            throw new NotImplementedException();
            //var rootState = new StateMachineBuilder()
            //.State("Swimming")
            //    .Update((state, time) =>
            //    {

            //    })
            //    .State("Hunting")
            //        .Update((state, time) =>
            //        {

            //        })
            //    .End()
            //.End()
            //.Build();
            //return rootState;
        }
        #endregion
    }
}