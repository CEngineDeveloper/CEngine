//------------------------------------------------------------------------------
// LogConfig.cs
// Copyright 2020 2020/7/9 
// Created by CYM on 2020/7/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using static CYM.CLog;

namespace CYM
{
    public sealed class LogConfig : ScriptableObjectConfig<LogConfig>
    {
        public bool Enable = true;
        public LogLevel Level = LogLevel.Warn;
        [HideInInspector]
        public Dictionary<string, TagInfo> Tags = new Dictionary<string, TagInfo>();

        [SerializeField]
        [HideReferenceObjectPicker]
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout,KeyLabel = "")]
        SerializableDic<string, LogItem> Items = new SerializableDic<string, LogItem>();

        public static LogItem Get(string key)
        {
            if (Ins.Items.ContainsKey(key))
            {
                Ins.Items[key].OnFetch(key);
                return Ins.Items[key];
            }
            else
            {
                Ins.Items.Add(key,new LogItem(key));
                return Get(key);
            }
        }

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
        }
        public override void OnInited()
        {
            base.OnInited();
            foreach (var item in Tags)
            {
                item.Value.Init();
            }
            Init(Enable, Level, Tags);
        }
        #endregion
    }
}