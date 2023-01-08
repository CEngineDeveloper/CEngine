//------------------------------------------------------------------------------
// DBBaseStrcut.cs
// Copyright 2022 2022/11/6 
// Created by CYM on 2022/11/6
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using System;

namespace CYM.Unit
{
    [Serializable]
    public class DBBaseBuff : DBBase
    {
        public float CD = 0;
        public float Input;
        public bool Valid = true;
    }

    [Serializable]
    public class DBBaseAlert : DBBase
    {
        public float CurTurn;
        public bool IsCommingTimeOutFalg;
        public long Cast;
        public string TipStr;
        public string DetailStr;
        public string TitleStr;
        public string Illustration;
        public AlertType Type = AlertType.Continue;
        public string StartSFX;
        public bool IsAutoTrigger;
        public string Bg;
        public string Icon;
    }
    [Serializable]
    public class DBBaseEvent : DBBase
    {
        public int CD;
    }
}