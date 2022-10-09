//------------------------------------------------------------------------------
// BaseBalancer.cs
// Copyright 2020 2020/5/14 
// Created by CYM on 2020/5/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM.AI
{
    public class Balancer<TUnit> 
        where TUnit : BaseUnit 
    {
        #region prop
        protected TUnit SelfUnit { get; private set; }
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        public List<BalanceAction<TUnit>> HireData { get; private set; } = new List<BalanceAction<TUnit>>();
        public List<BalanceAction<TUnit>> FireData { get; private set; } = new List<BalanceAction<TUnit>>();
        protected virtual float Buget => 0;
        protected virtual float Threshold => 0;
        //预算不足的最大时长
        protected virtual int MaxNoBugetCount => 0;
        //预算充足的最大时长
        protected virtual int MaxEnoughBugetCount => 0;
        protected int CurNoBugetCount { get; private set; } = 0;
        protected int CurEnoughBugetCount { get; private set; } = 0;
        public float LeftBuget { get; protected set; } = 0;
        #endregion

        #region constution
        public void Init(TUnit unit)
        {
            SelfUnit = unit;
            OnAddActions();
        }
        #endregion

        #region set
        public void Update()
        {
            LeftBuget = Buget;
            if (IsNoBuget()) CurNoBugetCount++;
            else CurNoBugetCount = 0;
            if (IsEnoughBuget()) CurEnoughBugetCount++;
            else CurEnoughBugetCount = 0;

            //解雇
            if (IsLongNoBuget())
            {
                foreach (var item in FireData)
                {
                    if (!item.IsValid()) continue;
                    bool succ = item.Do();
                    if (succ)
                    {
                        ChangeLeftBuget(item.OutputBuget);
                    }
                    if (!IsNoBuget())
                        break;
                }
            }
            //招聘
            else if (IsLongEnoughBuget())
            {
                foreach (var item in HireData)
                {
                    if (!item.IsValid()) continue;
                    bool succ = item.Do();
                    if (succ)
                    {
                        ChangeLeftBuget(-item.OutputBuget);
                    }
                    if (!IsEnoughBuget())
                    {
                        break;
                    }
                }
            }
        }
        protected void AddHire(BalanceAction<TUnit> action)
        {
            action.Init(SelfUnit, this, BalanceActionType.Hire);
            HireData.Add(action);
        }
        protected void AddFire(BalanceAction<TUnit> action)
        {
            action.Init(SelfUnit, this, BalanceActionType.Fire);
            FireData.Add(action);
        }
        public void ChangeLeftBuget(float val)
        {
            LeftBuget += val;
        }
        #endregion

        #region is
        public virtual bool IsLongNoBuget()
        {
            return IsNoBuget() && CurNoBugetCount >= MaxNoBugetCount;
        }
        public virtual bool IsLongEnoughBuget()
        {
            return IsEnoughBuget() && CurEnoughBugetCount >= MaxEnoughBugetCount;
        }
        public virtual bool IsNoBuget()
        {
            return LeftBuget <= 0;
        }
        public virtual bool IsEnoughBuget()
        {
            return LeftBuget > Threshold;
        }
        public virtual bool IsEnoughBuget(float val)
        {
            if (LeftBuget > val) return true;
            return false;
        }
        #endregion

        #region Callback
        protected virtual void OnAddActions()
        {

        }
        #endregion
    }
}