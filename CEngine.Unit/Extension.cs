using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM.Unit
{
    public static class Extension
    {
        #region cost
        public static float GetVal<T>(this List<Cost<T>> data, T type) where T : Enum
        {
            if (data == null)
            {
                return 0;
            }
            foreach (var item in data)
            {
                if (item.RealVal == 0)
                    continue;
                if (EnumTool<T>.Int(item.Type) == EnumTool<T>.Int(type))
                    return item.RealVal;

            }
            return 0;
        }
        public static Cost<T> GetCost<T>(this List<Cost<T>> data, T type) where T : Enum
        {
            if (data == null)
            {
                return null;
            }
            foreach (var item in data)
            {
                if (item.RealVal == 0)
                    continue;
                if (EnumTool<T>.Int(item.Type) == EnumTool<T>.Int(type))
                    return item;

            }
            return null;
        }
        public static string GetDescV<T>(this List<Cost<T>> data) where T : Enum
        {
            string retStr = "";
            string costStr = "";
            int index = 0;
            foreach (var item in data)
            {
                if (item.RealVal == 0) continue;
                string temp = item.GetDesc(false, false);
                temp = SysConst.STR_Indent + temp;
                if (index < data.Count - 1) temp = temp + "\n";
                index++;
                costStr += temp;
            }
            if (costStr != "")
                retStr = BaseLangMgr.Get("AC_消耗", UIUtil.Yellow(costStr));
            return retStr;
        }
        public static string GetDesc<T>(this List<Cost<T>> data, string Separator = "", string EndSeparator = ",", bool isHaveSign = false) where T : Enum
        {
            string temp = "";
            if (data == null)
            {
                CLog.Error("data is null");
            }
            foreach (var item in data)
            {
                if (item.RealVal == 0)
                    continue;
                temp += Separator + item.GetDesc(isHaveSign, false) + EndSeparator;
            }
            return temp.TrimEnd(EndSeparator.ToCharArray());
        }
        public static List<Cost<T>> SetInputVal<T>(this List<Cost<T>> cost, float input) where T : Enum
        {
            if (cost == null) return null;
            foreach (var item in cost)
                item.SetInputVal(input);
            return cost;
        }
        public static List<Cost<T>> SetAdd<T>(this List<Cost<T>> cost, float add) where T : Enum
        {
            if (cost == null) return null;
            foreach (var item in cost)
                item.SetAdd(add);
            return cost;
        }
        public static List<Cost<T>> SetPercent<T>(this List<Cost<T>> cost, float percent) where T : Enum
        {
            if (cost == null) return null;
            foreach (var item in cost)
                item.SetPercent(percent);
            return cost;
        }
        public static List<Cost<T>> SetUnit<T>(this List<Cost<T>> cost, BaseUnit unit) where T : Enum
        {
            if (cost == null) return null;
            foreach (var item in cost)
                item.SetUnit(unit);
            return cost;
        }
        // 获得奖励的字符窜
        public static string GetDesc(this List<BaseReward> data)
        {
            if (data == null) return "";
            string finalStr = "";
            int index = 0;
            foreach (var item in data)
            {
                if (index != 0) finalStr += "\n";
                finalStr += item.GetDesc();
                index++;
            }
            return finalStr;
        }
        #endregion
    }
}