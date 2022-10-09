//------------------------------------------------------------------------------
// BaseBalancAction.cs
// Copyright 2020 2020/5/15 
// Created by CYM on 2020/5/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.AI;
using System;
using System.Collections.Generic;

namespace CYM
{
    public class BalanceAction<TUnit> 
        where TUnit : BaseUnit 
    {
        #region prop
        protected Balancer<TUnit> Balancer { get; private set; }
        protected TUnit SelfUnit { get; private set; }
        #endregion

        #region buget
        private float _outputBuget = 0;
        private float _outputGroupBuget = 0;
        public float OutputBuget => _outputBuget;
        #endregion

        #region prop
        protected virtual int GroupCount => 0;
        public BalanceActionType Type { get; private set; } = BalanceActionType.Fire;
        #endregion

        #region life
        public void Init(TUnit unit, Balancer<TUnit> balancer, BalanceActionType type)
        {
            SelfUnit = unit;
            Balancer = balancer;
            Type = type;
        }
        protected virtual bool ForGroupData()
        {
            bool succ = false;
            if (GroupCount > 0)
            {
                for (int i = 0; i < GroupCount - 1; ++i)
                {
                    if (DoGroup(i)) succ = true;
                    else break;
                }
            }
            return succ;
        }
        #endregion

        #region set
        protected void HireLog(string str, params object[] obj)
        {
            if (SysConsole.Ins.IsOnlyPlayerAI)
                CLog.Green(str, obj);
        }
        protected void FireLog(string str, params object[] obj)
        {
            if (SysConsole.Ins.IsOnlyPlayerAI)
                CLog.Red(str, obj);
        }
        protected void ChangeLeftBuget(float val) => Balancer.ChangeLeftBuget(val);
        #endregion

        #region do
        public bool Do()
        {
            _outputGroupBuget = 0;
            _outputBuget = 0;
            OnInitData();
            bool isDo = OnDo(ref _outputBuget);
            bool isForGroup = ForGroupData();
            bool isSucc = isDo || isForGroup;
            if (SysConsole.Ins.IsOnlyPlayerAI)
                OnLog(isSucc);
            return isSucc;
        }
        protected bool DoGroup(object obj)
        {
            if (OnDogroup(obj))
            {
                if (Type == BalanceActionType.Hire)
                {
                    Balancer.ChangeLeftBuget(-_outputGroupBuget);
                    if (!Balancer.IsNoBuget()) return true;
                    else return false;
                }
                else if (Type == BalanceActionType.Fire)
                {
                    Balancer.ChangeLeftBuget(_outputGroupBuget);
                    if (!Balancer.IsEnoughBuget()) return true;
                    else return false;
                }
            }
            return true;
        }
        #endregion

        #region Callback
        //执行成功,返回:true,进行预算判断
        //执行失败,返回:false,不进行预算判断
        protected virtual bool OnDogroup(object data)
        {
            return true;
        }
        protected virtual bool OnDo(ref float outputBuget)
        {
            return true;
        }
        protected virtual void OnInitData()
        {

        }
        protected virtual void OnLog(bool b)
        {

        }
        #endregion

        #region is
        public virtual bool IsValid() => true;
        protected bool IsEnoughBuget(float val) => Balancer.IsEnoughBuget(val);
        public bool IsNoBuget() => Balancer.IsNoBuget();
        public bool IsEnoughBuget() => Balancer.IsEnoughBuget();
        #endregion

        #region check
        protected bool CheckGroupEnoughBuget(float val)
        {
            if (Balancer.LeftBuget > val)
            {
                _outputGroupBuget = val;
                return true;
            }
            return false;
        }
        protected bool CheckEnoughBuget(float val)
        {
            if (Balancer.LeftBuget > val)
            {
                _outputBuget = val;
                return true;
            }
            else
            {

            }
            return false;
        }
        #endregion
    }
    public class BalanceActionList<TUnit, TData> : BalanceAction<TUnit> 
        where TUnit : BaseUnit
    {
        protected virtual List<TData> Datas => throw new NotImplementedException();
        protected override bool ForGroupData()
        {
            bool succ = false;
            if (Datas != null)
            {
                foreach (var item in Datas)
                {
                    if (DoGroup(item)) succ = true;
                    else break;
                }
            }
            return succ;
        }
    }
    public class BalanceActionDicList<TUnit, TData> : BalanceAction<TUnit> 
        where TUnit : BaseUnit 
        where TData : IBase
    {
        protected virtual IDDicList<TData> Datas => throw new NotImplementedException();
        protected override bool ForGroupData()
        {
            bool succ = false;
            if (Datas != null)
            {
                foreach (var item in Datas)
                {
                    if (DoGroup(item)) succ = true;
                    else break;
                }
            }
            return succ;
        }
    }
}