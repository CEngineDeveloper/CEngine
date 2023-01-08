using System;
using System.IO;
using UnityEngine;

namespace CYM
{
    /// <summary>
    /// 通用的日志中心
    /// 支持:
    /// 1. 一键开关日志
    /// 2. 支持调试模式(仅editor下有效)下的日志:
    ///     1).特定字符串("dev")的日志
    ///     2).一键关闭调试模式的日志
    /// </summary>
    [Serializable]
    public class LogItem
    {
        /// <summary>
        /// 日志开关
        /// </summary>
        [SerializeField]
        bool IsEnable = true;
        //是否只在编辑器模式下生效
        [SerializeField]
        bool IsDebug = true;
        [SerializeField]
        bool IsWriteFile = true;
        [SerializeField][HideInInspector]
        string Prefix;
        [SerializeField]
        Color Color = Color.white;

        [NonSerialized]
        bool IsFetched = false;
        string hexColor = "FFFFFFFF";

        bool IsCanLog
        {
            get
            {
                if (!Application.isPlaying)
                {
                    CLog.Error("LogItem必须运行的时候才能使用");
                    return false;
                }
                if (!IsEnable)
                    return false;
                if (IsDebug)
                {
                    if (Application.isEditor)
                        return true;
                }
                else
                {
                    return true;
                }
                return false;
            }
        }

        public void Log(string str)
        {
            if (IsCanLog)
            {
                if(IsWriteFile)
                {
                    LogFileWriter.WriteLog($"Log>>{Prefix}=>{str}");
                }
                CLog.Log($"<color=#{hexColor}>{Prefix}=>{str}</color>");
            }
        }

        public void Error(string str)
        {
            if (IsCanLog)
            {
                if (IsWriteFile)
                {
                    LogFileWriter.WriteLog($"Error>>{Prefix}=>{str}");
                }
                CLog.Error($"{Prefix}=>{str}");
            }
        }

        public void OnFetch(string key)
        {
            if (!IsFetched)
            {
                hexColor = ColorUtility.ToHtmlStringRGBA(Color);
                Prefix = key;
                IsFetched = true;
            }
        }

        /// <param name="prefix">模块日志自定义前缀</param>
        /// <param name="isEnableLog">是否开启日志</param>
        /// <param name="isEnableDebugLog">是否开启Debug日志,true: 为日志模块自动添加dev前缀 且:仅编辑器下有效</param>
        public LogItem(string key, bool isEnableLog = true, bool isEnableDebugLog = false)
        {
            IsEnable = isEnableLog;
            IsDebug = isEnableDebugLog;
            OnFetch(key);
        }
        //public LogItem()
        //{
        //    OnFetch("None");
        //}


    }
}
