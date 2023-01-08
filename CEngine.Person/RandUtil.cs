//------------------------------------------------------------------------------
// RandUtil.cs
// Copyright 2022 2022/12/26 
// Created by CYM on 2022/12/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using System.Collections.Generic;
using CYM.Person;

namespace CYM
{
    public partial class RandUtil 
    {
        public static int RandAge(List<AgeRange> range)
        {
            return RandUtil.RangeInt(GameConfig.Ins.AgeRangeData[range.Rand()]);
        }
    }
}