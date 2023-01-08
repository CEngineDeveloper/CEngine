//------------------------------------------------------------------------------
// PluginDBStrcut.cs
// Copyright 2022 2022/12/26 
// Created by CYM on 2022/12/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CYM.Person
{
    [Serializable]
    public class DBBasePerson : DBBase
    {
        public string Theory = SysConst.STR_Inv;
        public string Civil = "Name_中原";
        public string FirstName = SysConst.STR_None;
        public string LastName = SysConst.STR_None;
        public Gender Gender = Gender.Male;
        public int Age = SysConst.INT_Inv;
        public AgeRange AgeRange = AgeRange.Adult;
        public Dictionary<PHIPart, string> HeadIcon = new Dictionary<PHIPart, string>();
        public Dictionary<PHIPart, string> ChildHeadIcon = new Dictionary<PHIPart, string>();
        public Dictionary<PHIPart, string> OldHeadIcon = new Dictionary<PHIPart, string>();
    }
}