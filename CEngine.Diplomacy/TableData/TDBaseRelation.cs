//------------------------------------------------------------------------------
// TDBaseRelation.cs
// Created by CYM on 2023/1/20
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using System;
using System.Collections.Generic;

namespace CYM.Diplomacy
{
    [Serializable]
    public class TDBaseWarData : TDBaseData
    {
        public List<string> Attackers { get; set; } = new List<string>();
        public List<string> Defensers { get; set; } = new List<string>();
        public HashList<string> AllowOccupy { get; set; } = new HashList<string>();
        public float AttackersWarPoint { get; set; }
        public float DefensersWarPoint { get; set; }
        public int WarDay { get; set; } = 0;
    }
    [Serializable]
    public class TDBaseRelationData : TDBaseData
    {
        public MultiDic<string, int> RelationShip { get; set; } = new MultiDic<string, int>();
        public MultiDic<string, bool> Alliance { get; set; } = new MultiDic<string, bool>();
        public MultiDic<string, bool> Marriage { get; set; } = new MultiDic<string, bool>();
        public Dictionary<string, HashList<string>> Vassal { get; set; } = new Dictionary<string, HashList<string>>();
        public List<TDBaseWarData> Warfare { get; set; } = new List<TDBaseWarData>();
    }
}