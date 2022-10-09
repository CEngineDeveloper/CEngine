using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CYM
{
    public static class ExtensionUI
    {
        #region Digital
        public static string D1(this float? f, bool isOption = false) => UIUtil.D1(f,isOption);
        public static string D2(this float? f, bool isOption = false) => UIUtil.D2(f,isOption);
        public static string DS2(this float? f) => UIUtil.DS2(f);
        public static string DC2(this float? f, bool isReverseCol = false) => UIUtil.DC2(f,isReverseCol);
        public static string DCS2(this float? f, bool isReverseCol = false) => UIUtil.DCS2(f,isReverseCol);
        public static string DS1(this float? f) => UIUtil.DS1(f);
        public static string DC1(this float? f, bool isReverseCol = false) => UIUtil.DC1(f,isReverseCol);
        public static string DCS1(this float? f, bool isReverseCol = false) => UIUtil.DCS1(f,isReverseCol);

        public static string D1(this float f, bool isOption = false) => UIUtil.D1(f, isOption);
        public static string D2(this float f, bool isOption = false) => UIUtil.D2(f, isOption);
        public static string DS2(this float f) => UIUtil.DS2(f);
        public static string DC2(this float f, bool isReverseCol = false) => UIUtil.DC2(f, isReverseCol);
        public static string DCS2(this float f, bool isReverseCol = false) => UIUtil.DCS2(f, isReverseCol);
        public static string DS1(this float f) => UIUtil.DS1(f);
        public static string DC1(this float f, bool isReverseCol = false) => UIUtil.DC1(f, isReverseCol);
        public static string DCS1(this float f, bool isReverseCol = false) => UIUtil.DCS1(f, isReverseCol);
        #endregion

        #region KMG
        public static string KMG(this float? number, KMGType type = KMGType.TenK) => UIUtil.KMG(number,type);
        public static string KMGC(this float? number, bool reverseColor = false, KMGType type = KMGType.TenK) => UIUtil.KMGC(number,reverseColor,type);
        public static string KMGCS(this float? number, bool reverseColor = false, KMGType type = KMGType.TenK) => UIUtil.KMGCS(number,reverseColor,type);

        public static string KMG(this float number, KMGType type = KMGType.TenK) => UIUtil.KMG(number, type);
        public static string KMGC(this float number, bool reverseColor = false, KMGType type = KMGType.TenK) => UIUtil.KMGC(number, reverseColor, type);
        public static string KMGCS(this float number, bool reverseColor = false, KMGType type = KMGType.TenK) => UIUtil.KMGCS(number, reverseColor, type);
        #endregion

        #region 百分比
        public static string PerS(this int? percent) => UIUtil.PerS(percent);
        public static string PerI(this float? percent, bool isHaveSignal = true) => UIUtil.PerI(percent,isHaveSignal);
        public static string PerCI(this float? percent, bool isHaveSignal = true) => UIUtil.PerCI(percent,isHaveSignal);
        public static string Per(this float? percent) => UIUtil.Per(percent);
        public static string PerS(this float? percent) => UIUtil.PerS(percent);
        public static string PerC(this float? percent, bool reverseColor = false) => UIUtil.PerC(percent,reverseColor);
        public static string PerCS(this float? percent, bool reverseColor = false) => UIUtil.PerCS(percent,reverseColor);

        public static string PerS(this int percent) => UIUtil.PerS(percent);
        public static string PerI(this float percent, bool isHaveSignal = true) => UIUtil.PerI(percent, isHaveSignal);
        public static string PerCI(this float percent, bool isHaveSignal = true) => UIUtil.PerCI(percent, isHaveSignal);
        public static string Per(this float percent) => UIUtil.Per(percent);
        public static string PerS(this float percent) => UIUtil.PerS(percent);
        public static string PerC(this float percent, bool reverseColor = false) => UIUtil.PerC(percent, reverseColor);
        public static string PerCS(this float percent, bool reverseColor = false) => UIUtil.PerCS(percent, reverseColor);
        #endregion

        #region UIColor
        //国家颜色
        public static string Nation(this string name) => UIUtil.Nation(name);
        //城市颜色
        public static string Castle(this string name) => UIUtil.Castle(name);
        //宗教
        public static string Religion(this string name) => UIUtil.Religion(name);
        //贸易
        public static string TradeRes(this string name) => UIUtil.TradeRes(name);
        //黄色
        public static string Yellow(this string name) => UIUtil.Yellow(name);
        public static string Yellow(this float number) => UIUtil.Yellow(number);
        //红色
        public static string Red(this string name) => UIUtil.Red(name);
        public static string Red(this float number) => UIUtil.Red(number);
        //绿色
        public static string Green(this string name) => UIUtil.Green(name);
        public static string Green(this float number) => UIUtil.Green(number);
        //灰色
        public static string Grey(this string name) => UIUtil.Grey(name);
        public static string Grey(this float number) => UIUtil.Grey(number);
        #endregion
    }
}
