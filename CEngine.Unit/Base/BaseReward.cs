//------------------------------------------------------------------------------
// BaseReward.cs
// Copyright 2019 2019/8/3 
// Created by CYM on 2019/8/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CYM.Unit
{
    #region 奖励
    public class BaseReward : ILuaObj
    {
        public BaseUnit TargetUnit { get; private set; }
        public virtual string GetDesc()
        {
            string final = "";
            return final;
        }

        public virtual void Do() { }
        public virtual void UnDo() { }
        public void OnBeCreated() { }
        public virtual void SetTarget(BaseUnit target)
        {
            TargetUnit = target;
        }

        #region get
        protected string GetStr(string key, params object[] ps)
        {
            return BaseLangMgr.Get(key, ps);
        }
        #endregion
    }
    #endregion

    #region reward cost
    /// <summary>
    /// Cost奖励，支持Cost的配置
    /// </summary>
    /// <typeparam name="TUnit"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class BaseRewardCost<TUnit, T> : BaseReward 
        where TUnit : BaseUnit 
        where T : Enum
    {
        #region config
        public List<Cost<T>> Data { get; set; }
        #endregion

        #region get
        public override string GetDesc()
        {
            return GetCostStr(Data);
        }
        public static string GetCostStr(List<Cost<T>> cost)
        {
            string final = "";

            List<TDBaseAttrData> tempAttrData = TDAttr<T>.AttrDataList[typeof(T)];
            if (tempAttrData == null)
                return "";

            if (cost != null)
            {
                for (int i = 0; i < cost.Count; ++i)
                {
                    var item = cost[i];
                    if (i != 0) final += "\n";
                    final += SysConst.STR_DoubbleIndent + item.GetDesc(true, true);
                }
            }
            if (final.IsInv())
                return "";
            return Util.GetStr("Text_Reward_Cost", final);
        }
        #endregion

        #region set
        public override void SetTarget(BaseUnit target)
        {
            base.SetTarget(target);
            if (Data != null)
            {
                foreach (var item in Data)
                {
                    item.SetUnit(target);
                }
            }
        }
        #endregion

        #region do
        public override void Do()
        {
            if (TargetUnit == null) return;
            if (TargetUnit.AttrMgr == null)
            {
                CLog.Error($"BaseRewardSimple 错误，单位缺失：{nameof(TargetUnit.AttrMgr)}组建");
                return;
            }
            TargetUnit.AttrMgr.DoCost(Data, true);
        }
        #endregion
    }
    #endregion

    #region reward buff
    /// <summary>
    /// Buff奖励，支持Buff的配置
    /// </summary>
    /// <typeparam name="TUnit"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TType"></typeparam>
    public class BaseRewardBuff<TUnit, TData, TType> : BaseReward where TUnit : BaseUnit where TData : TDBaseBuffData<TType>, new() where TType : Enum
    {
        #region config
        public List<string> Data { get; set; }
        #endregion

        #region get
        public override string GetDesc()
        {
            return GetBuffStr(TargetUnit, Data);
        }
        public static string GetBuffStr(BaseUnit targetUnit, List<string> buff)
        {
            string final = "";
            if (buff != null)
            {
                int index = 0;
                foreach (var item in buff)
                {
                    if (index != 0) final += "\n";
                    final += SysConst.STR_DoubbleSpace + targetUnit.BuffMgr.GetTableDesc(item, true, SysConst.STR_DoubbleIndent, null, true);
                    index++;
                }
            }
            if (final.IsInv())
                return "";
            return Util.GetStr("Text_Reward_Buff", final);
        }
        #endregion

        #region do
        public override void Do()
        {
            if (TargetUnit == null) return;
            TargetUnit.BuffMgr.Add(Data);
        }
        public override void UnDo()
        {
            if (TargetUnit == null) return;
            TargetUnit.BuffMgr.Remove(Data);
        }
        #endregion
    }
    #endregion

    #region reward buff
    /// <summary>
    /// 简单奖励，支持Buff和Cost的配置
    /// </summary>
    /// <typeparam name="TUnit"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TType"></typeparam>
    public class BaseRewardSimple<TUnit, TData, TType> : BaseReward 
        where TUnit : BaseUnit 
        where TData : TDBaseBuffData<TType>, new() where TType : Enum
    {
        #region config
        public List<string> Buff { get; set; }
        public List<Cost<TType>> Cost { get; set; }
        #endregion

        #region get
        public override string GetDesc()
        {
            string ret = "";
            string buffStr = BaseRewardBuff<TUnit, TData, TType>.GetBuffStr(TargetUnit, Buff);
            string costStr = BaseRewardCost<TUnit, TType>.GetCostStr(Cost);
            if (!costStr.IsInv()) ret += costStr + "\n";
            if (!buffStr.IsInv()) ret += buffStr;
            ret = ret.TrimEnd("\n");
            return ret;
        }
        #endregion

        #region set
        public override void SetTarget(BaseUnit target)
        {
            base.SetTarget(target);
            if (Cost != null)
            {
                foreach (var item in Cost)
                {
                    item.SetUnit(target);
                }
            }
        }
        #endregion

        #region do
        public override void Do()
        {
            if (TargetUnit == null) return;
            if (TargetUnit.AttrMgr == null)
            {
                CLog.Error($"BaseRewardSimple 错误，单位缺失：{nameof(TargetUnit.AttrMgr)}组建");
                return;
            }
            if (TargetUnit.BuffMgr == null)
            {
                CLog.Error($"BaseRewardSimple 错误，单位缺失：{nameof(TargetUnit.BuffMgr)}组建");
                return;
            }
            TargetUnit.AttrMgr.DoCost(Cost, true);
            TargetUnit.BuffMgr.Add(Buff);
        }
        public override void UnDo()
        {
            if (TargetUnit == null) return;
            if (TargetUnit.AttrMgr == null)
            {
                CLog.Error($"BaseRewardSimple 错误，单位缺失：{nameof(TargetUnit.AttrMgr)}组建");
                return;
            }
            if (TargetUnit.BuffMgr == null)
            {
                CLog.Error($"BaseRewardSimple 错误，单位缺失：{nameof(TargetUnit.BuffMgr)}组建");
                return;
            }
            TargetUnit.AttrMgr.DoCost(Cost, false);
            TargetUnit.BuffMgr.Remove(Buff);
        }
        #endregion
    }
    #endregion
}