using System;
using UnityEngine;
//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2020-7-16
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
// Desc     ：此代码由陈宜明于2020年编写,版权归陈宜明所有
// Copyright (c) 2020 陈宜明 All rights reserved.
namespace CYM
{
    /// <summary>
    /// 定时器
    /// </summary>
    [Serializable, Unobfus]
    public class Timer
    {
        public float OverTime { get; private set; } = float.MaxValue;

        float lastUpdateTime = 0.0f;
        int overCount = 0;
        float pauseTime = 0;
        public bool IsPaused { get; private set; } = false;

        public Timer()
        {

        }
        public Timer(float overTime)
        {
            this.OverTime = overTime;
        }
        /// <summary>
        /// 重置时间
        /// </summary>
        public void Restart(float? overTime = null)
        {
            Resume();
            overCount = 0;
            lastUpdateTime = Time.time;
            if (overTime != null)
                this.OverTime = overTime.Value;
            SetOverTime(this.OverTime);
        }
        /// <summary>
        /// 设置结束时间
        /// </summary>
        /// <param name="overTime"></param>
        public void SetOverTime(float overTime)
        {
            this.OverTime = overTime;
        }
        public void AddCount()
        {
            this.overCount++;
        }
        /// <summary>
        /// 当前流逝的时间
        /// </summary>
        /// <returns></returns>
        public float Elapsed()
        {
            if (IsPaused)
                return pauseTime - lastUpdateTime;
            return Time.time - lastUpdateTime;
        }
        public override string ToString()
        {
            return new TimeSpan(0, 0, (int)Elapsed()).ToString("c");
        }
        /// <summary>
        /// 检查是否结束,如果结束,自动调用Restar 
        /// </summary>
        /// <returns></returns>
        public bool CheckOver()
        {
            if (IsOver())
            {
                Restart(OverTime);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 检查是否结束,如果结束,在手动调用Restart之前,一直返回false,确保只会进入一次
        /// </summary>
        /// <returns></returns>
        public bool CheckOverOnce()
        {
            if (IsOver())
            {
                if (overCount == 0)
                {
                    overCount++;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断是否结束
        /// </summary>
        /// <returns></returns>
        public bool IsOver()
        {
            //如果为0则表示无效计时
            if (OverTime <= 0) return false;
            //时间直接流逝
            if (lastUpdateTime == float.MinValue) return true;
            return Elapsed() > OverTime;
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Pause()
        {
            pauseTime = Time.time;
            IsPaused = true;
        }
        /// <summary>
        /// 恢复时间
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
            lastUpdateTime = pauseTime = Time.time;
        }
        /// <summary>
        /// 让时间快速流逝
        /// </summary>
        public void Gone()
        {
            lastUpdateTime = float.MinValue;
        }
    }
}

