//------------------------------------------------------------------------------
// TDBasePerson.cs
// Copyright 2019 2019/5/14 
// Created by CYM on 2019/5/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Person
{

    [Serializable]
    public class TDBasePersonData : TDBaseData, ITDPersonData
    {
        #region Config
        public string Theory { get; set; } = SysConst.STR_Inv;
        public string Civil { get; set; } = "Name_中原";
        //并非Key,而是翻译后的字符串
        public string FirstName { get; set; } = SysConst.STR_None;
        //并非Key,而是翻译后的字符串
        public string LastName { get; set; } = SysConst.STR_None;
        public Gender Gender { get; set; } = Gender.Male;
        public int Age { get; set; } = SysConst.INT_Inv;
        public AgeRange AgeRange { get; set; } = AgeRange.Adult;
        public Dictionary<PHIPart, string> HeadIcon { get; set; } = new Dictionary<PHIPart, string>();
        //小孩子的随机头像
        public Dictionary<PHIPart, string> ChildHeadIcon { get; set; } = new Dictionary<PHIPart, string>();
        //老人的随机头像
        public Dictionary<PHIPart, string> OldHeadIcon { get; set; } = new Dictionary<PHIPart, string>();
        #endregion

        #region prop
        public bool IsCelebrity { get; private set; } = false;
        public TDBaseCivilData CivilData { get; private set; }
        #endregion

        #region life
        protected Dictionary<PHIPart, string> LastPartIDs { get; private set; } = new Dictionary<PHIPart, string>();
        protected TDBaseCivilData GetCivilData(string id)
        {
            return BaseGlobal.TDCivil.Get<TDBaseCivilData>(id);
        }
        public override void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            base.OnBeAdded(selfMono, obj);
            CivilData = GetCivilData(Civil);
            AgeRange = GetAgeRange();
        }
        #endregion

        #region rand
        //根据输入信息随机人物
        public void DoRand(TDBaseCivilData civilData, Gender gender = Gender.Male, string lastName = null)
        {
            IsCelebrity = false;
            Gender = gender;
            Civil = civilData.TDID;
            CivilData = GetCivilData(Civil);
            CivilData.RandHeadIcon(Gender, SysConst.PTag_Normal, HeadIcon);
            if (lastName.IsInv())
            {
                LastName = CivilData.RandLastNameKey();
            }
            else
            {
                if (BaseLangMgr.AllLastNames.Contains(lastName)) LastName = lastName;
                else throw new Exception("没有这个姓氏:" + lastName);
            }
            if (FirstName.IsInv())
            {
                FirstName = CivilData.RandFirstNameKey(Gender);
            }
            OnRandPerson();
            OnSetPersonInfo();
        }
        //根据已有的(年龄)信息,完全捏造一个人物
        public void DoGenerate()
        {
            IsCelebrity = true;
            AgeRange = GetAgeRange();
            CivilData = GetCivilData(Civil);
            CivilData.RandHeadIcon(Gender, SysConst.PTag_Normal, HeadIcon);
            OnGenPerson();
            OnSetPersonInfo();
        }
        #endregion

        #region Set
        public void Growup()
        {
            Age++;
            AgeRange = GetAgeRange();
        }
        public void SetAge(int age)
        {
            Age = age;
            AgeRange = GetAgeRange();
        }
        #endregion

        #region Get
        public new string GetTDID()
        {
            if (TDID.IsInv()) return LastName + FirstName;
            return TDID;
        }
        public override string GetName()
        {
            if (CivilData == null)
                return "";
            return CivilData.GetPersonName(this);
        }
        public virtual string GetFirstName()
        {
            if (FirstName.IsInv()) return CustomName;
            return FirstName.GetName();
        }
        public virtual int GetStar() => 1;
        public Sprite GetStarIcon() => BaseGlobal.RsIcon.Get(SysConst.Prefix_Star + GetStar());
        public Sprite GetPSprite(PHIPart part)
        {
            Sprite ret;
            var tempHeadIcon = GetHeadIcon();
            if (tempHeadIcon.ContainsKey(part))
            {
                //获得原始Key
                string temp = tempHeadIcon[part];
                if (temp.IsInv()) return null;
                //身份(职业)加工(自定义处理)身份加工
                string sourcePartKey = OnProcessIdentity(part, temp);
                string addPartKey = sourcePartKey;
                //隐藏设置
                if (OnProcessHide(part)) return null;
                //加工年龄
                addPartKey = OnProcessAge(part, addPartKey);
                //获得图片
                ret = BaseGlobal.RsHead.Get(addPartKey, false);
                if (ret == null) ret = BaseGlobal.RsHead.Get(sourcePartKey, true);
                //记录ID
                if (ret != null)
                {
                    if (LastPartIDs.ContainsKey(part)) LastPartIDs[part] = sourcePartKey;
                    else LastPartIDs.Add(part, sourcePartKey);
                }
                else
                {
                    LastPartIDs.Remove(part);
                }

                return ret;
            }
            else
            {
                return null;
            }
        }
        public AgeRange GetAgeRange()
        {
            foreach (var item in GameConfig.Ins.AgeRangeData)
            {
                if (item.Value.IsIn(Age))
                    return item.Key;
            }
            return AgeRange.Old;
        }
        public string GetAgeStr(bool haveAgeStr = true)
        {
            return haveAgeStr ? "Text_Age".GetName() + ":" + Age.ToString() : Age.ToString();
        }
        public override string GetDesc(params object[] ps)
        {
            return base.GetDesc(ps);
        }
        //获得人物评价
        public string GetEvaluation()
        {
            if (IsCelebrity) return BaseLangMgr.Get("Text_历史名人");
            else return BaseLangMgr.Get("Text_无名鼠辈");
        }
        public virtual float GetDeathProp()
        {
            float deathAge = 65;
            float add = 0;
            float mul = 1;
            //正常的死亡几率
            float ageFac = 0.00005f;
            //名人的死亡几率降低
            if (IsCelebrity) mul = 0.5f;
            //人过60,死亡几率扩大
            if (Age > deathAge) add = 0.1f + (Age - deathAge) * 0.001f;
            //快速死亡
            if (SysConsole.Ins.IsFastPersonDeath) add = 0.5f;
            //计算最终的死亡概率
            float final = GameConfig.Ins.DeathProb[AgeRange] + (Age * ageFac) + add;
            return final * mul;
        }
        //获得死亡描述
        public string GetDeathStr()
        {
            if (AgeRange == AgeRange.Child) return BaseLangMgr.RandCategory("ChildDeath");
            else if (AgeRange == AgeRange.Adult)
            {
                if (IsMale) return BaseLangMgr.RandCategory("AdultDeathMan");
                else return BaseLangMgr.RandCategory("AdultDeathWomen");
            }
            else if (AgeRange == AgeRange.Middle) return BaseLangMgr.RandCategory("MiddleDeath");
            else if (AgeRange == AgeRange.Old) return BaseLangMgr.RandCategory("OldDeath");
            return "";
        }
        public GenderInfo GetGenderInfo() => CivilData.GetInfo(Gender);
        protected virtual Dictionary<PHIPart, string> GetHeadIcon()
        {
            if (AgeRange == AgeRange.Child && ChildHeadIcon.Count>0) return ChildHeadIcon;
            else if (AgeRange == AgeRange.Old && OldHeadIcon.Count > 0) return OldHeadIcon;
            return HeadIcon;
        }

        #endregion

        #region is
        public bool IsMale => Gender == Gender.Male;
        public bool IsFemale => Gender == Gender.Female;
        public bool IsWomenOrChildren => AgeRange == AgeRange.Child || IsFemale;
        #endregion

        #region virtual
        //自定义身份加工
        protected virtual string OnProcessIdentity(PHIPart part, string source)
        {
            return source;
        }
        //自定义隐藏(比如年轻的时候隐藏胡子)
        protected virtual bool OnProcessHide(PHIPart part)
        {
            if (part == PHIPart.PBeard)
            {
                if (AgeRange == AgeRange.Child)
                    return true;
                if (Gender == Gender.Female)
                    return true;
            }
            else if (part == PHIPart.PDecorate)
            {
                if (AgeRange == AgeRange.Child)
                    return true;
            }
            return false;
        }
        protected virtual string OnProcessAge(PHIPart part, string inputStr)
        {
            inputStr = inputStr + "_" + AgeRange.ToString();
            return inputStr;
        }
        protected virtual void OnRandPerson()
        {

        }
        protected virtual void OnGenPerson()
        {

        }
        protected virtual void OnSetPersonInfo()
        {
            if (GameConfig.Ins.IsRandChildHeadIcon)
            {
                CivilData.RandHeadIcon(Gender, AgeRange.Child.ToString(), ChildHeadIcon);
            }
            if (GameConfig.Ins.IsRandOldHeadIcon)
            {
                CivilData.RandHeadIcon(Gender, AgeRange.Old.ToString(), OldHeadIcon);
            }
        }
        #endregion
    }
}