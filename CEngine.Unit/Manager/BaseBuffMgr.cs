using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Unit
{
    /// <summary>
    /// BUFF管理器
    /// </summary>
    /// <typeparam name="TTable">buff table</typeparam>
    /// <typeparam name="TData">buff data</typeparam>
    /// <typeparam name="TType">属性的类型枚举</typeparam>
    public class BaseBuffMgr<TData, TType> : BaseMgr, IDBListConverMgr<DBBaseBuff>, IBuffMgr
        where TData : TDBaseBuffData<TType>, new() 
        where TType : Enum
    {
        #region mgr
        IPlotMgr PlotMgr => BaseGlobal.PlotMgr;
        #endregion

        #region buff group
        /// <summary>
        /// buff组
        /// </summary>
        public class BuffGroup:IBuffGroup
        {
            public TData this[int index]=> Buffs[index];
            public BuffGroup()
            {
                MaxLayer = TDBaseBuffData<TType>.MAX_LAYER;
            }

            public int Layer { get { return Buffs.Count; } }
            public int MaxLayer { get; set; }

            public List<TData> Buffs { get; private set; } = new List<TData>();

            public TData Add(TData buff, BaseUnit self)
            {
                MaxLayer = buff.MaxLayer > MaxLayer ? buff.MaxLayer : MaxLayer;
                if (MaxLayer <= 0)
                    return default;
                if (MaxLayer > Layer)
                {
                    Buffs.Add(buff);
                    buff.OnBeAdded(self);
                }
                else
                {
                    Remove(buff);
                    Buffs.Add(buff);
                    buff.OnBeAdded(self);
                }
                return buff;
            }
            public TData Merge(TData buff, BaseUnit self)
            {
                MaxLayer = buff.MaxLayer > MaxLayer ? buff.MaxLayer : MaxLayer;
                if (MaxLayer <= 0)
                    return default;
                TData newBuff = default;
                if (Buffs.Count == 0)
                    newBuff = Add(buff, self);
                else
                {
                    newBuff = Buffs[0];
                    if (newBuff.MergeLayer >= MaxLayer)
                        return newBuff;
                    newBuff.OnMerge(buff);
                }
                return newBuff;
            }
            public void RemoveFirst()
            {
                Buffs[0].OnBeRemoved();
                Buffs.RemoveAt(0);
            }
            public void Remove(TData buff)
            {
                buff.OnBeRemoved();
                Buffs.Remove(buff);
            }
            public TData Buff
            {
                get
                {
                    if (Buffs == null || Buffs.Count <= 0)
                        return default;
                    return Buffs[0];
                }
            }

            public ITDBuffData IBuff => Buff;
            public Sprite GetIcon()
            {
                return IBuff.GetIcon();
            }
        }
        #endregion

        #region member variable
        public Callback Callback_OnBuffChange { get; set; }
        public Dictionary<string, BuffGroup> Data { get; private set; } = new Dictionary<string, BuffGroup>();
        public List<BuffGroup> ListData { get; private set; } = new List<BuffGroup>();
        public List<BuffGroup> ShowData { get; private set; } = new List<BuffGroup>();
        public List<BuffGroup> UpdateData { get; private set; } = new List<BuffGroup>();
        private List<TData> clearBuff = new List<TData>();
        ITDConfig ITDConfig;
        #endregion

        #region life
        //通用的转换buff
        public virtual string ConvertBuff => SysConst.STR_Inv;
        public sealed override MgrType MgrType => MgrType.All;
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            SelfBaseUnit = mono as BaseUnit;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        public override void OnDeath()
        {
            Clear();
            base.OnDeath();
        }
        #endregion

        #region Manual Update
        protected void ManualUpdateBuff()
        {
            if (PlotMgr != null && PlotMgr.IsInPlotPause())
                return;
            if (clearBuff.Count > 0)
                clearBuff.Clear();
            foreach (var item in UpdateData)
            {
                var temp = item;
                for (int i = 0; i < temp.Layer; ++i)
                {
                    temp[i].ManualUpdate();
                    if (temp[i].IsTimeOver)
                        clearBuff.Add(temp[i]);
                }
            }
            foreach (var item in clearBuff)
            {
                Remove(item);
            }
        }
        #endregion

        #region set
        public virtual void Clear()
        {
            foreach (var item in Data)
            {
                var temp = item.Value;
                for (int i = 0; i < temp.Layer; ++i)
                {
                    clearBuff.Add(temp[i]);
                }
            }
            foreach (var item in clearBuff)
            {
                Remove(item);
            }
            ListData.Clear();
            ShowData.Clear();
            Data.Clear();
        }
        public virtual void Add(List<string> buffName)
        {
            if (buffName == null) return;
            if (buffName.Count == 0) return;
            for (int i = 0; i < buffName.Count; ++i)
            {
                Add(buffName[i]);
            }
        }
        public virtual List<TData> Add(string[] buffName)
        {
            if (buffName == null) return null;
            if (buffName.Length == 0) return null;
            List<TData> ret = new List<TData>();
            for (int i = 0; i < buffName.Length; ++i)
            {
                ret.Add(Add(buffName[i]));
            }
            return ret;
        }
        /// <summary>
        /// 添加一个buff
        /// </summary>
        /// <param name="buffName"></param>
        /// <param name="caster">如果为null，caster默认为自己</param>
        public virtual TData Add(string buffName)
        {
            if (buffName.IsInv())
                return null;
            if (!ITDConfig.Contains(buffName))
            {
                CLog.Error("未找到buff errorId=" + buffName);
                return null;
            }
            TData tempBuff = ITDConfig.Get<TData>(buffName).Copy<TData>();
            return Add(tempBuff);
        }
        public virtual ITDBuffData AddBase(string buffName)
        {
            return Add(buffName);
        }
        public virtual void Remove(List<string> buffName)
        {
            if (buffName == null) return;
            for (int i = 0; i < buffName.Count; ++i)
            {
                Remove(buffName[i]);
            }
        }
        public virtual void Remove(string buffName, RemoveBuffType type = RemoveBuffType.Once)
        {
            if (buffName.IsInv()) return;
            if (!ITDConfig.Contains(buffName)) return;
            if (!Data.ContainsKey(buffName)) return;
            BuffGroup group = Data[buffName];
            if (type == RemoveBuffType.Group)
            {
                for (int i = 0; i < group.Layer; ++i)
                    Remove(group[i]);
            }
            else if (type == RemoveBuffType.Once)
            {
                if (group != null && group.Layer > 0)
                    Remove(group[0]);
            }
        }
        public virtual void RemoveAll(Predicate<TData> func)
        {
            List<TData> clearBuff = new List<TData>();
            foreach (var item in Data)
            {
                if (func(item.Value.Buff))
                {
                    clearBuff.Add(item.Value.Buff);
                }
            }
            foreach (var item in clearBuff)
            {
                Remove(item);
            }
        }
        #endregion

        #region is
        public bool IsHave(List<string> buffName)
        {
            if (buffName == null) return false;
            for (int i = 0; i < buffName.Count; ++i)
            {
                if (Data.ContainsKey(buffName[i]))
                    return true;
            }
            return false;
        }
        public bool IsHave(string buffName)
        {
            if (buffName == null) return false;
            if (Data.ContainsKey(buffName))
                return true;
            return false;
        }
        #endregion

        #region get
        public TData Get(string id)
        {
            if (Data.ContainsKey(id))
                return Data[id].Buff;
            return null;
        }
        public TData Get(int index)
        {
            if (ListData.Count > index)
                return ListData[index].Buff;
            return null;
        }
        public List<TData> GeBuffs(List<string> buffs)
        {
            List<TData> ret = new List<TData>();

            foreach (var buffid in buffs)
            {
                if (Data.ContainsKey(buffid))
                {
                    BuffGroup group = Data[buffid];
                    foreach (var item in group.Buffs)
                    {
                        ret.Add(item);
                    }
                }
            }

            return ret;
        }
        #endregion

        #region final action
        public virtual TData Add(TData buff)
        {
            if (!SelfBaseUnit.IsLive)
                return null;
            TData newBuff = null;
            BuffGroup buffGroup = null;
            //有buff组的buff叠加
            if (!string.IsNullOrEmpty(buff.BuffGroupID))
            {
                if (!Data.ContainsKey(buff.BuffGroupID))
                {
                    var tempGroup = new BuffGroup();
                    Data.Add(buff.BuffGroupID, tempGroup);
                    ListData.Add(tempGroup);
                    //没有隐藏的buff加入显示队列
                    if (!buff.IsHide)
                        ShowData.Add(tempGroup);
                    //非永久buff加入 update队列
                    if (buff.IsHaveMaxTime)
                        UpdateData.Add(tempGroup);
                }
                buffGroup = Data[buff.BuffGroupID];
                newBuff = buffGroup.Add(buff, SelfBaseUnit);
            }
            //没有buff组的buff合并
            else
            {
                if (!Data.ContainsKey(buff.TDID))
                {
                    var tempGroup = new BuffGroup();
                    Data.Add(buff.TDID, tempGroup);
                    ListData.Add(tempGroup);
                    //没有隐藏的buff加入显示队列
                    if (!buff.IsHide)
                        ShowData.Add(tempGroup);
                    //非永久buff加入 update队列
                    if (buff.IsHaveMaxTime)
                        UpdateData.Add(tempGroup);
                }
                buffGroup = Data[buff.TDID];
                newBuff = buffGroup.Merge(buff, SelfBaseUnit);
            }
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnBuffChange?.Invoke();
            }
            return newBuff;
        }
        public virtual void Remove(TData buff)
        {
            if (buff == null) return;
            //buff叠加
            if (!string.IsNullOrEmpty(buff.BuffGroupID))
            {
                if (Data.ContainsKey(buff.BuffGroupID))
                {
                    var tempGroup = Data[buff.BuffGroupID];
                    tempGroup.Remove(buff);
                    if (tempGroup.Layer <= 0)
                    {
                        Data.Remove(buff.BuffGroupID);
                        ListData.Remove(tempGroup);
                        if (!buff.IsHide)
                            ShowData.Remove(tempGroup);
                        if (buff.IsHaveRTMaxTime)
                            UpdateData.Remove(tempGroup);
                    }
                }
            }
            //buff合并
            else
            {
                if (Data.ContainsKey(buff.TDID))
                {
                    var tempGroup = Data[buff.TDID];
                    tempGroup.Remove(buff);
                    if (tempGroup.Layer <= 0)
                    {
                        Data.Remove(buff.TDID);
                        ListData.Remove(tempGroup);
                        if (!buff.IsHide)
                            ShowData.Remove(tempGroup);
                        if (buff.IsHaveRTMaxTime)
                            UpdateData.Remove(tempGroup);
                    }
                }
            }
            Callback_OnBuffChange?.Invoke();
        }
        #endregion

        #region Table
        // 通过ID获得buff实体
        public List<TData> GetTableBuffs(List<string> ids)
        {
            List<TData> temp = new List<TData>();
            foreach (var item in ids)
            {
                var tempBuff = ITDConfig.Get<TData>(item);
                if (tempBuff == null)
                {
                    CLog.Error("没有这个buff:" + item);
                    continue;
                }
                temp.Add(tempBuff);
            }
            return temp;
        }
        // 拼接所有传入的buff addtion 的字符窜
        public string GetTableDesc(List<string> ids, bool newLine = false, string split = SysConst.STR_Indent, float? anticipationFaction = null, bool appendHeadInfo = false)
        {
            return GetDesc(GetTableBuffs(ids), newLine, split, anticipationFaction, appendHeadInfo);
        }
        // 拼接所有传入的buff addtion 的字符窜
        public string GetTableDesc(string id, bool newLine = false, string split = SysConst.STR_Indent, float? inputVal = null, bool appendHeadInfo = false)
        {
            var tempBuff = ITDConfig.Get<TData>(id);
            if (tempBuff == null)
            {
                CLog.Error("没有这个buff:" + id);
                return "";
            }
            return GetDesc(tempBuff, newLine, split, inputVal, appendHeadInfo);
        }
        public List<AttrAdditon<TType>> GetTableAdt(string buffId)
        {
            var tempBuff = ITDConfig.Get<TData>(buffId);
            if (tempBuff == null)
            {
                CLog.Error("没有这个buff:" + buffId);
                return new List<AttrAdditon<TType>>();
            }
            return tempBuff.Attr;
        }
        #endregion

        #region AddtionStr
        // 设置buff信息
        private static string AppendBuffHeadInfo(TData buff)
        {
            string CDTime = "";
            if (buff.IsHaveRTMaxTime) CDTime = string.Format("({0})", buff.RTMaxTime.ToString());
            else if (buff.IsHaveMaxTime) CDTime = string.Format("({0})", buff.MaxTime.ToString());
            return BaseLangMgr.Get("Buff_HeadInfo", buff.GetName(), CDTime);
        }

        // 拼接所有传入的buff addtion 的字符窜
        public string GetDesc(List<TData> buffs, bool newLine = false, string split = SysConst.STR_Indent, float? anticipationFaction = null, bool appendHeadInfo = false)
        {
            string showStr = "";
            int index = 0;
            foreach (var item in buffs)
            {
                if (newLine && index != 0)
                    showStr += "\n";
                showStr += GetDesc(item, newLine, split, anticipationFaction, appendHeadInfo);
                index++;
            }
            return showStr;
        }
        public string GetDesc(string buff, bool newLine = false, string split = SysConst.STR_Indent, float? inputVal = null, bool appendHeadInfo = false)
        {
            return GetDesc(Get(buff), newLine, split, inputVal, appendHeadInfo);
        }
        // 通过所有传入的buff获得加成字符窜
        public string GetDesc(TData buff, bool newLine = false, string split = SysConst.STR_Indent, float? inputVal = null, bool appendHeadInfo = false)
        {
            string showStr = "";
            if (buff == null) return "";
            if (appendHeadInfo)
            {
                showStr += AppendBuffHeadInfo(buff) + "\n";
            }
            int index = 0;
            var data = buff.GetAdtStrs(inputVal);
            foreach (var addStr in data)
            {
                if (newLine && index != 0)
                    showStr += "\n";
                showStr += split + addStr;
                index++;
            }
            return showStr;
        }
        #endregion

        #region db
        public override void OnRead2(DBBaseGame data)
        {
            base.OnRead2(data);
            if (IsNew)
            {
                if(!ConvertBuff.IsInv())
                    Add(ConvertBuff);
            }
        }
        public override void OnWrite(DBBaseGame data)
        {
            base.OnWrite(data);
        }
        #endregion

        #region db utile
        public void SaveDBData(ref List<DBBaseBuff> ret)
        {
            ret = new List<DBBaseBuff>();
            foreach (var item in Data.Values)
            {
                foreach (var buff in item.Buffs)
                {
                    DBBaseBuff dbbuff = new DBBaseBuff();
                    dbbuff.TDID = buff.TDID;
                    dbbuff.CD = buff.CurTime;
                    dbbuff.Input = buff.Input;
                    dbbuff.Valid = buff.Valid;
                    ret.Add(dbbuff);
                }
            }
        }
        public void LoadDBData(ref List<DBBaseBuff> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                var temp = Add(item.TDID);
                temp.SetCD(item.CD);
                temp.SetInput(item.Input);
                temp.SetValid(item.Valid);
            }
        }
        #endregion
    }

}