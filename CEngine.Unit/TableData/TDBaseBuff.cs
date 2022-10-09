
using System;
using System.Collections.Generic;
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
    [Serializable]
    public class TDBaseBuffData<TType> : TDBaseData , ITDBuffData
        where TType : Enum
    {
        #region def
        public const float MAX_TIME = float.MaxValue;
        public const float NO_INTERVAL = 0.0f;
        public const float FOREVER = 0.0f;
        public const int MAX_LAYER = 100;
        public const float TIME_STEP = 1.0f;
        #endregion

        #region config
        public List<AttrAdditon<TType>> IntervalAttr { get; set; } = new List<AttrAdditon<TType>>();
        public List<AttrAdditon<TType>> Attr { get; set; } = new List<AttrAdditon<TType>>();
        public List<AttrConvert<TType>> Convert { get; set; } = new List<AttrConvert<TType>>();
        public int MaxLayer { get; set; } = 1;
        public bool IsHide { get; set; } = false;
        public float MaxTime { get; set; } = 0;
        public int IntervalTime { get; set; } = 0;
        public ImmuneGainType Immune { get; set; } = ImmuneGainType.Positive;
        public float InputValStart { get; set; } = 0;
        //buff 合并类型
        public BuffMergeType MergeType { get; set; } = BuffMergeType.None;
        //自定义buff组的id,优先级高于BuffMergeType
        public string BuffGroupID { get; set; }//有配置Buff组的会叠加,没有配置Buff组的会合并
        public List<string> Performs { get; set; } = new List<string>();
        #endregion

        #region life
        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            if (MergeType != BuffMergeType.None && !BuffGroupID.IsInv())
            {
                CLog.Error("Buff配置错误!MergeType 和 BuffGroupID不能同时使用");
            }
            else if (MergeType == BuffMergeType.Group)
            {
                BuffGroupID = TDID;
            }
        }
        #endregion

        #region runtime
        protected List<BasePerform> R_Performs { get; private set; } = null;
        protected BaseUnit R_Caster { get; private set; } = null;
        protected List<AttrAdditon<TType>> R_Attr { get; private set; } = null;
        protected List<AttrConvert<TType>> R_Convert { get; private set; } = null;
        public int MergeLayer { get; private set; }
        public float CurTime { get; private set; } = 0;
        public float Input { get; private set; } = 0;
        public float CurInterval { get; private set; } = 0;
        public bool Valid { get; private set; } = true;
        public float PercentCD
        {
            get
            {
                if (!IsHaveRTMaxTime)
                    return 0.0f;
                return CurTime / RTMaxTime;
            }
        }
        public string PercentCDStr
        {
            get
            {
                if (!IsHaveRTMaxTime)
                    return BaseLangMgr.Get("Text_永久");
                return CurTime.ToString() + "/" + RTMaxTime.ToString();
            }
        }
        //运行时的MaxTime,RunTime MaxTime,这个值不是配置值,是可以实时变化的
        public float RTMaxTime { get; private set; } = 0;
        #endregion

        #region func
        // obj1:来源对象
        // obj2:来源技能
        public override void OnBeAdded(BaseCoreMono mono, params object[] obj)
        {
            base.OnBeAdded(mono, obj);
            R_Caster = GetAddedObjData<BaseUnit>(0);
            R_Attr = AttrMgr.Add(Attr);
            R_Convert = AttrMgr.Add(Convert);
            R_Performs = new List<BasePerform>();
            CurTime = 0;
            RTMaxTime = MaxTime;
            Valid = true;
            if (PerformMgr != null)
            {
                foreach (var item in Performs)
                    R_Performs.Add(PerformMgr.Spawn(item));
            }
        }
        public override void OnBeRemoved()
        {
            base.OnBeRemoved();
            AttrMgr.Remove(R_Attr);
            AttrMgr.Remove(R_Convert);
            if (PerformMgr != null)
            {
                foreach (var item in R_Performs)
                    PerformMgr.Despawn(item);
            }
        }
        public virtual void OnMerge(ITDBuffData newBuff, params object[] obj)
        {
            MergeLayer++;

            if (MergeType == BuffMergeType.None)
                CurTime = 0;
            else if (MergeType == BuffMergeType.CD)
                RTMaxTime += MaxTime;
            else
                CLog.Error("其他的合并类型不应该出现在这里,错误的类型:{0}", MergeType.GetName());

        }
        protected virtual void OnInterval()
        {
            if (IntervalAttr != null)
            {
                AttrMgr.Add(IntervalAttr);
            }
        }
        public override void ManualUpdate()
        {
            base.ManualUpdate();
            if (RTMaxTime != FOREVER)
                CurTime += TIME_STEP;
            if (IntervalTime != NO_INTERVAL)
            {
                CurInterval += TIME_STEP;
                if (CurInterval >= IntervalTime)
                {
                    OnInterval();
                    CurInterval = 0;
                }
            }
        }
        protected override void DeepClone(object sourceObj)
        {
            TDBaseBuffData<TType> sourceBuff = sourceObj as TDBaseBuffData<TType>;
            IntervalAttr = new List<AttrAdditon<TType>>();
            Attr = new List<AttrAdditon<TType>>();
            Convert = new List<AttrConvert<TType>>();
            foreach (var item in sourceBuff.IntervalAttr)
                IntervalAttr.Add(item.Clone() as AttrAdditon<TType>);
            foreach (var item in sourceBuff.Attr)
                Attr.Add(item.Clone() as AttrAdditon<TType>);
            foreach (var item in sourceBuff.Convert)
                Convert.Add(item.Clone() as AttrConvert<TType>);
        }
        #endregion

        #region is
        // buff时间是否结束
        public bool IsTimeOver
        {
            get
            {
                if (RTMaxTime == FOREVER)
                    return false;
                return CurTime >= RTMaxTime;
            }
        }
        // 是否永久
        public bool IsForever
        {
            get
            {
                if (RTMaxTime == FOREVER)
                    return true;
                return false;
            }
        }
        public bool IsHaveRTMaxTime => RTMaxTime > 0;
        public bool IsHaveMaxTime => MaxTime > 0;
        #endregion

        #region set
        // 设置属性修改因子
        public virtual void SetInput(float input)
        {
            //使用InputVal必须设置为DeepCopy
            if (input != 0 && CloneType != CloneType.Deep)
            {
                CLog.Error("使用InputVal必须设置为DeepCopy");
                return;
            }

            Input = input;
            var realInputVal = Input - InputValStart;
            if (R_Attr == null)
            {
                CLog.Error("错误!SetInput,R_Attr == null,{0}", TDID);
                return;
            }
            foreach (var item in R_Attr)
            {
                item.SetInput(realInputVal);
            }
            AttrMgr.SetDirty();
        }
        public virtual void SetValid(bool b)
        {           
            //使用InputVal必须设置为DeepCopy
            if (b==false && CloneType != CloneType.Deep)
            {
                CLog.Error("使用SetValid必须设置为DeepCopy");
                return;
            }
            Valid = b;
            if (R_Attr == null)
            {
                CLog.Error("错误!SetValid,R_Attr == null,{0}", TDID);
                return;
            }
            foreach (var item in R_Attr)
            {
                item.SetValid(b);
            }
            AttrMgr.SetDirty();
        }
        public virtual void SetCD(float cd) => CurTime = cd;
        public virtual void SetRTMaxTime(float maxTime)
        {
            if (MaxTime == FOREVER)
            {
                CLog.Error("SetMaxTime,配置为永久时间的Buff无法调用SetMaxTime");
                return;
            }
            RTMaxTime = maxTime;
        }
        // 强制结束Buff
        protected void ForceOverBuffTime() => CurTime = RTMaxTime;
        #endregion

        #region desc
        // 获取buff的加成描述列表 翻译
        public virtual List<string> GetAdtStrs(float? inputVal = null)
        {
            List<string> data = new List<string>();
            for (int i = 0; i < Attr.Count; ++i)
                data.Add(Attr[i].GetDesc(false, true, inputVal));
            return data;
        }
        // 通过Layer 获得加成列表
        public virtual List<string> GetAdtStrsByLayer(int layer = 1)
        {
            List<string> data = new List<string>();
            for (int i = 0; i < Attr.Count; ++i)
                data.Add(Attr[i].GetDescByLayer(false, true, layer));
            return data;
        }
        // 获得buff的加成描述字符串组合
        public string GetAdtDesc(bool isNewLine = true, string splite = SysConst.STR_Indent)
        {
            string addition = "";
            List<string> temp = GetAdtStrs();
            for (int i = 0; i < temp.Count; ++i)
            {
                addition += splite + temp[i];
                if (isNewLine)
                {
                    if (i < temp.Count - 1)
                        addition += "\n";
                }
            }
            return addition;
        }
        public string GetAdtDescH()
        {
            string splite = ",";
            string addition = "";
            List<string> temp = GetAdtStrs();
            for (int i = 0; i < temp.Count; ++i)
            {
                addition += temp[i];
                if (i < temp.Count - 1)
                    addition += splite;
            }
            return addition;
        }
        public string GetAdtDescByLayer(int layer = 1, bool isNewLine = false, string splite = SysConst.STR_Indent)
        {
            string addition = "";
            List<string> temp = GetAdtStrsByLayer(layer);
            for (int i = 0; i < temp.Count; ++i)
            {
                addition += splite + temp[i];
                if (isNewLine)
                {
                    if (i < temp.Count - 1)
                        addition += "\n";
                }
            }
            return addition;
        }
        public string GetTipInfo()
        {
            string ret = "";
            ret = string.Format("{0} {1}", GetName(), !IsHaveRTMaxTime ? "" : CurTime.ToString() + "/" + RTMaxTime)+"\n";
            ret += GetAdtDesc();
            return ret;
        }
        public string GetInfo()
        {
            string ret = "";
            ret = string.Format("{0}", GetName())+"\n";
            ret += GetAdtDesc();
            return ret;
        }
        #endregion

        #region must override
        protected virtual BasePerformMgr PerformMgr
        {
            get
            {
                if (SelfBaseUnit.PerformMgr == null)
                {
                    CLog.Error($"TDBaseBuff 错误，单位缺失：{nameof(SelfBaseUnit.PerformMgr)}组建");
                    return null;
                }
                return SelfBaseUnit.PerformMgr;
            }
        }
        // 属性管理器
        protected virtual BaseAttrMgr<TType> AttrMgr
        {
            get
            {
                if (SelfBaseUnit.AttrMgr == null)
                {
                    CLog.Error($"TDBaseBuff 错误，单位缺失：{nameof(SelfBaseUnit.AttrMgr)}组建");
                    return null;
                }
                return SelfBaseUnit.AttrMgr as BaseAttrMgr<TType>;
            }
        }
        #endregion
    }

}


