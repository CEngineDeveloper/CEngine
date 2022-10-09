using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Unit
{
    public class AttrGroup<T>
    {
        public T Cur;
        public T Max;
        public T Cha;
    }
    // 属性管理器基类,T必须为属性枚举
    public class BaseAttrMgr<T> : BaseMgr, IAttrMgr 
        where T : Enum
    {
        #region Callback val
        public Callback<HashSet<T>> Callback_OnAttrChange { get; set; }
        #endregion

        #region prop
        protected List<TDBaseAttrData> attrDataList = new List<TDBaseAttrData>();
        protected Dictionary<string, TDBaseAttrData> attrDataDic = new Dictionary<string, TDBaseAttrData>();
        protected List<AttrConvert<T>> _attrConvertData = new List<AttrConvert<T>>();
        protected readonly List<AttrAdditon<T>> _data = new List<AttrAdditon<T>>();
        protected readonly List<AttrAdditon<T>> _percentData = new List<AttrAdditon<T>>();
        protected float[] _baseDataPool;
        protected float[] _curDataPool;
        protected HashSet<T> lastChangedType = new HashSet<T>();
        protected Func<float>[] _customAttrVal;
        protected AttrGroup<T>[] _attrGroup;
        protected static Dictionary<T, Dictionary<string, string>> attrAlias = new Dictionary<T, Dictionary<string, string>>();
        protected static string curAliasKey;
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.All;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            var tempArray = Enum.GetValues(typeof(T));
            _curDataPool = new float[tempArray.Length];
            _baseDataPool = new float[tempArray.Length];
            _customAttrVal = new Func<float>[tempArray.Length];
            _attrGroup = new AttrGroup<T>[tempArray.Length];
            attrDataList = GetAttrDataList();
            attrDataDic = GetAttrDataDic();
        }
        #endregion

        #region is
        // 是否有消耗
        public bool IsHaveCost(Cost<T> data)
        {
            if (data == null)
                return false;
            return GetVal(data.Type) >= data.RealVal;
        }
        // 是否有消耗
        public bool IsHaveCost(List<Cost<T>> data)
        {
            if (data == null)
                return false;
            foreach (var item in data)
            {
                if (GetVal(item.Type) < item.RealVal)
                {
                    return false;
                }
            }
            return true;
        }
        public bool IsCanCost(Cost<T> cost)
        {
            return GetVal(cost.Type) >= cost.RealVal;
        }
        #endregion

        #region alias
        //设置属性别名Key
        public static void SetAliasKey(string key)
        {
            curAliasKey = key;
        }
        /// <summary>
        /// 添加属性别名
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <param name="key"></param>
        /// <param name="nameKey"></param>
        public static void AddAlias(T type, string aliasKey, string nameKey)
        {
            if (!attrAlias.ContainsKey(type))
                attrAlias.Add(type, new Dictionary<string, string>());
            attrAlias[type].Add(aliasKey, nameKey);
        }
        #endregion

        #region set
        protected void AddGroupAttr(T cur, T max, T change)
        {
            var index = EnumTool<T>.Int(cur);
            var tempAttrData = attrDataList[index];
            if (tempAttrData.Type != AttrType.Dynamic)
            {
                CLog.Error("错误!Cur属性必须是Dynamic");
                return;
            }
            _attrGroup[index] = new AttrGroup<T> { Cur = cur, Max = max, Cha = change };
        }
        protected void AddCustomAttrVal(T type, Func<float> func)
        {
            if (func == null) return;
            var index = EnumTool<T>.Int(type);
            _customAttrVal[index] = func;
        }
        public void InitFrom(BaseAttrMgr<T> otherAttr)
        {
            for(int i=0;i<otherAttr._baseDataPool.Length;++i)
            {
                _baseDataPool[i] = otherAttr._baseDataPool[i];
            }
        }
        public List<AttrAdditon<T>> Add(List<AttrAdditon<T>> array)
        {
            if (array == null) return null;
            for (var i = 0; i < array.Count; ++i)
            {
                if (array[i] != null)
                {
                    var item = array[i];
                    var index = EnumTool<T>.Int(item.Type);
                    var tempAttrData = attrDataList[index];

                    //检查数值合适性
                    if (tempAttrData.NumberType == NumberType.Percent)
                    {
                        if (item.Val < -1 || item.Val > 1)
                            CLog.Error("错误!AddAttrAdditon,Percent,范围越界,Type:{0},Val:{1}", item.Type, item.Val);
                        else if (item.AddType == AttrOpType.Percent ||
                            item.AddType == AttrOpType.PercentAdd)
                            CLog.Error("错误!AddAttrAdditon,Percent,加成方式不对,Type:{0},Val:{1}", item.Type, item.Val);
                    }
                    else if (tempAttrData.NumberType == NumberType.Bool)
                    {
                        if (item.Val != 0 || item.Val != 1)
                            CLog.Error("错误!AddAttrAdditon,Bool,Type:{0},Val:{1}", item.Type, item.Val);
                        else if (item.AddType == AttrOpType.Percent ||
                                 item.AddType == AttrOpType.PercentAdd)
                            CLog.Error("错误!AddAttrAdditon,Bool,加成方式不对,Type:{0},Val:{1}", item.Type, item.Val);
                    }

                    //固定值,持续变化
                    if (tempAttrData.Type == AttrType.Fixed)
                    {
                        if (item.AddType == AttrOpType.Direct || item.AddType == AttrOpType.DirectAdd) _data.Add(item);
                        else _percentData.Add(item);
                    }
                    //动态值一次性变化
                    else if (tempAttrData.Type == AttrType.Dynamic)
                    {
                        if (item.AddType == AttrOpType.Direct) SetVal(item.Type, item.RealVal);
                        else if (item.AddType == AttrOpType.DirectAdd) ChangeVal(item.Type, item.RealVal);
                        else if (item.AddType == AttrOpType.Percent) SetVal(item.Type, item.RealVal * _getCurAttrVal(index));
                        else if (item.AddType == AttrOpType.PercentAdd) ChangeVal(item.Type, item.RealVal * _getCurAttrVal(index));
                    }
                }
            }
            SetDirty();
            return array;
        }
        public void Remove(List<AttrAdditon<T>> array)
        {
            if (array == null) return;
            for (var i = 0; i < array.Count; ++i)
            {
                if (array[i] != null)
                {
                    if (array[i].AddType == AttrOpType.Direct ||
                        array[i].AddType == AttrOpType.DirectAdd)
                        _data.Remove(array[i]);
                    else
                        _percentData.Remove(array[i]);
                }
            }
            SetDirty();
        }

        public List<AttrConvert<T>> Add(List<AttrConvert<T>> array)
        {
            if (array == null) return null;
            for (var i = 0; i < array.Count; ++i)
            {
                if (array[i] != null)
                {
                    _attrConvertData.Add(array[i]);
                }
            }
            SetDirty();
            return array;
        }
        public void Remove(List<AttrConvert<T>> array)
        {
            if (array == null) return;
            for (var i = 0; i < array.Count; ++i)
            {
                if (array[i] != null)
                {
                    _attrConvertData.Remove(array[i]);
                }
            }
            SetDirty();
        }
        protected virtual void AddAttrVal(T type, float val)
        {
            var index = EnumTool<T>.Int(type);
            if (attrDataList != null)
            {
                TDBaseAttrData tempAttrData = null;
                if (index >= attrDataList.Count)
                {
                    CLog.Error("SetAttrVal:" + type.ToString() + ":没有配置属性表");
                    return;
                }
                tempAttrData = attrDataList[index];
                if (tempAttrData == null)
                {
                    _curDataPool[index] += val;
                }
                else
                {
                    _curDataPool[index] = Mathf.Clamp(_curDataPool[index] + val, tempAttrData.Min, tempAttrData.Max);
                }
            }
            else
            {
                _curDataPool[index] += val;
            }
        }
        /// <summary>
        /// 手动改变属性值
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <param name="val">改变的值</param>
        /// <param name="minVal">最小值</param>
        /// <param name="maxVal">最大值</param>
        /// <param name="onlyDynamicVal">只能改变Dynamic属性,如果改变了Fixed属性则会提示错误</param>
        public virtual void ChangeVal(T type, float val, float? minVal = null, float? maxVal = null, bool onlyDynamicVal = true)
        {
            if (val == 0) return;
            int index = EnumTool<T>.Int(type);
            TDBaseAttrData tempAttrData = GetAttrData(index);

            if (tempAttrData == null) return;
            else if (onlyDynamicVal)
            {
                if (tempAttrData.Type == AttrType.Fixed)
                {
                    CLog.Error("错误!! {0}是Fixed类型,无法使用ChangeVal", typeof(T).Name);
                    return;
                }
            }
            SetVal(type, _getBaseAttrVal(index) + val, minVal, maxVal);
        }
        public virtual void SetVal(T type, float val, float? minVal = null, float? maxVal = null)
        {
            var index = EnumTool<T>.Int(type);
            if (attrDataList != null)
            {
                //截取最大最小限制
                if (minVal != null)
                {
                    if (val < minVal.Value) val = minVal.Value;
                }
                if (maxVal != null)
                {
                    if (val > maxVal) val = maxVal.Value;
                }

                TDBaseAttrData tempAttrData = GetAttrData(index);
                if (tempAttrData == null)
                {
                    _curDataPool[index] = val;
                    _baseDataPool[index] = val;
                }
                else
                {
                    _curDataPool[index] = Mathf.Clamp(val, tempAttrData.Min, tempAttrData.Max);
                    _baseDataPool[index] = Mathf.Clamp(val, tempAttrData.Min, tempAttrData.Max);
                }
            }
            else
            {
                _curDataPool[index] = val;
                _baseDataPool[index] = val;
            }

            if (!lastChangedType.Contains(type))
                lastChangedType.Add(type);
            SetDirty();
        }
        public override void Refresh()
        {
            base.Refresh();
            //重置当前值
            for (int i = 0; i < _baseDataPool.Length; ++i)
                _curDataPool[i] = _getBaseAttrVal(i);
            //计算非百分值
            for (int i = 0; i < _data.Count; ++i)
            {
                if(_data[i].Valid) AddAttrVal(_data[i].Type, _data[i].RealVal);
            }
            //计算百分值
            for (int i = 0; i < _percentData.Count; ++i)
            {
                if (_percentData[i].Valid) AddAttrVal(_percentData[i].Type, _percentData[i].RealVal * _getBaseAttrVal(EnumTool<T>.Int(_percentData[i].Type)));
            }
            //额外加成
            if (_attrConvertData != null)
            {
                foreach (var item in _attrConvertData)
                {
                    if (item != null)
                    {
                        if (item.FactionType == AttrFactionType.DirectAdd)
                            AddAttrVal(item.To, GetExtraAddtion(item));
                        else if (item.FactionType == AttrFactionType.PercentAdd)
                            AddAttrVal(item.To, GetExtraAddtion(item) * _getBaseAttrVal(EnumTool<T>.Int(item.To)));
                    }
                }
            }
            OnAttrChanged(lastChangedType);
            Callback_OnAttrChange?.Invoke(lastChangedType);
            lastChangedType.Clear();
        }
        public void DoCost<TCostType>(List<Cost<TCostType>> datas, bool isReverse = false) where TCostType : Enum
        {
            if (datas == null) return;
            if (isReverse)
            {
                foreach (var item in datas)
                    ChangeVal((T)(object)item.Type, item.RealVal);
            }
            else
            {
                foreach (var item in datas)
                    ChangeVal((T)(object)item.Type, -item.RealVal);
            }
        }
        public virtual void DoCost(List<Cost<T>> datas, bool isReverse = false)
        {
            if (datas == null) return;
            if (isReverse)
            {
                foreach (var item in datas)
                    ChangeVal(item.Type, item.RealVal);
            }
            else
            {
                foreach (var item in datas)
                    ChangeVal(item.Type, -item.RealVal);
            }
        }
        public virtual void DoCost(Cost<T> datas, bool isReverse = false)
        {
            if (datas == null) return;
            if (isReverse) ChangeVal(datas.Type, datas.RealVal);
            else ChangeVal(datas.Type, -datas.RealVal);
        }
        public virtual void DoReward(List<BaseReward> rewards, bool isReverse = false)
        {
            if (rewards == null) return;
            if (!isReverse)
            {
                foreach (var item in rewards)
                    item.Do();
            }
            else
            {
                foreach (var item in rewards)
                    item.UnDo();
            }
        }
        #endregion

        #region core get
        //获得最终属性值(包含自定义Get属性)
        private float _getCurAttrVal(int index)
        {
            if (index < _customAttrVal.Length)
            {
                var invoke = _customAttrVal[index];
                if (invoke != null) return invoke.Invoke();
            }
            return _curDataPool[index];
        }
        private float _getBaseAttrVal(int index)
        {
            if (index < _customAttrVal.Length)
            {
                var invoke = _customAttrVal[index];
                if (invoke != null) return invoke.Invoke();
            }
            return _baseDataPool[index];
        }
        #endregion

        #region get
        public TDBaseAttrData GetAttrData(T type)
        {
            int index = EnumTool<T>.Int(type);
            TDBaseAttrData tempAttrData = GetAttrData(index);
            if (tempAttrData == null)
            {
                CLog.Error("InitAttr:" + type.ToString() + ":没有配置属性表");
                return null;
            }
            return tempAttrData;
        }
        public TDBaseAttrData GetAttrData(int index)
        {
            TDBaseAttrData tempAttrData = null;
            if (index >= attrDataList.Count)
            {
                CLog.Error("InitAttr:" + index + ":没有配置属性表");
                return null;
            }
            tempAttrData = attrDataList[index];
            return tempAttrData;
        }
        public Dictionary<T, float> GetAllCurAttr()
        {
            Dictionary<T, float> ret = new Dictionary<T, float>();

            for (int i = 0; i < _curDataPool.Length; ++i)
            {
                ret.Add((T)(object)(i), _getCurAttrVal(i));
            }
            return ret;
        }
        public Dictionary<string, float> GetAllCurAttrStr()
        {
            Dictionary<string, float> ret = new Dictionary<string, float>();

            for (int i = 0; i < _curDataPool.Length; ++i)
            {
                ret.Add(((T)(object)(i)).ToString(), _getCurAttrVal(i));
            }
            return ret;
        }
        //获得原始Attr属性值
        public float GetVal(T type)
        {
            var index = EnumTool<T>.Int(type);
            return _getCurAttrVal(index);
        }
        public float GetVal(int type)
        {
            return _getCurAttrVal(type);
        }
        //获得Icon
        public Sprite GetIcon(T type)
        {
            if (attrDataList == null)
                return null;
            return attrDataList[EnumTool<T>.Int(type)].GetIcon();
        }
        // 获得额外的加成
        protected float GetExtraAddtion(AttrConvert<T> data, float? customVal = null)
        {
            if (data == null) return 0;
            int fromIndex = EnumTool<T>.Int(data.From);
            TDBaseAttrData attrData = attrDataList[fromIndex];
            var fromVal = _getCurAttrVal(fromIndex);
            if (customVal.HasValue) fromVal = customVal.Value;
            if (fromVal <= data.IgnoreMin || fromVal >= data.IgnoreMax) return 0;
            fromVal += data.Offset;

            var toVal = 0.0f;
            if (data.IsUseSlot && fromVal >= 0 && fromVal < data.Slot.Count)
            {
                toVal = data.Slot[Mathf.RoundToInt(fromVal)];
            }
            else
            {
                var step = data.Step;
                if (step == 0)
                {
                    CLog.Error("Step不能为0");
                    return 0.0f;
                }

                if (!data.IsReverse) toVal = (fromVal / step) * data.Faction;
                else toVal = ((attrData.Max - fromVal) / step) * data.Faction;
            }
            return Mathf.Clamp(toVal, data.Min, data.Max);
        }
        // 得到额外加成的字符窜
        // customVal:可以使用这个值替代原来的属性值
        public string GetConvertStr(T from, string Indent = "", bool isNewline = true, float? customVal = null)
        {
            Dictionary<T, float> tempVal = new Dictionary<T, float>();
            Dictionary<T, AttrConvert<T>> tempConvertVal = new Dictionary<T, AttrConvert<T>>();
            string hint = "";
            if (_attrConvertData != null)
            {
                foreach (var item in _attrConvertData)
                {
                    if (item != null && EnumTool<T>.Int(item.From) == EnumTool<T>.Int(from))
                    {
                        if (!tempVal.ContainsKey(item.To))
                            tempVal.Add(item.To, 0.0f);
                        tempVal[item.To] += GetExtraAddtion(item, customVal);

                        if (!tempConvertVal.ContainsKey(item.To))
                            tempConvertVal.Add(item.To, item);
                    }
                }
            }
            int index = 0;
            foreach (var item in tempVal)
            {
                bool isPercent = tempConvertVal[item.Key].FactionType == AttrFactionType.PercentAdd;
                hint += Indent + GetAttrStr(item.Key, item.Value, isPercent);
                if (isNewline && index < tempVal.Count - 1)
                {
                    hint += "\n";
                }
                index++;
            }
            return hint;
        }
        // 获得翻译后的描述
        public string GetAttrDesc(T type)
        {
            if (attrDataList == null) return "attrData is null:" + type.ToString();
            var index = EnumTool<T>.Int(type);
            if (index >= attrDataList.Count)
                return "out of index:" + type.ToString();
            return attrDataList[index].GetDesc();
        }
        //获得属性字符串
        public string GetAttrStr(List<T> attrs)
        {
            string ret = "";
            ret = StrUtil.List(attrs, (x) =>
            {
                return GetAttrStr(x);
            });
            return ret;
        }
        //获得属性名称+数值
        public string GetAttrStr(T type)
        {
            return GetAttrName(type) + ": " + GetAttrValStr(type, GetVal(type), false, true, false);
        }
        #endregion

        #region Callback
        protected virtual void OnAttrChanged(HashSet<T> data) { }
        #endregion

        #region Utile
        // 获取带有颜色的加成字符,附带正面效果和负面效果的颜色变化
        // e.x. 国家威望 10%
        public static string GetAttrStr(T Type, float Val, bool isPercent = false, bool isIgnoreName = false, bool isColor = true)
        {
            string color = GetAttrColor(Type, Val);
            //属性名称
            string name = isIgnoreName ? "" : GetAttrName(Type);
            //设置属性值
            string strVal = GetAttrValStr(Type, Val, isPercent, false);
            //组合
            string str1 = name + SysConst.UCD_NoBreakingSpace + strVal;
            //属性颜色
            if (isColor) str1 = color + str1 + "</color>";
            return str1;
        }
        // 获取带有颜色的加成字符,附带正面效果和负面效果的颜色变化
        // e.x. +10%
        public static string GetAttrValStr(T Type, float Val, bool isPercent = false, bool isColor = false, bool isSign = true)
        {
            //加工
            string strVal = "None";
            //外部强制设置输入的百分比
            if (isPercent) strVal = UIUtil.Per(Val);
            else strVal = GetAttrNumberStr(Type, Val);
            if (isSign) strVal = UIUtil.Sign(strVal);
            if (isColor)
            {
                string color = GetAttrColor(Type, Val);
                strVal = string.Format("{0}{1}</color>", color, strVal);
            }
            return strVal;
        }
        /// <summary>
        /// 获得完整的属性消耗字符窜
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="RealVal"></param>
        /// <param name="isHaveSign"></param>
        /// <param name="isHaveColor"></param>
        /// <param name="isHeveAttrName"></param>
        /// e.x. +10%威望
        /// <returns></returns>
        public static string GetAttrCostStr(T Type, float RealVal, bool isHaveSign = false, bool isHaveColor = true, bool isHeveAttrName = true)
        {
            string color = GetAttrColor(Type, RealVal);
            string strVal = GetAttrNumberStr(Type, RealVal);
            string strSign = "";
            if (isHaveSign) strSign = GetAttrSign(RealVal);
            string finalStr = strSign + strVal;
            if (isHeveAttrName)
                finalStr = UIUtil.AttrTypeNameSuffix(finalStr, Type);
            string colorFormat = "{0}{1}</color>";
            if (isHaveColor)
                return string.Format(colorFormat, color, finalStr);
            return finalStr;
        }
        public static string GetAttrName(T Type)
        {
            var enumData = (Type as Enum);
            return enumData.GetName();
        }
        /// <summary>
        /// 获得转换后的属性字符窜,比如百分比,KGM
        /// </summary>
        /// <returns></returns>
        public static string GetAttrNumberStr(T Type, float Val)
        {
            List<TDBaseAttrData> tempAttrData = GetAttrDataList();
            if (tempAttrData != null)
            {
                var tempData = tempAttrData[EnumTool<T>.Int(Type)];
                if (tempData.NumberType == NumberType.KMG)
                    return UIUtil.KMG(Val);
                else if (tempData.NumberType == NumberType.Percent)
                    return UIUtil.Per(Val);
                else if (tempData.NumberType == NumberType.Normal)
                    return UIUtil.D1(Val, true);
                else if (tempData.NumberType == NumberType.D2)
                    return UIUtil.D2(Val,true);
                else if (tempData.NumberType == NumberType.Integer)
                    return UIUtil.Round(Val);
                else if (tempData.NumberType == NumberType.Bool)
                    return UIUtil.Bool(Val);
            }
            return Val.ToString();
        }
        /// <summary>
        /// 获得属性的正负符号
        /// </summary>
        /// <returns></returns>
        public static string GetAttrSign(float Val)
        {
            if (Val <= 0)
                return "";
            return "+";
        }
        public static Sprite GetAttrIcon(T type)
        {
            return GetAttrDataList()[EnumTool<T>.Int(type)].GetIcon();
        }
        /// <summary>
        /// 获得属性颜色
        /// </summary>
        /// <returns></returns>
        public static string GetAttrColor(T Type, float Val, bool isReverse = false)
        {
            string color = SysConst.CH_Yellow;
            List<TDBaseAttrData> tempAttrData = GetAttrDataList();
            if (tempAttrData != null)
            {
                var tempData = tempAttrData[EnumTool<T>.Int(Type)];
                if (tempData.BuffType == AttrBuffType.Forward)
                {
                    if (Val > 0)
                        color = SysConst.CH_Green;
                    else if (Val < 0)
                        color = SysConst.CH_Red;
                }
                else if (tempData.BuffType == AttrBuffType.Backward)
                {
                    if (Val < 0)
                        color = SysConst.CH_Green;
                    else if (Val > 0)
                        color = SysConst.CH_Red;
                }
                else
                {
                    color = SysConst.CH_Yellow;
                }

                if (isReverse)
                {
                    if (color.Contains(SysConst.COL_Green))
                    {
                        color.Replace(SysConst.COL_Green, SysConst.COL_Red);
                    }
                    else if (color.Contains(SysConst.COL_Red))
                    {
                        color.Replace(SysConst.COL_Red, SysConst.COL_Green);
                    }
                }
            }
            return color;
        }
        /// <summary>
        /// 获取属性配置数据
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static List<TDBaseAttrData> GetAttrDataList()
        {
            List<TDBaseAttrData> tempAttrData = new List<TDBaseAttrData>();
            if (TDAttr<T>.AttrDataList == null)
                return tempAttrData;
            Type tempType = typeof(T);
            if (TDAttr<T>.AttrDataList.ContainsKey(tempType))
                return TDAttr<T>.AttrDataList[tempType];
            return tempAttrData;
        }
        /// <summary>
        /// 获得属性配置数据Dic
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, TDBaseAttrData> GetAttrDataDic()
        {
            Dictionary<string, TDBaseAttrData> tempAttrData = new Dictionary<string, TDBaseAttrData>();
            if (TDAttr<T>.AttrDataDic == null)
                return tempAttrData;
            Type tempType = typeof(T);
            if (TDAttr<T>.AttrDataList.ContainsKey(tempType))
                return TDAttr<T>.AttrDataDic[tempType];
            return tempAttrData;
        }
        /// <summary>
        /// 根据传入的属性类型,以及值判断是否为正面效果
        /// </summary>
        /// <returns></returns>
        public static bool IsPositive(T Type, float Val)
        {
            bool ret = true;
            List<TDBaseAttrData> tempAttrData = GetAttrDataList();
            if (tempAttrData != null)
            {
                var tempData = tempAttrData[EnumTool<T>.Int(Type)];
                if (tempData.BuffType == AttrBuffType.Forward)
                {
                    if (Val < 0)
                        ret = false;
                    else if (Val > 0)
                        ret = true;
                }
                else if (tempData.BuffType == AttrBuffType.Backward)
                {
                    if (Val < 0)
                        ret = true;
                    else if (Val > 0)
                        ret = false;
                }

            }
            return ret;
        }
        #endregion

        #region db util
        // 初始化所有属性
        public void LoadDBData(ref Dictionary<T, float> attrs)
        {
            if (attrs == null)
                return;

            var temp = attrs;
            EnumTool<T>.For((x) =>
            {
                int index = EnumTool<T>.Int(x);
                if (attrDataList.Count <= index)
                {
                    CLog.Error("没有这个属性:{0}", x);
                    return;
                }
                var tempAttrData = attrDataList[index];
                float obj = tempAttrData.Default;
                if (temp.ContainsKey(x))
                {
                    obj = temp[x];
                }
                SetVal(x, obj);
            });
        }
        // 获得所有属性
        public void SaveDBData(ref Dictionary<T, float> data)
        {
            data = new Dictionary<T, float>();

            for (int i = 0; i < _baseDataPool.Length; ++i)
            {
                data.Add((T)(object)(i), _getBaseAttrVal(i));
            }
        }
        #endregion

        #region db
        public override void OnReadEnd(DBBaseGame data)
        {
            base.OnReadEnd(data);
            SetDirty();
        }
        #endregion
    }

}