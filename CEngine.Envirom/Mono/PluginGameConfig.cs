//------------------------------------------------------------------------------
// PluginGameConfig.cs
// Copyright 2023 2023/1/21 
// Created by CYM on 2023/1/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using CYM.Envirom;

namespace CYM
{
    public partial class GameConfig : CustomScriptableObjectConfig<GameConfig>
    {
        #region SLG Weather
        [SerializeField, FoldoutGroup("SLG Weather")]
        public int StartCount = 30;
        [SerializeField, FoldoutGroup("SLG Weather")]
        public int TotalCount = 40;
        [SerializeField, FoldoutGroup("SLG Weather")]
        public int CellCount = 30;
        [SerializeField, FoldoutGroup("SLG Weather")]
        public int Squar = 1024;
        [SerializeField, FoldoutGroup("SLG Weather"), HideReferenceObjectPicker]
        public List<SLGWeatherConfigData> SLGWeather = new List<SLGWeatherConfigData>();
        #endregion

        #region SLG Season
        [SerializeField, FoldoutGroup("SLG Season")]
        public bool IsShowSnow = false;
        [SerializeField, FoldoutGroup("SLG Season")]
        public float WindPowerAdt = 0.8f;
        [SerializeField, FoldoutGroup("SLG Season"), DictionaryDrawerSettings(IsReadOnly = true), HideReferenceObjectPicker]
        public Dictionary<SeasonType, SeasonData> Season = new Dictionary<SeasonType, SeasonData>()
        {
            {
                SeasonType.Spring,new SeasonData
                {
                        SunIntensity = 0.85f,
                        AccumulatedSnow = 0.0f,
                        WindzonePower = 0.2f,
                }
            },
            {
                SeasonType.Summer,new SeasonData
                {
                        SunIntensity = 0.9f,
                        AccumulatedSnow = 0.0f,
                        WindzonePower = 0.25f,
                }
            },
            {
                SeasonType.Fall,new SeasonData
                {
                        SunIntensity = 0.75f,
                        AccumulatedSnow = 0.0f,
                        WindzonePower = 0.25f,
                }
            },
            {
                SeasonType.Winter,new SeasonData
                {
                        SunIntensity = 0.7f,
                        AccumulatedSnow = 0.2f,
                        WindzonePower = 0.29f,
                }
            }
        };
        #endregion
    }
}