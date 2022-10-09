//------------------------------------------------------------------------------
// AITargetFinder.cs
// Copyright 2020 2020/2/3 
// Created by CYM on 2020/2/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CYM.AI
{
    public abstract class AITargetFinder<T> where T : BaseUnit
    {
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        public T CurTarget { get; protected set; }
        public T Find()
        {
            CurTarget = Calc();
            return CurTarget;
        }
        protected virtual T Calc()
        {
            T target = null;
            List<T> allEnemies = GetAllTargets();
            target = GetNearestTarget(allEnemies);
            return target;
        }
        protected virtual bool IsContinue(T unit)
        {
            return false;
        }
        protected virtual Vector3 GetStartPos() => throw new NotImplementedException();
        #region get data
        protected virtual List<T> GetAllTargets()
        {
            throw new Exception("此函数必须实现:GetAllTargets");
        }
        protected T GetNearestTarget(List<T> data)
        {
            if (data == null)
                return null;
            T target = null; //data.FirstOrDefault();
            float maxDis = float.MaxValue;
            foreach (var item in data)
            {
                if (IsContinue(item)) continue;
                var curDis = MathUtil.SqrDistance(GetStartPos(), item.Pos);
                if (curDis < maxDis)
                {
                    maxDis = curDis;
                    target = item;
                }
            }
            return target;
        }
        #endregion
    }

    public class AINearestFinder<T>: AITargetFinder<T>
         where T : BaseUnit
    {
        Func<List<T>> getAllTargets;
        Func<T,bool> isContinue;
        Func<Vector3> getStartPos;
        public AINearestFinder(Func<Vector3> getPos, Func<List<T>> getAllTargets, Func<T, bool> isContinue)
        {
            this.getAllTargets = getAllTargets;
            this.isContinue = isContinue;
            this.getStartPos = getPos;
        }
        protected override List<T> GetAllTargets()
        {
            if (getAllTargets == null)
                return new List<T>();
            return getAllTargets?.Invoke();
        }
        protected override bool IsContinue(T unit)
        {
            if (isContinue == null)
                return false;
            return isContinue.Invoke(unit) ;
        }
        protected override Vector3 GetStartPos()
        {
            return getStartPos.Invoke();
        }

        //public static AINearestFinder<T> Ins { get;private set; }
        public static T Find(Func<Vector3> getPos, Func<List<T>> getAllTargets, Func<T, bool> isContinue)
        {
            //if (Ins == null)
            var data = new AINearestFinder<T>(getPos, getAllTargets, isContinue);
            return data.Find();
        }
    }
    public class AISpreadTargetFinder<T> : AITargetFinder<T> where T : BaseUnit
    {
        public virtual int MaxAlliesPerEnemy => 4;

        public virtual bool IsValid(T target)
        {
            int numberOfUnitsAttackingThisEnemy = 0;
            foreach (var ally in GetAllAllys())
            {
                if (ally.AIMgr.CurTarget == target)
                    numberOfUnitsAttackingThisEnemy++;
            }
            if (numberOfUnitsAttackingThisEnemy < MaxAlliesPerEnemy)
                return true;
            return false;
        }

        protected override T Calc()
        {
            T target = null;
            List<T> allEnemies = GetAllTargets();
            if (allEnemies.Count == 0)
                return null;

            List<T> validEnemies = new List<T>();
            //找到所有有效的目标
            foreach (var item in allEnemies)
            {
                if (IsValid(item))
                    validEnemies.Add(item);
            }
            //计算距离最近的单位
            if (validEnemies.Count > 0)
            {
                target = GetNearestTarget(validEnemies);
            }
            else
            {
                target = GetNearestTarget(allEnemies);
            }

            return target;
        }

        #region get data
        protected virtual List<T> GetAllAllys()
        {
            throw new System.Exception("此函数必须实现:GetAllAllys");
        }
        #endregion
    }
}