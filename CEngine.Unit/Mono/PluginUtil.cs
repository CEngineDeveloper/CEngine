//------------------------------------------------------------------------------
// Util.cs
// Copyright 2022 2022/9/26 
// Created by CYM on 2022/9/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.Unit;
using System;

namespace CYM
{
    public partial class Util  
    {
        public static float GetUpFactionVal(IUpFactionData an, float? inputVal)
        {
            float newInputVal = 0;
            if (inputVal == null) newInputVal = an.InputVal;
            else newInputVal = inputVal.Value;
            //计算最终的InputVal
            var realInputVal = newInputVal - an.InputValStart;
            //百分比相乘
            if (an.FactionType == UpFactionType.Percent) return an.Val * realInputVal * an.Faction + an.Add;
            //百分比累加
            else if (an.FactionType == UpFactionType.PercentAdd) return an.Val * (1 + realInputVal * an.Faction) + an.Add;
            //线性累加
            else if (an.FactionType == UpFactionType.LinerAdd) return an.Val + realInputVal * an.Faction + an.Add;
            //指数累加
            else if (an.FactionType == UpFactionType.PowAdd) return an.Val + FormulaUtil.Pow(realInputVal, an.Faction) + an.Add;
            throw new Exception("错误的增长类型");
        }
    }
}