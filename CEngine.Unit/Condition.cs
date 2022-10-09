//------------------------------------------------------------------------------
// Condition.cs
// Copyright 2022 2022/9/26 
// Created by CYM on 2022/9/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CYM.Unit
{
    #region Cost
    // 消耗判断
    public class BaseCostCondition<TUnit, Type> : BaseCondition
        where Type : Enum
        where TUnit : BaseUnit
    {
        public List<Cost<Type>> CostDatas;
        public TUnit Unit => ACM.ParamSelfBaseUnit as TUnit;

        public BaseCostCondition() : base()
        {
            CostDatas = null;
            IsCost = true;
        }
        public BaseCostCondition<TUnit, Type> SetCost(List<Cost<Type>> datas)
        {
            foreach (var item in datas)
                item.SetUnit(Unit);
            CostDatas = datas;
            return this;
        }
        public BaseCostCondition<TUnit, Type> SetCost(Cost<Type> datas)
        {
            datas.SetUnit(Unit);
            CostDatas = new List<Cost<Type>>();
            CostDatas.Add(datas);
            return this;
        }
        protected override bool OnCheckError()
        {
            if (CostDatas == null)
            {
                CLog.Error("错误:没有设置CostDatas");
                return false;
            }
            if (Unit == null)
            {
                CLog.Error("错误!没有设置Unit!");
                return false;
            }
            return true;
        }
        protected override bool OnDoActionImpl()
        {
            foreach (var item in CostDatas)
            {
                if (item.IsCondition)
                {
                    if (GetAttrVal(item.Type) < item.RealVal)
                        return false;
                }
            }
            return true;
        }
        //此类型,基本不显示条件(保留)
        public override string GetDesc()
        {
            string retStr = "";
            int index = 0;
            if (CostDatas == null) return retStr;
            foreach (var item in CostDatas)
            {
                if (item.RealVal == 0) continue;
                if (!item.IsCondition) continue;

                bool tempBool = true;
                if (MathUtil.Round(GetAttrVal(item.Type), 2) < item.RealVal)
                    tempBool = false;
                string tempstr = BaseLangMgr.Get("AC_IsAttrToAct", item.Type.GetName(), ACCompareType.MoreEqual.GetFull().GetName(), item.GetDesc(false, false, false));
                if (tempBool) retStr += SysConst.STR_Indent + UIUtil.Green(tempstr);
                else retStr += SysConst.STR_Indent + UIUtil.Red(tempstr);

                if (index < CostDatas.Count - 1)
                    retStr += "\n";
                index++;
            }
            return retStr;
        }
        public override string GetCost()
        {
            int index = 0;
            string ret = "";
            string retStr = "";
            if (CostDatas == null) return retStr;
            foreach (var item in CostDatas)
            {
                if (item.RealVal == 0) continue;

                bool tempBool = true;
                if (MathUtil.Round(GetAttrVal(item.Type), 2) < item.RealVal) tempBool = false;

                string temp = item.GetDesc(false, false);
                if (tempBool || !item.IsCondition) temp = SysConst.STR_Indent + UIUtil.Green(temp);
                else temp = SysConst.STR_Indent + UIUtil.Red(temp);

                if (index < CostDatas.Count - 1)
                    temp += "\n";
                index++;
                ret += temp;
            }
            return ret;
        }
        protected float GetAttrVal(Type type)
        {
            if (Unit.AttrMgr == null)
            {
                CLog.Error($"BaseCostCondition 错误，单位缺失：{nameof(Unit.AttrMgr)}组建");
                return 0;
            }
            return Unit.AttrMgr.GetVal(EnumTool<Type>.Int(type));
        }
    }
    #endregion

    #region Attr
    public class BaseAttrCondition<TUnit, Type> : BaseCondition
        where Type : Enum
        where TUnit : BaseUnit
    {
        protected Type AttrType;
        public TUnit Unit => ACM.ParamSelfBaseUnit as TUnit;

        public BaseCondition SetAttr(Type type)
        {
            AttrType = type;
            return this;
        }
        protected override bool OnDoActionImpl()
        {
            return DoCompare(GetAttrVal(AttrType));
        }
        public override string GetDesc()
        {
            TDBaseAttrData attrData = BaseAttrMgr<Type>.GetAttrDataList()[EnumTool<Type>.Int(AttrType)];
            return GetStr("AC_IsAttrToAct", (AttrType as Enum).GetName(), CompareType.GetName(), BaseAttrMgr<Type>.GetAttrNumberStr(AttrType, val));
        }

        #region 必须重载的函数
        protected virtual float GetAttrVal(Type type)
        {
            if (Unit.AttrMgr == null)
            {
                CLog.Error($"BaseCostCondition 错误，单位缺失：{nameof(Unit.AttrMgr)}组建");
                return 0;
            }
            return Unit.AttrMgr.GetVal(EnumTool<Type>.Int(type));
        }
        #endregion
    }
    #endregion
}