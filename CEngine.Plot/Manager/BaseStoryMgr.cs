//------------------------------------------------------------------------------
// BaseNextMgr.cs
// Copyright 2021 2021/3/20 
// Created by CYM on 2021/3/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
/// <summary>
/// 简单的故事,对话管理器
/// </summary>
namespace CYM.Plot
{
    public class BaseStoryMgr<TData> : BaseGFlowMgr , IStoryMgr<TData>
        where TData:TDBaseStoryData
    {
        #region prop
        ITDConfig ITDConfig;
        List<TData> Group;
        int Index = -1;
        int SubIndex = -1;
        string lastID = "";
        //是否强制暂停，玩家不可以通过手动点击，进行下一段对话
        bool isForcePause = false;
        //用户自定义结束对话的回调
        Func<bool> IsCustomStop = null;
        public bool IsHave => Index != -1;
        #endregion

        #region Callback
        public event Callback<TData, string> Callback_OnStart;
        public event Callback<TData, int, string> Callback_OnNext;
        public event Callback<TData, int, string> Callback_OnSubNext;
        public event Callback Callback_OnEnd;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (IsCustomStop != null)
            {
                if (IsCustomStop())
                {
                    Stop();
                    IsCustomStop = null;
                }
            }
        }
        #endregion

        #region is
        public bool IsWait() => !IsFinished();
        public bool IsFinished()
        {
            if (Index == -1)
                return true;
            if (Group == null)
                return true;
            return Index >= Group.Count;
        }
        #endregion

        #region set
        public bool Start(string group, bool pause = false, Func<bool> customStop = null)
        {
            //有新的对话就强制重新开始
            if (lastID != group)
            {
                Index = -1;
                SubIndex = -1;
            }
            if (Index == -1)
            {
                if (lastID == group)
                    return false;
                lastID = group;
                Index = 0;
                SubIndex = 1;
                isForcePause = pause;
                IsCustomStop = customStop;
                Group = ITDConfig.GetRawGroup(group) as List<TData>;
                if (Index >= Group.Count)
                {
                    return false;
                }
                Group[Index].TryGetTalk(SubIndex, out string subName);
                Callback_OnStart?.Invoke(Group[Index], subName);
                return true;
            }
            else
            {
                return IsWait();
            }
        }
        public void Next()
        {
            if (isForcePause)
                return;
            SubIndex++;
            if (Group[Index].TryGetTalk(SubIndex, out string subName))
            {
                Callback_OnSubNext?.Invoke(Group[Index], Index, subName);
            }
            else
            {
                Index++;
                SubIndex = 1;
                if (Index >= Group.Count)
                {
                    Callback_OnEnd?.Invoke();
                    Index = -1;
                    return;
                }
                Group[Index].TryGetTalk(SubIndex, out string desc);
                Callback_OnNext?.Invoke(Group[Index], Index, desc);
            }
        }
        public void Stop()
        {
            Callback_OnEnd?.Invoke();
            Index = -1;
        }
        #endregion
    }
}