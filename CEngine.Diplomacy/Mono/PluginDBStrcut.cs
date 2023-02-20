//------------------------------------------------------------------------------
// PluginDBStrcut.cs
// Copyright 2023 2023/1/20 
// Created by CYM on 2023/1/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Diplomacy;
using System;
using System.Collections.Generic;

namespace CYM
{
    [Serializable]
    public class DBBaseWar : DBBase
    {
        public List<string> Attackers = new List<string>();
        public List<string> Defensers = new List<string>();
        public HashList<string> AllowLand = new HashList<string>();
        public int WarDay = 0;
        public float AttackersWarPoint;
        public float DefensersWarPoint;
    }

    [Serializable]
    public class DBBaseArticle : DBBase
    {
        public long Self;
        public long Target;

        public float Float1;
        public float Float2;
        public float Float3;
        public int Int1;
        public int Int2;
        public int Int3;
        public string Str1;
        public string Str2;
        public string Str3;
        public bool Bool1;
        public bool Bool2;
        public bool Bool3;
        public long Long1;
        public long Long2;
        public long Long3;

        public ArticleObjType ArticleObjType = ArticleObjType.Self;
    }
}