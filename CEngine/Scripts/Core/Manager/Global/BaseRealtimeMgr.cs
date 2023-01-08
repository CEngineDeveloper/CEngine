//------------------------------------------------------------------------------
// BaseLogicTurnMgr.cs
// Copyright 2018 2018/11/10 
// Created by CYM on 2018/11/10
// Owner: CYM
// 即时制游戏的管理器
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CYM
{
    public class BaseRealtimeMgr : BaseGFlowMgr
    {
        #region private
        float curTime = 0;
        public event Callback<float> Callback_OnSpeedChange;
        public event Callback<bool> Callback_OnPauseChange;
        #endregion

        #region pause
        public bool IsPrePause { get; private set; } = true;
        public bool IsPause => IsPauseFlag || CustomPauseFlag;
        protected bool IsPauseFlag { get; private set; } = true;
        protected virtual bool CustomPauseFlag
        {
            get
            {
                if (BaseInputMgr.IsFullScreen ||
                    BaseInputMgr.IsInGuideMask ||
                    BaseInputMgr.IsDevConsoleShow)
                    return true;
                return false;
            }
        }
        #endregion

        #region prop
        public float SpeedRate { get; private set; } = 1;
        public int CurSpeedIndex { get; private set; } = 0;
        public List<float> GameSpeedData => GameConfig.Ins.GameSpeed;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedTurnbase = true;
            NeedLateUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!IsPause)
            {
                if (curTime > 1.0)
                {
                    curTime = 0.0f;
                    BaseGlobal.DateTimeMgr?.AddDay(1);
                    BaseGlobal.Ins.OnTurnbase(
                        BaseDateTimeMgr.IsDayChanged,
                        BaseDateTimeMgr.IsMonthChanged,
                        BaseDateTimeMgr.IsYearChanged);
                }
                else
                {
                    curTime += Time.deltaTime * SpeedRate;
                }
            }
        }
        public override void OnLateUpdate()
        {
            base.OnLateUpdate();
            if (IsPrePause != IsPause)
            {
                Callback_OnPauseChange?.Invoke(IsPause);
                IsPrePause = IsPause;
            }
        }
        #endregion

        #region set
        public void TogglePause()
        {
            SetPause(!IsPauseFlag);
        }
        public void SetPause(bool b)
        {
            IsPauseFlag = b;
        }
        public void SetSpeed(float speed)
        {
            SpeedRate = speed;
            Callback_OnSpeedChange?.Invoke(speed);
        }
        public void AddSpeed()
        {
            CurSpeedIndex++;
            CurSpeedIndex = Mathf.Clamp(CurSpeedIndex, 0, GameSpeedData.Count - 1);
            SpeedRate = GameSpeedData[CurSpeedIndex];
            Callback_OnSpeedChange?.Invoke(SpeedRate);
        }
        public void SubSpeed()
        {
            CurSpeedIndex--;
            CurSpeedIndex = Mathf.Clamp(CurSpeedIndex, 0, GameSpeedData.Count - 1);
            SpeedRate = GameSpeedData[CurSpeedIndex];
            Callback_OnSpeedChange?.Invoke(SpeedRate);
        }
        public void Reset()
        {
            CurSpeedIndex = 0;
            CurSpeedIndex = Mathf.Clamp(CurSpeedIndex, 0, GameSpeedData.Count - 1);
            SpeedRate = GameSpeedData[CurSpeedIndex];
            Callback_OnSpeedChange?.Invoke(SpeedRate);
            IsPauseFlag = true;
        }
        #endregion

        #region Callback
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            Reset();
        }
        #endregion
    }
}