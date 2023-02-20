using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//**********************************************
// Class Name	: Battle_BaseLuaBattle
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    #region TD
    [Serializable]
    public class TDBaseData : IBase, ICloneable, ITDBaseData
    {
        #region prop
        public ITDMgr TDMgr { get; set; }
        protected BaseAudioMgr AudioMgr => BaseGlobal.AudioMgr;
        protected object[] AddedObjs { get; private set; }
        public BaseUnit SelfBaseUnit { get; protected set; }
        public BaseUnit OwnerBaseUnit { get; protected set; }
        //来源
        public BaseUnit CastBaseUnit { get; protected set; }
        #endregion

        #region config
        //索引
        public int Index { get; set; }
        //runtime id
        public long ID { get; set; }
        //string id
        public string TDID { get; set; } = SysConst.STR_Inv;
        //int id
        public int INID { get; set; } = SysConst.INT_Inv;
        //剧本ID
        public string Drama { get; set; } = SysConst.STR_Inv;
        //分组ID
        public string Group { get; set; } = SysConst.STR_Inv;
        public int Repeat { get; set; } = SysConst.INT_Inv;
        //和所有人都是敌人的单位(盗贼,野怪等)
        public bool IsWild { get; set; } = false;
        //克隆类型
        public CloneType CloneType { get; set; } = CloneType.Memberwise;
        //图标
        public string Icon { get; set; } = "";
        //描述
        public string Desc { get; set; } = "";
        //简介
        public string Cont { get; set; } = "";
        //名称
        public string Name { get; set; } = "";
        //Prefab
        public string Prefab { get; set; } = "";
        //Buff
        public string Buff { get; set; } = "";
        //音效
        public string SFX { get; set; } = "";
        //模板
        public string Template { get; set; } = "";
        //注释
        public string Notes { get; set; } = "";
        #endregion

        #region Custom
        //是否从配置表中读取的,有些是动态生成的
        public bool IsConfig { get;private set; } = false;
        //单位是否存活
        public bool IsLive { get; set; } = true;
        //自定义名称
        public string CustomName { get; set; } = "";
        //自定义分数
        public float CustomScore => throw new NotImplementedException();
        #endregion

        #region life
        //默认无效的TDID
        protected virtual string CustomInvalidID => "0";
        /// <summary>
        /// 被添加的时候触发:手动调用
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="obj"></param>
        public virtual void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            IsLive = true;
            AddedObjs = obj;
            SetSelfBaseUnit(selfMono as BaseUnit);
        }
        /// <summary>
        /// 被移除的时候,手动调用
        /// </summary>
        public virtual void OnBeRemoved()
        {
            IsLive = false;
        }
        /// <summary>
        /// 新生成
        /// </summary>
        public virtual void OnNewSpawn() { }
        /// <summary>
        /// update:手动调用
        /// </summary>
        public virtual void ManualUpdate() { }
        /// <summary>
        /// 帧同步:手动调用
        /// </summary>
        /// <param name="gameFramesPerSecond"></param>
        public virtual void OnTurnframe(int gameFramesPerSecond) { }
        /// <summary>
        /// 更新:手动调用
        /// </summary>
        public virtual void OnTurnbase() { }
        /// <summary>
        /// 被添加到数据表里
        /// </summary>
        public virtual void OnBeAddedToData() 
        {
            IsConfig = true;
        }
        public virtual void OnLuaParseStart() { }
        public virtual void OnLuaParseEnd() { }
        public virtual void OnExcelParseStart() { }
        public virtual void OnExcelParseEnd() { }
        public virtual void OnAllLoadEnd1() { }
        public virtual void OnAllLoadEnd2() { }
        public virtual void DoDeath()
        {
            IsLive = false;
            TDMgr?.Despawn(this);
        }
        #endregion

        #region get
        // 安全获得输入对象
        protected TType GetAddedObjData<TType>(int index) where TType : class
        {
            if (AddedObjs == null || AddedObjs.Length <= index)
                return default;
            return (AddedObjs[index] as TType);
        }
        public string GetTDID()
        {
            return TDID;
        }
        // 返回翻译后的名字
        public virtual string GetName()
        {
            if (!CustomName.IsInv())
                return CustomName;
            if (!Name.IsInv())
                return BaseLangMgr.Get(Name);
            string nameTDID = SysConst.Prefix_Name + TDID;
            if (BaseLangMgr.IsContain(nameTDID))
                return BaseLangMgr.Get(nameTDID);
            return BaseLangMgr.Get(TDID);
        }
        // 获取自动提示
        public virtual string GetDesc(params object[] ps)
        {
            if (!string.IsNullOrEmpty(Desc))
                return BaseLangMgr.Get(Desc, ps);
            string descTDID = SysConst.Prefix_Desc + TDID;
            if (BaseLangMgr.IsContain(descTDID))
                return BaseLangMgr.Get(descTDID, ps);
            return BaseLangMgr.Get(SysConst.STR_Desc_NoDesc);
        }
        public virtual string GetCont()
        {
            if (!string.IsNullOrEmpty(Cont))
                return BaseLangMgr.Get(Cont);
            string descTDID = SysConst.Prefix_Cont + TDID;
            if (BaseLangMgr.IsContain(descTDID))
                return BaseLangMgr.Get(descTDID);
            return BaseLangMgr.Get(SysConst.STR_Desc_NoDesc);
        }
        // 获取icon
        public virtual Sprite GetIcon()
        {
            if (Icon == CustomInvalidID)
                return null;
            if (!Icon.IsInv()) 
                return BaseGlobal.RsIcon.Get(Icon,false);
            else if (!TDID.IsInv() && BaseGlobal.RsIcon.IsHave(TDID)) 
                return BaseGlobal.RsIcon.Get(TDID,false);
            return null;
        }
        public virtual Sprite GetIllstration()
        {
            if (Icon == CustomInvalidID)
                return null;
            if (!Icon.IsInv())
                return BaseGlobal.RsIllustration.Get(Icon, false);
            else if (!TDID.IsInv() && BaseGlobal.RsIllustration.IsHave(TDID))
                return BaseGlobal.RsIllustration.Get(TDID, false);
            return null;
        }
        // 获得禁用的图标,有可能没有
        public virtual Sprite GetDisIcon()
        {
            if (Icon == CustomInvalidID)
                return null;
            if (!Icon.IsInv())
                return BaseGlobal.RsIcon.Get(Icon + SysConst.Suffix_Disable,false);
            return BaseGlobal.RsIcon.Get(TDID + SysConst.Suffix_Disable,false);
        }
        public Sprite GetSelIcon()
        {
            if (Icon == CustomInvalidID)
                return null;
            if (!Icon.IsInv())
                return BaseGlobal.RsIcon.Get(Icon + SysConst.Suffix_Sel,false);
            return BaseGlobal.RsIcon.Get(TDID + SysConst.Suffix_Sel,false);
        }
        // prefab
        public virtual GameObject GetPrefab(IRsCacherT<GameObject> cacher = null)
        {
            if (cacher == null)
                cacher = BaseGlobal.RsPrefab;
            if (Prefab == CustomInvalidID)
                return null;
            if (!Prefab.IsInv())
                return cacher.Get(Prefab);
            return cacher.Get(TDID);
        }
        public virtual string GetBuff()
        {
            if (Buff == CustomInvalidID)
                return null;
            if (!Buff.IsInv())
                return Buff;
            return "Buff_"+TDID;
        }
        // 获得animator
        public virtual RuntimeAnimatorController GetAnimator()
        {
            return BaseGlobal.RsAnimator.Get(TDID);
        }
        //获得SFX
        public virtual AudioClip GetSFX()
        {
            if (SFX == CustomInvalidID)
                return null;
            if (!SFX.IsInv())
                return BaseGlobal.RsAudio.Get(SFX);
            return null;
        }
        public string GetStr(string key, params object[] ps)
        {
            return BaseLangMgr.Get(key, ps);
        }
        #endregion

        #region set
        public void SetCustomName(string name)
        {
            CustomName = name;
        }
        public void SetOwnerBaseUnit(BaseUnit owner)
        {
            OwnerBaseUnit = owner;
        }
        public void SetSelfBaseUnit(BaseUnit unit)
        {
            SelfBaseUnit = unit;
        }
        public void SeCastBaseUnit(BaseUnit unit)
        {
            CastBaseUnit = unit;
        }
        #endregion

        #region is
        public bool IsPlayer()
        {
            if (SelfBaseUnit == null) return false;
            return SelfBaseUnit.IsPlayer();
        }
        //判断数据表格是否是无效的
        public virtual bool IsInv()
        {
            return TDID.IsInv() && ID.IsInv();
        }
        public virtual bool IsValid()
        {
            return TDID.IsValid() && ID.IsValid();
        }
        #endregion

        #region copy
        protected virtual void DeepClone(object sourceObj)
        {
            throw new NotImplementedException("此函数必须被实现");
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public virtual TClass Copy<TClass>() where TClass : TDBaseData, new()
        {
            TClass tempBuff = null;
            {
                //浅层拷贝,拷贝所有值字段
                if (CloneType == CloneType.Memberwise)
                    tempBuff = Clone() as TClass;
                //拷贝所有值字段,包括用户自定义的深层拷贝
                else if (CloneType == CloneType.Deep)
                {
                    tempBuff = Clone() as TClass;
                    tempBuff.DeepClone(this);
                }
            }
            return tempBuff;
        }
        #endregion

        #region operate
        public static explicit operator string(TDBaseData data)
        {
            return data.TDID;
        }
        #endregion
    }
    #endregion

    #region Config
    // 会根据类名自动生成Lua方发, e.g. TDNationData 会截头去尾 变成:AddNation
    public class TDBaseConfig<T> : IDDic<string, T>, ITDConfig 
        where T : TDBaseData, new()
    {
        #region drama id
        //剧本的ID
        static string Drama
        {
            get
            {
                if (BaseGlobal.ScreenMgr == null) return "";
                return BaseGlobal.ScreenMgr.SelectedDrama;
            }
        }
        #endregion

        #region val
        public Type DataType { get; private set; }
        public virtual string TableMapperInfo => "";
        public Excel.TableMapper TableMapper { get; protected set; }
        public Dictionary<string, TDBaseData> BaseDatas { get; private set; } = new Dictionary<string, TDBaseData>();
        public List<string> ListKeys { get; private set; } = new List<string>();
        public List<T> ListValues { get; private set; } = new List<T>();
        public List<object> ListObjValues { get; private set; } = new List<object>();
        public Dictionary<string, List<T>> Groups { get; private set; } = new Dictionary<string, List<T>>();
        /// <summary>
        /// 默认lua表格数据
        /// </summary>
        protected string LuaTableKey = SysConst.STR_Inv;
        protected DynValue MetaTable = null;
        protected string AddMethonName;
        protected string AlterMethonName;
        /// <summary>
        /// 默认的命名控件
        /// </summary>
        protected string NameSpace = SysConst.STR_Inv;
        protected DynValue baseTable;
        protected T TempClassData;
        public T Default { get; private set; }
        #endregion

        #region init
        public TDBaseConfig()
        {
            Init();
        }
        public TDBaseConfig(string keyName)
        {
            Init(keyName);
        }

        protected virtual void Init(string keyName = null)
        {
            if (keyName == null) LuaTableKey = typeof(T).Name;
            else LuaTableKey = keyName;
            //去除头部TD前缀和尾部Data后缀
            {
                LuaTableKey = LuaTableKey.TrimStart("TD");
                LuaTableKey = LuaTableKey.TrimStart("Base");
                LuaTableKey = LuaTableKey.TrimEnd("Data");
            }
            NameSpace = typeof(T).Namespace.ToString();
            AddMethonName = string.Format("Add{0}", LuaTableKey);
            AlterMethonName = string.Format("Alter{0}", LuaTableKey);
            DataType = typeof(T);
            if (TableMapperInfo != "")
            {
                TableMapper = new Excel.TableMapper(typeof(T)).Map(TableMapperInfo);
            }
            BaseLuaMgr.AddGlobalAction(AddMethonName, AddFromLua);
            BaseLuaMgr.AddGlobalAction(AlterMethonName, AlterFronLua);
            BaseLuaMgr.AddTDConfig(LuaTableKey, typeof(T), this);
        }
        #endregion

        #region set
        public sealed override void Add(string id, T ent)
        {
            //添加组
            if (!ent.Group.IsInv())
            {
                string drama_groupID = ent.Drama + ent.Group;
                if (!Groups.ContainsKey(drama_groupID))
                    Groups.Add(drama_groupID, new List<T>());
                var groupData = Groups[drama_groupID];
                //自动归组,适用于不同参数的对话
                if (ent.Repeat.IsInv())
                {
                    groupData.Add(ent);
                    //如果没有填写id,则根据组名,自动创建
                    _internalAdd(ent.Group+ groupData.Count, drama_groupID + groupData.Count, ent);
                }
                //依据repeat自动创建一摸一样的添加到组,适用于对话
                else
                {
                    for (int i = 1; i <= ent.Repeat; ++i)
                    {
                        var newEnt = ent.Copy<T>();
                        groupData.Add(newEnt);
                        _internalAdd(ent.Group + i, drama_groupID + i,ent);
                    }
                }
            }
            //添加
            else
            {
                string drama_tdid = ent.Drama + id;
                _internalAdd(id, drama_tdid, ent);
            }

            void _internalAdd(string tdid,string indexKey, T ent)
            {
                if (ContainsKey(indexKey))
                {
                    CLog.Error("重复的Key:" + indexKey);
                    return;
                }
                else
                {
                    //如果标记了剧本，自动拷贝原始数据到新的剧本副本中
                    if (!ent.Drama.IsInv())
                    {
                        var source = Get(tdid);
                        if (source != null)
                        {
                            ConfigCopy<T>.To(source, ent);
                        }
                    }

                    //设置TDID
                    ent.TDID = tdid;
                    if (int.TryParse(tdid, out int intID))
                    {
                        //解析INID
                        ent.INID = intID;
                    }
                }
                //添加到数据中
                base.Add(indexKey, ent);
                BaseDatas.Add(indexKey, ent);
                ListKeys.Add(indexKey);
                ListObjValues.Add(ent);
                ListValues.Add(ent);
            }
        }
        public void Alter(string id, T ent)
        {
            if (!ContainsKey(id))
            {
                CLog.Error("没有这个Key:" + id);
                return;
            }
            this[id] = ent;
            BaseDatas[id] = ent;
        }
        public override void Remove(string id)
        {
            base.Remove(id);
            BaseDatas.Remove(id);
            ListKeys.Remove(id);
        }
        public T Rand()
        {
            return Get(ListKeys.Rand());
        }
        public string RandKey()
        {
            return ListKeys.Rand();
        }
        DynValue CopyPairs(DynValue target, DynValue source)
        {
            foreach (var item in source.Table.Pairs)
            {
                target.Table.Set(item.Key, item.Value);
            }
            return target;
        }
        #endregion

        #region Add data
        public DynValue AddByDefaultFromLua(DynValue metaValue, DynValue luaValue)
        {
            if (metaValue == null) return null;
            if (luaValue == null) return null;
            Closure funcGetNewTable = GetDynVal("GetNewTable").Function;
            DynValue ret = funcGetNewTable.Call();
            if (metaValue.IsNotNil())
                ret = CopyPairs(ret, metaValue);
            if (luaValue.IsNotNil())
                ret = CopyPairs(ret, luaValue);

            return ret;
        }
        public void AddFromLua(DynValue luaValue)
        {
            if (luaValue.Table == null)
            {
                CLog.Error("错误!Add(DynValue table),必须是一个Table");
                return;
            }
            if (MetaTable == null) 
                MetaTable = GetDynVal(LuaTableKey);

            if (MetaTable != null)
                baseTable = AddByDefaultFromLua(MetaTable, luaValue);
            else 
                baseTable = luaValue;

            //获得Lua类模板
            Type classType = typeof(T);
            TempClassData = (T)LuaReader.Convert(baseTable, classType);
            if (TempClassData == null) return;
            string key = TempClassData.TDID;
            if (ContainsKey(key))
            {
                CLog.Error(baseTable.ToString() + "Add:已经存在这个key:" + key);
                return;
            }
            TempClassData.TDID = key;
            AddFromObj(TempClassData);
        }
        //修改/重载/适用于DLC/玩家扩展
        public void AlterFronLua(DynValue luaValue)
        {
            if (luaValue.Table == null)
            {
                CLog.Error("错误!Add(DynValue table),必须是一个Table");
                return;
            }
            string key = luaValue.Table.Get("TDID").ToString();
            if (!ContainsKey(key))
            {
                CLog.Error(baseTable.ToString() + "Alter:不存在key:" + key);
                return;
            }
            else
            {
                var cObject = Get(key);
                foreach (TablePair propertyPair in luaValue.Table.Pairs)
                    LuaReader.SetValue(cObject, propertyPair.Key.String, propertyPair.Value);
            }
        }
        public T AddFromObj(T ent)
        {
            Add(ent.TDID, ent);
            ent.OnBeAddedToData();
            return ent;
        }
        public T AlterFromObj(T ent)
        {
            Alter(ent.TDID, ent);
            return ent;
        }
        public void AddAlterRangeFromObj(IEnumerable<object> data)
        {
            if (data == null)
                return;
            foreach (var obj in data)
            {
                var item = obj as T;
                if (!Contains(item.TDID))
                    AddFromObj(item);
                else
                    AlterFromObj(item);
            }
        }
        #endregion

        #region get
        protected Table GetTable(string name)
        {
            DynValue temp = BaseLuaMgr.Lua.Globals.Get(name);
            if (temp == null)
                return null;
            return temp.Table;
        }
        protected DynValue GetDynVal(string name)
        {
            DynValue temp = BaseLuaMgr.Lua.Globals.Get(name);
            return temp;
        }
        protected string GetStrByBaseTable(string name)
        {
            DynValue temp = baseTable.Table.RawGet(name);
            if (temp == null)
                return null;
            return temp.String;
        }
        public List<T> GetGroup(string group)
        {
            return GetRawGroup(group) as List<T>;
        }
        public TData Get<TData>(string key) where TData : TDBaseData
        {
            return Get(key) as TData;
        }
        protected string GetStr(string key, params object[] objs) => BaseLangMgr.Get(key, objs);
        #endregion

        #region raw get
        public new T Get(string key)
        {
            string dramaKey = Drama + key;
            if (base.ContainsKey(dramaKey))
                return base.Get(dramaKey);
            return base.Get(key);
        }
        public IList GetRawGroup(string group)
        {
            string dramaGroup = Drama + group;
            if (Groups.ContainsKey(dramaGroup))
                return Groups[dramaGroup];
            else if (Groups.ContainsKey(group))
                return Groups[group];
            return new List<T>();
        }
        #endregion

        #region Callback
        public virtual void OnLuaParseStart()
        {
            foreach (var item in this)
            {
                item.Value.OnLuaParseStart();
            }
        }
        public virtual void OnLuaParseEnd()
        {
            int index = 0;
            if (MetaTable == null) MetaTable = GetDynVal(LuaTableKey);
            Default = (T)LuaReader.Convert(MetaTable, typeof(T));
            foreach (var item in this)
            {
                item.Value.OnLuaParseEnd();
                item.Value.Index = index;
                index++;
            }
        }

        public virtual void OnExcelParseStart()
        {
            foreach (var item in this)
            {
                item.Value.OnExcelParseStart();
            }
        }
        public virtual void OnExcelParseEnd()
        {
            foreach (var item in this)
            {
                item.Value.OnExcelParseEnd();
            }
        }
        public virtual void OnAllLoadEnd1()
        {
            foreach (var item in this)
            {
                item.Value.OnAllLoadEnd1();
            }
        }
        public virtual void OnAllLoadEnd2()
        {
            foreach (var item in this)
            {
                item.Value.OnAllLoadEnd2();
            }
        }
        #endregion
    }
    // 静态配置类,完全继承TDBaseConfig,但是调用更加方便
    public class TDBaseGlobalConfig<TConfigData, TConfig> : TDBaseConfig<TConfigData>
        where TConfigData : TDBaseData, new()
        where TConfig : TDBaseConfig<TConfigData>,new()
    {
        #region life
        public static TConfig Ins { get; private set; }
        public TDBaseGlobalConfig():base()
        {
            Ins = this as TConfig;
        }
        #endregion

        #region pub get
        public new static TData Get<TData>(string key) where TData : TDBaseData => Ins.Get<TData>(key);
        public new static TConfigData Get(string key) => Ins.Get(key);
        public new static List<TConfigData> GetGroup(string group) => Ins.GetGroup(group);
        public new static IList GetRawGroup(string group) => Ins.GetRawGroup(group);
        #endregion
    }
    #endregion
}