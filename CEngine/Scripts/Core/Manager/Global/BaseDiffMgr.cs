using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    /// <summary>
    /// 游戏内设置属性
    /// </summary>
    [Serializable]
    public class DBBaseGameDiff : DBBase
    {
        public GameDiffType GameDifficultyType = GameDiffType.Simple;
        //游戏内GM Mode
        public bool IsGMMod = false;
        public bool IsAnalytics = false;
        public bool IsHavePlot = true;
        public bool IsAIHosting = false;
    }
    public class BaseDiffMgr<T> : BaseGFlowMgr, IDiffMgr<T>, IDBConverMgr<T> 
        where T : DBBaseGameDiff, new()
    {
        #region prop
        //外部GM Mode 输入
        public bool IsGMMod { get; protected set; } = false;
        public T Setting { get; protected set; } = new T();
        #endregion

        #region Set
        /// <summary>
        /// 设置游戏难度
        /// </summary>
        /// <param name="type"></param>
        public void SetDiffType(GameDiffType type)
        {
            Setting.GameDifficultyType = type;
        }
        /// <summary>
        /// 设置游戏GM模式
        /// </summary>
        public void SetGMMod(bool b)
        {
            IsGMMod = b;
            Setting.IsGMMod = b;
        }
        /// <summary>
        /// 设置数据分析
        /// </summary>
        /// <param name="b"></param>
        public void SetAnalytics(bool b)
        {
            Setting.IsAnalytics = b;
        }
        /// <summary>
        /// 设置剧情模式
        /// </summary>
        /// <param name="b"></param>
        public void SetHavePlot(bool b)
        {
            Setting.IsHavePlot = b;
        }
        /// <summary>
        /// 设置AI托管
        /// </summary>
        /// <param name="b"></param>
        public void SetAIHosting(bool b)
        {
            Setting.IsAIHosting = b;
        }
        #endregion

        #region get
        public GameDiffType GetDiffType()
        {
            return Setting.GameDifficultyType;
        }
        #endregion

        #region is 
        /// <summary>
        /// 是否允许数据统计
        /// </summary>
        /// <returns></returns>
        public bool IsAnalytics()
        {
            return Setting.IsAnalytics && !Application.isEditor;
        }
        /// <summary>
        /// 是否为GMmod
        /// </summary>
        /// <returns></returns>
        public bool IsGMMode()
        {
            if (BuildConfig.Ins.IsTrial)
                return false;
            return
                BuildConfig.Ins.IsDevelop ||
                Application.isEditor ||
                Setting.IsGMMod || 
                IsGMMod;
        }
        /// <summary>
        /// 是否设置了GM模式
        /// </summary>
        /// <returns></returns>
        public bool IsSettedGMMod()
        {
            return Setting.IsGMMod;
        }
        /// <summary>
        /// 是有有剧情
        /// </summary>
        /// <returns></returns>
        public bool IsHavePlot()
        {
            return Setting.IsHavePlot;
        }
        /// <summary>
        /// AI托管
        /// </summary>
        /// <returns></returns>
        public bool IsAIHosting()
        {
            if (SysConsole.Ins.IsOnlyPlayerAI)
                return true;
            return Setting.IsAIHosting;
        }
        #endregion

        #region is target
        //是否进入AI托管(进入AI托管,不一定AI会操作)
        public bool IsAIHosting(BaseUnit unit)
        {
            return (IsAIHosting() && unit.IsPlayerCtrl()) || unit.IsAI();
        }
        //是否AI操作,更加细化
        public bool IsAIProcess(BaseUnit unit)
        {
            if (SysConsole.Ins.IsOnlyPlayerAI)
            {
                if (unit.IsPlayer()) return true;
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region DB
        public void LoadDBData(ref T data)
        {
            //如果是新存档,则不加载
            if (IsNewGame)
                return;
            Setting = data;
        }

        public void SaveDBData(ref T data)
        {
            data = Setting;
        }
        #endregion
    }
}