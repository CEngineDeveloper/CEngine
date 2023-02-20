//------------------------------------------------------------------------------
// BaseDateMgr.cs
// Copyright 2018 2018/11/10 
// Created by CYM on 2018/11/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace CYM
{
    public class BaseDateTimeMgr : BaseGFlowMgr
    {
        #region prop
        const string space = SysConst.UCD_NoBreakingSpace;
        readonly string DateStrFormat = "{0}"+ space + "{1}.{2}.{3}";
        readonly string DateStrFormat_Year = "{0}"+ space + "{1}";
        readonly string DateStrFormat_Month = "{0}"+ space + "{1}{3}"+ space + "{2}{4}";
        readonly string DateStrFormat_Day = "{0}" + space + "{1}{2}" + space + "{3}{4}" + space + "{5}{6}";
        public GameDateTimeType CurDateTimeAgeType { get; protected set; } = GameDateTimeType.BC;
        public GameDateTimeType StartDateTimeAgeType { get; protected set; } = GameDateTimeType.BC;
        public DateTime CurDateTime { get; protected set; } = new DateTime(1, 3, 1);
        public DateTime StartDateTime { get; protected set; } = new DateTime(1, 1, 1);
        int PreMonth { get; set; }
        int PreYear { get; set; }
        int PreDay { get; set; }
        public static bool IsDayChanged { get; private set; }
        public static bool IsMonthChanged { get; private set; }
        public static bool IsYearChanged { get; private set; }
        #endregion

        #region life
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            var datetime = GameConfig.Ins.StartDateTime;
            StartDateTime = new DateTime(datetime.Year, datetime.Month, datetime.Day);
            StartDateTimeAgeType = datetime.Type;
        }
        #endregion

        #region Callback
        public event Callback Callback_OnDayChanged;
        public event Callback Callback_OnMonthChanged;
        public event Callback Callback_OnYearChanged;
        #endregion

        #region set
        public void AddDay(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddDays(val);
            CheckChange();
        }
        public void AddMonth(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddMonths(val);
            CheckChange();
        }
        public void AddYear(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddYears(val);
            CheckChange();
        }
        #endregion

        #region get
        string GetTimeAgeType()
        {
            string dateTypeStr = "AD";
            if (CurDateTimeAgeType == GameDateTimeType.BC)
                dateTypeStr = "BC";
            return dateTypeStr;
        }
        public int GetYear()
        {
            int curYear = 0;
            if (CurDateTimeAgeType == GameDateTimeType.BC)
                curYear = (StartDateTime.Year - CurDateTime.Year + 1);
            else if (CurDateTimeAgeType == GameDateTimeType.AD)
            {
                if (StartDateTimeAgeType == GameDateTimeType.AD)
                    curYear = (StartDateTime.Year + CurDateTime.Year - 1);
                else
                    curYear = (CurDateTime.Year - StartDateTime.Year - 1);
            }
            curYear = Mathf.Clamp(curYear, 0, int.MaxValue);
            return curYear;
        }
        public string GetCurDateStr(bool isHaveAgeType = true)
        { 
            if(isHaveAgeType)
                return string.Format(DateStrFormat, GetTimeAgeType(), GetYear(), CurDateTime.Month, CurDateTime.Day);
            return string.Format("{0}.{1}.{2}", GetYear(), CurDateTime.Month, CurDateTime.Day);
        }
        public string GetCurYear()=> string.Format(DateStrFormat_Year, GetTimeAgeType(), GetYear());
        public string GetCurYearMonth()
        {
            return string.Format(
                DateStrFormat_Month,
                GetTimeAgeType(),
                GetYear(),
                CurDateTime.Month,
                BaseLangMgr.Get("Unit_年"),
                BaseLangMgr.Get("Unit_月"));
        }
        public string GetCurYMD()
        {
            return string.Format(
                DateStrFormat_Day,
                GetTimeAgeType(),
                GetYear(),
                BaseLangMgr.Get("Unit_年"),
                CurDateTime.Month,          
                BaseLangMgr.Get("Unit_月"),
                CurDateTime.Day,
                BaseLangMgr.Get("Unit_日")
                );
        }
        public string GetTYMD()
        {
            return string.Format(
                "{0}"+space+ "{1}{2}" + space + "{3}{4}" + space + "{5}{6}",
                GetTimeAgeType(),
                GetYear(),
                BaseLangMgr.Get("Unit_年"),
                CurDateTime.Month,
                BaseLangMgr.Get("Unit_月"),
                CurDateTime.Day,
                BaseLangMgr.Get("Unit_日")
                );
        }
        #endregion

        #region utile
        void RecodePreDate()
        {
            PreMonth = CurDateTime.Month;
            PreYear = CurDateTime.Year;
            PreDay = CurDateTime.Day;
        }
        void CheckChange()
        {
            IsDayChanged = CurDateTime.Day != PreDay;
            IsMonthChanged = CurDateTime.Month != PreMonth;
            IsYearChanged = CurDateTime.Year != PreYear;
            if (IsDayChanged) Callback_OnDayChanged?.Invoke();
            if (IsMonthChanged) Callback_OnMonthChanged?.Invoke();
            if (IsYearChanged)
            {
                UpdateDateTimeType();
                Callback_OnYearChanged?.Invoke();
            }
        }
        void UpdateDateTimeType()
        {
            if (CurDateTimeAgeType == GameDateTimeType.BC &&
                (StartDateTime.Year - CurDateTime.Year) <= 0)
            {
                CurDateTimeAgeType = GameDateTimeType.AD;
            }
        }
        #endregion

        #region DB
        public override void OnRead1(DBBaseGame data)
        {
            base.OnRead1(data);
            CurDateTime = data.CurDateTime;
            CurDateTimeAgeType = data.CurDateTimeAgeType;
        }
        public override void OnWrite(DBBaseGame data)
        {
            base.OnWrite(data);
            data.CurDateTime = CurDateTime;
            data.CurDateTimeAgeType = CurDateTimeAgeType;
        }
        #endregion
    }
}