using System;
using System.Collections.Generic;
using UnityEngine;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.Unit
{
    // 属性配置，包括默认值,最大值，最小值，图标，转换数值，是否为正向加成,是否为百分比数值
    public class TDBaseAttrData : TDBaseData
    {
        public string Category { get; set; } = "";
        public AttrType Type { get; set; } = AttrType.Fixed;
        public AttrBuffType BuffType { get; set; } = AttrBuffType.Forward;
        public NumberType NumberType { get; set; } = NumberType.Normal;
        public float Max { get; set; } = float.MaxValue;
        public float Min { get; set; } = float.MinValue;
        public float Default { get; set; } = 0.0f;

        public string EnumName { get; set; }

        public string GetMax()
        {
            if (Max == float.MaxValue)
                return SysConst.STR_Infinite;
            return Max.ToString();
        }
        public string GetMin()
        {
            if (Min == float.MinValue)
                return SysConst.STR_Infinite;
            return Min.ToString();
        }
        public override Sprite GetIcon()
        {
            Sprite ret = null;
            if (Icon.IsInv()) ret = BaseGlobal.RsIcon.Get(SysConst.Prefix_Attr + TDID, false);
            else ret = Icon.GetIcon(false);
            if (ret == null) ret = null;
            return ret;
        }
        public override string GetName()
        {
            return Util.GetStr(EnumName + "." + TDID);
        }
        public override string GetDesc(params object[] ps)
        {
            return (EnumName + "." + TDID).GetDesc();
        }
    }

    // 这里的T必须是枚举
    public class TDAttr<T> : TDBaseConfig<TDBaseAttrData> 
        where T : Enum
    {
        public static Dictionary<Type, List<TDBaseAttrData>> AttrDataList { get; private set; } = new Dictionary<Type, List<TDBaseAttrData>>();
        public static Dictionary<Type, Dictionary<string, TDBaseAttrData>> AttrDataDic { get; private set; } = new Dictionary<Type, Dictionary<string, TDBaseAttrData>>();
        public static Dictionary<Type, HashList<string>> AttrCategories { get; private set; } = new Dictionary<Type, HashList<string>>();

        public TDAttr() : base()
        {
        }
        public TDAttr(string keyName) : base(keyName)
        {
        }
        public override void OnLuaParseEnd()
        {
            base.OnLuaParseEnd();
        }
        public override void OnAllLoadEnd2()
        {
            base.OnAllLoadEnd2();
            //添加到全局属性表
            Type type = typeof(T);
            string enumName = type.Name;
            List<TDBaseAttrData> data = new List<TDBaseAttrData>();
            Dictionary<string, TDBaseAttrData> dataDic = new Dictionary<string, TDBaseAttrData>();
            HashList<string> dataCategory = new HashList<string>();
            string[] names = Enum.GetNames(type);
            foreach (var item in names)
            {
                var val = Get(item);
                if (val == null || val.TDID.IsInv())
                {
                    CLog.Error("没有这个属性:{0}", item);
                    continue;
                }
                val.EnumName = enumName;
                data.Add(val);
                dataDic.Add(item, val);
                dataCategory.Add(val.Category);
            }
            AttrDataList.Add(type, data);
            AttrDataDic.Add(type, dataDic);
            AttrCategories.Add(type, dataCategory);
        }
    }

}