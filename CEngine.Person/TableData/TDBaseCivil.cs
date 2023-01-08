//------------------------------------------------------------------------------
// TDBaseName.cs
// Copyright 2019 2019/2/18 
// Created by CYM on 2019/2/18
// Owner: CYM
// 填写类的描述...
// P:普通的头像列表,自己配置的列表,S:标记了特殊用途Tag的头像列表,年龄Tag,或者自定义Tag
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CYM.Person
{
    [Serializable]
    public class GenderInfo
    {
        #region Config
        public string Name { get; set; } = "";
        #endregion

        #region IConfig
        public PartConfig Bare { get; set; } = new PartConfig();
        public PartConfig Eye { get; set; } = new PartConfig();
        public PartConfig Nose { get; set; } = new PartConfig();
        public PartConfig Hair { get; set; } = new PartConfig();
        public PartConfig Mouse { get; set; } = new PartConfig();
        public PartConfig Brow { get; set; } = new PartConfig();
        public PartConfig Beard { get; set; } = new PartConfig();
        public PartConfig Body { get; set; } = new PartConfig();
        public PartConfig Decorate { get; set; } = new PartConfig();
        public PartConfig BG { get; set; } = new PartConfig();
        public PartConfig Helmet { get; set; } = new PartConfig();
        public PartConfig Frame { get; set; } = new PartConfig();
        public PartConfig Full { get; set; } = new PartConfig();
        #endregion

        #region prop
        //已经进入配置的数据
        HashSet<string> IntoConfiged = new HashSet<string>();
        public List<string> NameKeys { get; private set; } = new List<string>();
        #endregion

        #region get
        public HashList<PartInfo> GetSBare(string tag) => GetPartInfo(tag, Bare.Config, Bare.Parsed);
        public HashList<PartInfo> GetSEye(string tag) => GetPartInfo(tag, Eye.Config, Eye.Parsed);
        public HashList<PartInfo> GetSNose(string tag) => GetPartInfo(tag, Nose.Config, Nose.Parsed);
        public HashList<PartInfo> GetSHair(string tag) => GetPartInfo(tag, Hair.Config, Hair.Parsed);
        public HashList<PartInfo> GetSMouse(string tag) => GetPartInfo(tag, Mouse.Config, Mouse.Parsed);
        public HashList<PartInfo> GetSBrow(string tag) => GetPartInfo(tag, Brow.Config, Brow.Parsed);
        public HashList<PartInfo> GetSBeard(string tag) => GetPartInfo(tag, Beard.Config, Beard.Parsed);
        public HashList<PartInfo> GetSBody(string tag) => GetPartInfo(tag, Body.Config, Body.Parsed);
        public HashList<PartInfo> GetSDecorate(string tag) => GetPartInfo(tag, Decorate.Config, Decorate.Parsed);
        public HashList<PartInfo> GetSBG(string tag) => GetPartInfo(tag, BG.Config, BG.Parsed);
        public HashList<PartInfo> GetSHelmet(string tag) => GetPartInfo(tag, Helmet.Config, Helmet.Parsed);
        public HashList<PartInfo> GetSFrame(string tag) => GetPartInfo(tag, Frame.Config, Frame.Parsed);
        public HashList<PartInfo> GetSFull(string tag) => GetPartInfo(tag, Full.Config, Full.Parsed);
        private HashList<PartInfo> GetPartInfo(string tag, HashList<PartInfo> pdata, Dictionary<string, HashList<PartInfo>> sdata)
        {
            if (tag == SysConst.PTag_Normal) return pdata;
            if (sdata.ContainsKey(tag)) return sdata[tag];
            return pdata;
        }
        #endregion

        #region parse
        public void Parse()
        {
            IntoConfiged.Clear();
            NameKeys = BaseLangMgr.GetCategoryKeyList(Name);
            ParseInternal(Bare, Bare.Config, Bare.Parsed);
            ParseInternal(Eye, Eye.Config, Eye.Parsed);
            ParseInternal(Nose,Nose.Config, Nose.Parsed);
            ParseInternal(Hair, Hair.Config, Hair.Parsed);
            ParseInternal(Mouse, Mouse.Config, Mouse.Parsed);
            ParseInternal(Brow, Brow.Config, Brow.Parsed);
            ParseInternal(Beard, Beard.Config, Beard.Parsed);
            ParseInternal(Body, Body.Config, Body.Parsed);
            ParseInternal(Decorate, Decorate.Config, Decorate.Parsed);
            ParseInternal(BG, BG.Config, BG.Parsed);
            ParseInternal(Helmet, Helmet.Config, Helmet.Parsed);
            ParseInternal(Frame, Frame.Config, Frame.Parsed);
            ParseInternal(Full, Full.Config, Full.Parsed);

            //内部解析函数
            void ParseInternal(PartConfig iConfig, List<PartInfo> pConfig, Dictionary<string, HashList<PartInfo>> sConfig)
            {
                //PConfig进入配置,不再使用iConfig和rConfig生成
                foreach (var pConfigItem in pConfig)
                {
                    IntoConfiged.Add(pConfigItem.Name);
                }
                //解析RConfig配置,进入PConfig
                if (!iConfig.Director.IsInv())
                {
                    var data = BaseGlobal.RsHead.GetStrsByCategory(iConfig.Director);
                    if (data != null)
                    {
                        foreach (var dir in data)
                        {
                            if (IntoConfiged.Contains(dir))
                                continue;
                            pConfig.Add(new PartInfo { Name = dir, IsIn = true, Prop = iConfig.Prop });
                            IntoConfiged.Add(dir);
                        }
                    }
                }
                //解析IConfig配置,进入PConfig
                if (!iConfig.PrefixName.IsInv())
                {
                    for (int i = 1; i <= iConfig.PrefixCount; ++i)
                    {
                        string name = iConfig.PrefixName + i;
                        if (IntoConfiged.Contains(name))
                            continue;
                        pConfig.Add(new PartInfo { Name = name, IsIn = true, Prop = iConfig.Prop });
                    }
                }
                //解析PConfig
                foreach (var pConfigItem in pConfig)
                {
                    //加入年龄Tag
                    EnumTool<AgeRange>.For(x =>
                    {
                        string tagAge = x.ToString();
                        HashList<PartInfo> temp;
                        if (sConfig.ContainsKey(tagAge)) temp = sConfig[tagAge];
                        else
                        {
                            temp = new HashList<PartInfo>();
                            sConfig.Add(tagAge, temp);
                        }
                        var addtionName = pConfigItem.Name + "_" + tagAge;
                        if (BaseGlobal.RsHead.IsHave(addtionName))
                        {
                            PartInfo newPartInfo = pConfigItem.Clone() as PartInfo;
                            newPartInfo.Name = addtionName;
                            temp.Add(newPartInfo);
                        }
                    });

                    //加入自定义Tag
                    if (pConfigItem.Tag == null) continue;
                    foreach (var tag in pConfigItem.Tag)
                    {
                        HashList<PartInfo> temp;
                        if (sConfig.ContainsKey(tag)) temp = sConfig[tag];
                        else
                        {
                            temp = new HashList<PartInfo>();
                            sConfig.Add(tag, temp);
                        }
                        temp.Add(pConfigItem);
                    }
                }
                pConfig.RemoveAll((x) =>
                {
                    return !x.IsIn;
                });
            }
        }
        #endregion
    }
    [Serializable]
    public class PartConfig
    {
        public string Director { get; set; } = SysConst.STR_Inv;
        public string PrefixName { get; set; } = SysConst.STR_Inv;
        public int PrefixCount { get; set; } = SysConst.INT_Inv;
        public float Prop { get; set; } = 1.0f;

        public HashList<PartInfo> Config { get; set; } = new HashList<PartInfo>();
        public Dictionary<string, HashList<PartInfo>> Parsed { get;private set; } = new Dictionary<string, HashList<PartInfo>>();
    }
    [Serializable]
    public class PartInfo : ICloneable
    {
        public string Name { get; set; } = SysConst.STR_Inv;
        //是否在随机列表中
        public bool IsIn { get; set; } = true;
        public float Prop { get; set; } = 1.0f;
        public HashList<string> Tag { get; set; } = new HashList<string>();

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    [Serializable]
    public class TDBaseCivilData : TDBaseData
    {
        #region config
        public string Splite { get; set; } = "";
        public string Last { get; set; } = "";
        public string LegionNameGroup { get; set; } = "";
        public GenderInfo Male { get; set; } = new GenderInfo();
        public GenderInfo Female { get; set; } = new GenderInfo();
        #endregion

        #region prop
        List<string> All { get; set; } = new List<string>();
        public List<string> LastNameKeys { get; private set; } = new List<string>();
        public List<string> LegionName { get; private set; } = new List<string>();
        #endregion

        #region life
        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            LastNameKeys.Clear();
            LastNameKeys = BaseLangMgr.GetCategoryKeyList(Last);
            All.Clear();
            All.AddRange(Male.NameKeys);
            All.AddRange(Female.NameKeys);
            All.AddRange(LastNameKeys);
            Male.Parse();
            Female.Parse();
            LegionName = BaseLangMgr.GetCategoryKeyList(LegionNameGroup);
        }
        #endregion

        #region get
        public string GetPersonName(ITDPersonData person)
        {
            string ret = "";
            string splite = "";
            string firstName = "";
            if (!BaseLangMgr.Space.IsInv()) 
                splite = BaseLangMgr.Space;
            else 
                splite = Splite;

            if (CustomName.IsInv()) 
                firstName = person.FirstName.GetName();
            else 
                firstName = CustomName;

            ret = person.LastName.GetName() + splite + firstName;
            return ret;
        }
        public string RandFirstNameKey(Gender gender) => GetInfo(gender).NameKeys.Rand();
        public string RandLastNameKey() => LastNameKeys.Rand();
        // 只获得名称
        public string RandName(bool isTrans = false)
        {
            string ret = RandUtil.RandArray(All);
            if (isTrans) return BaseLangMgr.Get(ret);
            return ret;
        }
        public Dictionary<PHIPart, string> RandHeadIcon(Gender gender, string tag, Dictionary<PHIPart, string> ret)
        {
            RandPart(PHIPart.PBare, ref ret);
            RandPart(PHIPart.PEye, ref ret);
            RandPart(PHIPart.PNose, ref ret);
            RandPart(PHIPart.PHair, ref ret);
            RandPart(PHIPart.PMouse, ref ret);
            RandPart(PHIPart.PBrow, ref ret);
            RandPart(PHIPart.PBeard, ref ret);
            RandPart(PHIPart.PHelmet, ref ret);
            RandPart(PHIPart.PBody, ref ret);
            RandPart(PHIPart.PDecorate, ref ret);
            RandPart(PHIPart.PBG, ref ret);
            RandPart(PHIPart.PFrame, ref ret);
            RandPart(PHIPart.PFull, ref ret);
            return ret;

            void RandPart(PHIPart part, ref Dictionary<PHIPart, string> data)
            {
                List<PartInfo> parts = GetParts(part);
                var key = parts.Rand();
                if (key == null)
                    return;
                if (!RandUtil.Rand(key.Prop))
                {
                    data.Add(part, "");
                    return;
                }
                if (!data.ContainsKey(part))
                    data.Add(part, key.Name);
            }

            List<PartInfo> GetParts(PHIPart part)
            {
                var info = GetInfo(gender);
                //Face
                if (part == PHIPart.PBare) return info.GetSBare(tag);
                else if (part == PHIPart.PEye) return info.GetSEye(tag);
                else if (part == PHIPart.PHair) return info.GetSHair(tag);
                else if (part == PHIPart.PNose) return info.GetSNose(tag);
                else if (part == PHIPart.PMouse) return info.GetSMouse(tag);
                else if (part == PHIPart.PBrow) return info.GetSBrow(tag);
                else if (part == PHIPart.PBeard) return info.GetSBeard(tag);
                //其他
                else if (part == PHIPart.PBG) return info.GetSBG(tag);
                else if (part == PHIPart.PBody) return info.GetSBody(tag);
                else if (part == PHIPart.PDecorate) return info.GetSDecorate(tag);
                else if (part == PHIPart.PFrame) return info.GetSFrame(tag);
                else if (part == PHIPart.PHelmet) return info.GetSHelmet(tag);
                //完整
                else if (part == PHIPart.PFull) return info.GetSFull(tag);
                return new List<PartInfo>();
            }
        }
        public GenderInfo GetInfo(Gender gender)
        {
            GenderInfo info = new GenderInfo();
            if (gender == Gender.Female) info = Female;
            else if (gender == Gender.Male) info = Male;
            return info;
        }
        #endregion

        #region set
        //随机军团名称
        public string RandLegionName() => LegionName.Rand().GetName();
        #endregion
    }
    public class TDBaseCivil<TData, TConfig> : TDBaseGlobalConfig<TData, TConfig>
        where TData : TDBaseCivilData, new()
        where TConfig : TDBaseConfig<TData>, new()
    {
        public HashList<string> AllLastName { get; private set; } = new HashList<string>();
        public TDBaseCivil():base()
        {
            BaseGlobal.TDCivil = this;
        }
        public override void OnLuaParseEnd()
        {
            base.OnLuaParseEnd();
            AllLastName.Clear();
            foreach (var item in this)
            {
                AllLastName.AddRange(item.Value.LastNameKeys);
            }
        }
    }
}