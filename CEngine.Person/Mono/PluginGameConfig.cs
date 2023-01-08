//------------------------------------------------------------------------------
// PluginGameConfig.cs
// Copyright 2022 2022/12/26 
// Created by CYM on 2022/12/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Person;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public partial class GameConfig : CustomScriptableObjectConfig<GameConfig>
    {
        #region Person
        [SerializeField, FoldoutGroup("Person"), Tooltip("是否随机年龄头像,否则一一对应年龄头像")]
        public bool IsRandChildHeadIcon = true;
        [SerializeField, FoldoutGroup("Person"), Tooltip("是否随机年龄头像,否则一一对应年龄头像")]
        public bool IsRandOldHeadIcon = true;
        [SerializeField, FoldoutGroup("Person"), DictionaryDrawerSettings(IsReadOnly = true,DisplayMode = DictionaryDisplayOptions.OneLine),HideReferenceObjectPicker]
        public RangeDic<AgeRange> AgeRangeData = new RangeDic<AgeRange>()
        {
            { AgeRange.Child,   new Range(1,16) },
            { AgeRange.Adult,   new Range(16,40) },
            { AgeRange.Middle,  new Range(40,45) },
            { AgeRange.Old,     new Range(45,80) },
        };
        [SerializeField, FoldoutGroup("Person"), DictionaryDrawerSettings(IsReadOnly = true), HideReferenceObjectPicker]
        public Dictionary<AgeRange, float> DeathProb = new Dictionary<AgeRange, float>()
        {
            { AgeRange.Child,   0.0015f },
            { AgeRange.Adult,   0.001f },
            { AgeRange.Middle,  0.0015f },
            { AgeRange.Old,     0.01f },
        };
        #endregion
    }
}