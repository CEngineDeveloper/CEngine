//------------------------------------------------------------------------------
// BaseWeatherMgr.cs
// Copyright 2019 2019/2/25 
// Created by CYM on 2019/2/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
namespace CYM.Envirom
{
    public enum SeasonType
    {
        Spring = 0,
        Summer = 1,
        Fall = 2,
        Winter = 3
    }
    [System.Serializable]
    [HideReferenceObjectPicker]
    public class SeasonData
    {
        public Sprite Icon;
        public float SunIntensity;
        public float AccumulatedSnow;
        public float WindzonePower;

        public SeasonData()
        { 
        
        }
    }
    public class BaseSeasonMgr : BaseGFlowMgr
    {
        Dictionary<SeasonType, SeasonData> Data => GameConfig.Ins.Season;
        float WindPowerAdt => GameConfig.Ins.WindPowerAdt;
        bool IsShowSnow => GameConfig.Ins.IsShowSnow;

        #region Callback
        /// <summary>
        /// 季节变化
        /// </summary>
        public event Callback<SeasonType> Callback_OnSeasonChanged;
        #endregion

        #region prop
        BaseDateTimeMgr DateTimeMgr => BaseGlobal.DateTimeMgr;
        private TweenerCore<float, float, FloatOptions> sunTween;
        private TweenerCore<float, float, FloatOptions> snowTween;
        WindZone Wind => WindZoneObj.Obj;
        public SeasonData CurData { get; private set; } = new SeasonData();
        public SeasonType SeasonType { get; private set; } = SeasonType.Spring;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedTurnbase = true;
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            RefreshByMonth(true);
        }
        public override void OnTurnbase(bool day, bool month, bool year)
        {
            base.OnTurnbase(day, month, year);
            if (month)
            {
                RefreshByMonth();
            }
        }
        #endregion

        #region get
        public string GetSeasonID()
        {
            return SeasonType.ToString();
        }

        public Sprite GetIcon()
        {
            return CurData.Icon;
        }
        public string GetName()
        {
            return SeasonType.GetName();
        }
        public string GetDesc()
        {
            return SeasonType.GetDesc();
        }
        #endregion

        #region set
        public void RefreshByMonth(bool force = false)
        {
            if (DateTimeMgr == null)
                return;
            var month = DateTimeMgr.CurDateTime.Month;
            if (month == 3 || month == 4 || month == 5)
            {
                Change(SeasonType.Spring, force);
            }
            else if (month == 6 || month == 7 || month == 8)
            {
                Change(SeasonType.Summer, force);
            }
            else if (month == 9 || month == 10 || month == 11)
            {
                Change(SeasonType.Fall, force);
            }
            else if (month == 12 || month == 1 || month == 2)
            {
                Change(SeasonType.Winter, force);
            }
            else
            {
                Change(SeasonType.Spring, force);
            }
        }
        public void Change(SeasonType type, bool isForce = false)
        {
            if (!isForce)
            {
                if (type == SeasonType)
                    return;

            }
            SeasonType = type;
            CurData = Data[type];
            if (CurData == null)
            {
                CLog.Error("没有Season数据！");
                return;
            }
            if (sunTween != null) DOTween.Kill(sunTween);
            if (snowTween != null) DOTween.Kill(snowTween);
            sunTween = DOTween.To(() => SunObj.Obj.intensity, x => SunObj.Obj.intensity = x, CurData.SunIntensity, 1.0f);
            ChangeWindPower(CurData.WindzonePower);
            if (IsShowSnow)
            {
                snowTween = DOTween.To(() => TerrainObj.Obj.materialTemplate.GetFloat("_SnowAmount"), x => TerrainObj.Obj.materialTemplate.SetFloat("_SnowAmount", x), CurData.AccumulatedSnow, 1.0f);
            }
            Callback_OnSeasonChanged?.Invoke(type);
            OnSeasonChanged(type);
        }
        public void Next()
        {
            int val = (int)SeasonType + 1;
            if (val > (int)SeasonType.Winter)
            {
                val = 0;
            }
            Change((SeasonType)val);
        }
        public void ChangeWindPower(float power)
        {
            if (Wind) Wind.windMain = power * (1 + WindPowerAdt);
        }
        #endregion

        #region Callback
        protected virtual void OnSeasonChanged(SeasonType type)
        {

        }
        #endregion
    }
}