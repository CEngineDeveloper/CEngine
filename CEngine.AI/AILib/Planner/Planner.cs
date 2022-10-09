//------------------------------------------------------------------------------
// Planner.cs
// Copyright 2020 2020/5/14 
// Created by CYM on 2020/5/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM.AI.Planner
{
    public enum PlannerType
    {
        Sequence, //顺序执行,统一帧里全部执行,但是只有当执行成功后才会继续执行下一个
        Select,   //选择执行,统一帧里全部执行,但是只有当执行失败后才会继续执行下一个
        Parallel, //并行执行,统一帧里全部执行
        Random,   //随机执行(一次只会执行一个)
        Randpeat, //循环随机,有一个执行成功则停止(一次只会执行一个)
        Step,     //依次执行,完成一个以后再下一个(一次只会执行一个)
    }
    public class Planner<TUnit, TGlobal> where TUnit : BaseUnit where TGlobal : BaseGlobal
    {
        #region prop
        public List<PlanAction<TUnit>> Source { get; private set; } = new List<PlanAction<TUnit>>();
        public List<PlanAction<TUnit>> Data { get; private set; } = new List<PlanAction<TUnit>>();
        List<PlanAction<TUnit>> clearData = new List<PlanAction<TUnit>>();
        protected TUnit SelfUnit { get; private set; }
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        #endregion

        #region param
        protected virtual PlannerType Type => PlannerType.Step;
        //是否随机排序
        protected virtual bool IsRandomSort => true;
        //是否执行成功后删除
        protected virtual bool IsRemoveSucceed => false;
        //是否将所有不符合条件的都删除
        protected virtual bool IsRemoveInvalid => false;
        #endregion

        #region constution
        public void Init(TUnit unit)
        {
            SelfUnit = unit;
            OnAddActions();
            Source.AddRange(Data);
            DoRandomSort();
        }
        #endregion

        #region set
        protected void Add(PlanAction<TUnit> action)
        {
            action.Init(SelfUnit);
            Data.Add(action);
        }
        public void Update()
        {
            //如果没有,则再次添加
            if (Data.Count <= 0)
            {
                foreach (var item in Source)
                {
                    if (IsRemoveInvalid)
                    {
                        if (item.IsValid())
                            Data.Add(item);
                    }
                    else Data.Add(item);
                }
                DoRandomSort();
            }
            if (Data.Count <= 0) return;
            //删除无效的Action
            if (IsRemoveInvalid)
            {
                Data.RemoveAll(x => !x.IsValid());
            }

            //执行
            clearData.Clear();
            if (Type == PlannerType.Sequence)
            {
                foreach (var data in Data)
                {
                    var succ = data.IsValid() && data.Do();
                    if (succ)
                    {
                        if (IsRemoveSucceed)
                            clearData.Add(data);
                    }
                    else break;
                }
                foreach (var item in clearData)
                    Data.Remove(item);
            }
            if (Type == PlannerType.Select)
            {
                foreach (var data in Data)
                {
                    var succ = data.IsValid() && data.Do();
                    if (succ)
                    {
                        if (IsRemoveSucceed)
                            clearData.Add(data);
                        break;
                    }
                    else { }
                }
            }
            else if (Type == PlannerType.Parallel)
            {
                foreach (var data in Data)
                {
                    var succ = data.IsValid() && data.Do();
                    if (succ && IsRemoveSucceed)
                        clearData.Add(data);
                }
            }
            else if (Type == PlannerType.Step)
            {
                var data = Data[0];
                var succ = data.IsValid() && data.Do();
                if (succ) Data.Remove(data);
            }
            else if (Type == PlannerType.Random)
            {
                var data = Data.Rand();
                var succ = data.IsValid() && data.Do();
                if (succ && IsRemoveSucceed)
                    Data.Remove(data);
            }
            else if (Type == PlannerType.Randpeat)
            {
                for (int i = 0; i < Data.Count; ++i)
                {
                    var data = Data.Rand();
                    var succ = data.IsValid() && data.Do();
                    if (succ)
                    {
                        if (IsRemoveSucceed)
                            Data.Remove(data);
                        break;
                    }
                }
            }

            //删除
            foreach (var item in clearData)
                Data.Remove(item);
        }
        #endregion

        #region Callback
        protected virtual void OnAddActions()
        {

        }
        protected virtual void DoRandomSort()
        {
            if (!IsRandomSort) return;
            if (Data.Count <= 0) return;
            Data = Data.RandomSort();
        }
        #endregion
    }
}