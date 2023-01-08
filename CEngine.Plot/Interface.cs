//------------------------------------------------------------------------------
// Interface.cs
// Copyright 2022 2022/11/8 
// Created by CYM on 2022/11/8
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using System;

namespace CYM.Plot
{
    public interface ITalkMgr
    {
        #region set
        TalkFragment StartOption(string id);
        TalkFragment Start(string id, int index = 0);
        TalkFragment Next();
        void ClickOption(int index);
        void ClickTalk();
        string SelectOption(int index);
        void SelectPreOption();
        void SelectNextOption();
        void Stop();
        bool IsHave();
        bool IsInOption();
        bool IsLockNextTalk { get; }
        #endregion

        #region get
        TalkFragment CurTalkFragment();
        #endregion
    }
    public interface INarrationMgr<out TDataOut>
    {
        #region Callback
        event Callback<TDataOut, NarrationFragment> Callback_OnStart;
        event Callback<TDataOut, NarrationFragment, int> Callback_OnNext;
        event Callback<TDataOut, NarrationFragment> Callback_OnEnd;
        event Callback<TDataOut> Callback_OnChange;
        #endregion

        #region set
        NarrationFragment Start(string id, Callback<TDataOut> endAction = null);
        NarrationFragment Next();
        void Stop();
        #endregion

        #region get
        NarrationFragment CurNarrationFragment();
        #endregion

        #region is
        bool IsHave();
        #endregion
    }
    public interface IStoryMgr<out TOut>
    {
        event Callback<TOut, string> Callback_OnStart;
        event Callback<TOut, int, string> Callback_OnNext;
        event Callback<TOut, int, string> Callback_OnSubNext;
        event Callback Callback_OnEnd;
        bool Start(string group, bool pause = false, Func<bool> customStop = null);
        void Next();
        void Stop();
        bool IsWait();
        bool IsFinished();
    }
}