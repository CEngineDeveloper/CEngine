//------------------------------------------------------------------------------
// BaseImmuneMgr.cs
// Copyright 2018 2018/4/7 
// Created by CYM on 2018/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
namespace CYM.Unit
{
    /// <summary>
    /// 增益或者减益类型
    /// </summary>
    public enum ImmuneGainType
    {
        All = 0,
        Negative = 1,//负面
        Positive = 2,//正面
    }
    /// <summary>
    /// 效果的免疫类型
    /// </summary>
    public enum ImmuneType
    {
        All = 0,
        Normal = 1, //一般类型:普攻
        Magic = 2,  //魔法类型:技能
    }
    public struct ImmuneData
    {
        public ImmuneData(ImmuneGainType gainType, ImmuneType immuneType, bool igron)
        {
            GainType = gainType;
            ImmuneType = immuneType;
            IsIgron = igron;
        }
        public ImmuneGainType GainType;
        public ImmuneType ImmuneType;
        public bool IsIgron;
    }
    public class BaseImmuneMgr : BaseMgr
    {
        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        #endregion

        List<ImmuneData> _datas = new List<ImmuneData>();

        public void AddImmunity(ImmuneData effectDataType)
        {
            _datas.Add(effectDataType);
        }

        public void RemoveImmunity(ImmuneData effectDataType)
        {
            _datas.Remove(effectDataType);
        }

        public bool IsImmunity(ImmuneData effectDataType)
        {
            bool isImmunity = false;
            for (int i = 0; i < _datas.Count; i++)
            {
                if (IsImmunity(effectDataType, _datas[i]))
                    isImmunity = true;
            }
            return isImmunity;
        }

        bool IsImmunity(ImmuneData targetDataType, ImmuneData dataType)
        {
            if (targetDataType.IsIgron)
                return false;
            return (targetDataType.GainType == dataType.GainType || dataType.GainType == ImmuneGainType.All) &&
                (targetDataType.ImmuneType == dataType.ImmuneType || dataType.ImmuneType == ImmuneType.All);
        }
    }
}

