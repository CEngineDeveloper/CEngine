using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Unit
{
    #region attr
    // 属性加成
    public class AttrAdditon<T> : IBase, ICloneable, IAttrAdditon where T : Enum
    {
        public AttrAdditon(T _type, AttrOpType _addType, float _val)
        {
            Type = _type;
            AddType = _addType;
            Val = _val;
        }

        #region config
        public AttrAdditon() { }
        public long ID { get; set; }
        public string TDID { get; set; }
        public T Type { get; set; }
        public AttrOpType AddType { get; set; } = AttrOpType.DirectAdd;
        public AttrFactionType FactionType { get; set; } = AttrFactionType.DirectAdd;
        public float Val { get; set; }
        // 每个step产生的faction因子
        public float Faction { get; set; } = 1.0f;
        // 动态设置
        public float Step { get; set; } = 1;
        //外部输入值偏移
        public float InputValStart { get; set; } = 0;
        // 真实的数值
        public float RealVal => AnticipationVal(InputVal);
        public float InputVal { get; private set; } = 0;
        public bool Valid { get; private set; } = true;
        public float Min { get; private set; } = float.MinValue;
        public float Max { get; private set; } = float.MaxValue;
        #endregion

        #region set
        public float AnticipationVal(float? inputVal)
        {
            if (inputVal == null) inputVal = InputVal;
            inputVal = inputVal - InputValStart;
            var faction = inputVal.Value / Step * Faction;
            float ret = 0;
            if (FactionType == AttrFactionType.PercentAdd) ret = Val * (1 + faction);
            else if (FactionType == AttrFactionType.DirectAdd) ret = Val + (faction);
            else if (FactionType == AttrFactionType.DirectMul) ret = Val * faction;
            else if (FactionType == AttrFactionType.None) ret = Val;
            else throw new Exception("错误的FactionType:" + FactionType.ToString());
            return Mathf.Clamp(ret, Min, Max);
        }
        public void SetInput(float val) => InputVal = val;
        public void SetValid(bool val) => Valid = val;
        #endregion

        #region is
        public bool IsPercentSign() => AddType == AttrOpType.PercentAdd || AddType == AttrOpType.Percent;
        #endregion

        #region get
        // 获得加成的描述
        // anticipationFaction:用户自定义的因子
        public string GetDesc(bool isIgnoreName = false, bool isColor = true, float? inputVal = null)
        {
            return BaseAttrMgr<T>.GetAttrStr(
                Type,
                AnticipationVal(inputVal),
                IsPercentSign(),
                isIgnoreName,
                isColor);
        }
        // layer:用户自定义层数
        public string GetDescByLayer(bool isIgnoreName = false, bool isColor = true, int layer = 1)
        {
            return BaseAttrMgr<T>.GetAttrStr(
                Type,
                RealVal * layer,
                IsPercentSign(),
                isIgnoreName,
                isColor);
        }
        public Sprite GetIcon() => BaseAttrMgr<T>.GetAttrIcon(Type);
        public object Clone() => base.MemberwiseClone();
        #endregion

    }
    // 属性转换结构体
    public class AttrConvert<T> : ICloneable where T : Enum
    {
        public float IgnoreMax { get; set; } = float.MaxValue;  //最大忽略
        public float IgnoreMin { get; set; } = float.MinValue;  //最小忽略
        public float Max { get; set; } = float.MaxValue;        //最大转换
        public float Min { get; set; } = float.MinValue;        //最小转换
        public float Step { get; set; } = 1;                    //转换阶梯
        public float Faction { get; set; } = 1;                 //转换因子
        public float Offset { get; set; } = 0;                  //原数值修正
        public bool IsReverse { get; set; } = false;            //是否反转
        public T From { get; set; }                             //来源属性类型
        public T To { get; set; }                               //转换的属性类型
        public AttrFactionType FactionType { get; set; } = AttrFactionType.DirectAdd; //属性影响因子
        public List<float> Slot { get; set; } = new List<float>(); //固定槽位
        public bool IsUseSlot => Slot != null && Slot.Count > 0; //是否使用槽位
        public object Clone() => base.MemberwiseClone();
    }
    #endregion

    #region data
    [Serializable]
    public class Cost<T> : ICloneable, IUpFactionData where T : Enum
    {
        #region construct
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        public Cost(T type, float v) { Type = type; Val = v; }
        public Cost() { }
        public BaseUnit BaseUnit { get; private set; }
        #endregion

        #region config
        //成长因子
        public UpFactionType FactionType { get; set; } = UpFactionType.PercentAdd;
        //属性类型
        public T Type { get; set; }
        //基础值
        public float Val { get; set; }
        //固定附加值
        public float Add { get; set; } = 0.0f;
        //百分比增加
        public float Percent { get; set; } = 0.0f;
        // 修正因子
        public float Faction { get; set; } = 0.0f;
        //是否用于条件判断
        public bool IsCondition { get; set; } = true;
        //最终输出的最小值
        public float Min { get; set; } = float.MinValue;
        //最终输出的最大值
        public float Max { get; set; } = float.MaxValue;
        //外部输入值偏移
        public float InputValStart { get; set; } = 0;
        //输入因子
        public float InputVal { get; private set; } = 0;
        //引用的输入值(优先级高于InputVal)
        public string RefInputVal { get; set; } = SysConst.STR_Inv;
        #endregion

        #region set
        public Cost<T> SetUnit(BaseUnit unit)
        {
            BaseUnit = unit;
            return this;
        }
        public Cost<T> SetInputVal(float val)
        {
            InputVal = val;
            return this;
        }
        public Cost<T> SetAdd(float val)
        {
            Add = val;
            return this;
        }
        public Cost<T> SetPercent(float val)
        {
            Percent = val;
            return this;
        }
        public float AnticipationVal(float? inputVal) => Util.GetUpFactionVal(this, inputVal) * (1 + Percent);
        public object Clone() => null;
        #endregion

        #region get
        public float GetRealInputVal()
        {
            if (RefInputVal.IsInv()) return InputVal;
            return BaseGlobal.RefMgr.GetUnitFloat(RefInputVal, BaseUnit);
        }
        public float RealVal => Mathf.Clamp(AnticipationVal(GetRealInputVal()), Min, Max);
        public string GetName() => BaseAttrMgr<T>.GetAttrName(Type);
        public Sprite GetIcon() => BaseAttrMgr<T>.GetAttrIcon(Type);
        public string GetDesc(bool isHaveSign = false, bool isHaveColor = false, bool isHeveAttrName = true) => BaseAttrMgr<T>.GetAttrCostStr(Type, RealVal, isHaveSign, isHaveColor, isHeveAttrName);
        public string GetJumpStr(bool isReserve)
        {
            if (isReserve) return "+" + Type.GetName() + UIUtil.D2(RealVal);
            else return "-" + Type.GetName() + UIUtil.D2(RealVal);
        }
        #endregion

        #region is
        public bool IsHaveVal() => RealVal != 0.0f;
        #endregion
    }
    #endregion
}
