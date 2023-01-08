//------------------------------------------------------------------------------
// BaseFormulaUtils.cs
// Copyright 2019 2019/8/28 
// Created by CYM on 2019/8/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public partial class FormulaUtil
    {
        //指数基数
        //用于较短的升级曲线,基于一个基础值,和一个差距因子
        public static float PowBase(float lv, float baseVal = 1, float gapFaction = 1)
        {
            if (lv <= 0) return 0;
            return Mathf.Pow(MathUtil.Clamp0(lv), 2) * gapFaction + baseVal;
        }
        //简单指数
        public static float Pow(float lv, float gapFaction = 1)
        {
            if (lv <= 0) return 0;
            return Mathf.Pow(MathUtil.Clamp0(lv), 2) * gapFaction;
        }
    }
}