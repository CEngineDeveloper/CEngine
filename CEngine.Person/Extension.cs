//------------------------------------------------------------------------------
// Extension.cs
// Copyright 2022 2022/12/26 
// Created by CYM on 2022/12/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
using CYM.Person;

namespace CYM
{
    public static partial class Extension
    {
        public static int GetAge(this List<AgeRange> ages)
        {
            return RandUtil.RandAge(ages);
        }
    }
}