//------------------------------------------------------------------------------
// BaseRelationMgr.cs
// Copyright 2019 2019/5/12 
// Created by CYM on 2019/5/12
// Owner: CYM
// 全局的单位关系管理器,用来管理所有的联盟,联姻,战争等关系
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CYM.Diplomacy
{
    public enum RelationShipType
    {
        Close, //亲密
        Friendly,//友好
        Neutral,//中立
        Disgust,//厌恶
        Feud,//世仇
    }
    public interface IWarfareData
    {
        long ID { get; set; }
        string TDID { get; set; }
        int WarDay { get; set; }
        float AttackersWarPoint { get; set; }
        float DefensersWarPoint { get; set; }
        void AddToConvence(BaseUnit chief, BaseUnit unit);
    }
    public interface IRelationMgr
    {
        #region event
        event Callback Callback_OnChange;
        event Callback<IHashList> Callback_OnChangeRelationBaseUnits;
        #endregion

        #region get
        IWarfareData GetBaseWarfareData(long id);
        int GetRelationShip(BaseUnit first, BaseUnit second);
        #endregion

        #region set
        void SetRelationShip(BaseUnit first, BaseUnit second, int val);
        void ChangeRelationShip(BaseUnit first, BaseUnit second, int val);
        #endregion

        #region is
        bool IsInWarfare(BaseUnit first);
        bool IsInWarfare(BaseUnit first, BaseUnit second);
        bool IsFriend(BaseUnit first, BaseUnit second);

        bool IsHaveAlliance(BaseUnit unit);
        bool IsHaveMarriage(BaseUnit unit);
        bool IsHaveVassal(BaseUnit unit);
        bool IsHaveSuzerain(BaseUnit unit);
        #endregion
    }
    public class BaseRelationMgr<TUnit, TWarData, TDBWarData> : BaseGFlowMgr, IRelationMgr
        where TUnit : BaseUnit
        where TWarData : BaseWarfareData<TUnit>, IWarfareData, new()
        where TDBWarData : DBBaseWar, new()
    {
        #region prop
        List<MultiKey<TUnit>> needRemoveArmisticeKeys = new List<MultiKey<TUnit>>();
        public int MaxRelationShip { get; private set; } = 100;
        public int MinRelationShip { get; private set; } = -100;
        #endregion

        #region Callback val
        public event Callback<TWarData, TUnit, TUnit> Callback_OnDeclareWar;
        public event Callback<TWarData, TUnit, TUnit> Callback_OnDeclarePeace;
        public event Callback<TWarData, TUnit, TUnit> Callback_OnDeclareChiefPeace;
        public event Callback<TWarData, TUnit, TUnit> Callback_OnDeclareSeparatePeace;
        public event Callback<TWarData, TUnit> Callback_OnAddToAttacker;
        public event Callback<TWarData, TUnit> Callback_OnAddToDefensers;
        public event Callback<TWarData, TUnit> Callback_OnRemoveWardata;
        public event Callback<TUnit, TUnit, int> Callback_OnChangeRelationShip;
        public event Callback<TUnit, TUnit, bool> Callback_OnChangeAlliance;
        public event Callback<TUnit, TUnit, bool> Callback_OnChangeMarriage;
        public event Callback<TUnit, TUnit, bool> Callback_OnChangeSubsidiary;
        public event Callback<TUnit, TUnit, int> Callback_OnChangeArmistice;
        public event Callback<HashList<TUnit>> Callback_OnChangeRelationUnits;
        public event Callback<IHashList> Callback_OnChangeRelationBaseUnits;
        public event Callback Callback_OnChange;
        #endregion

        #region Global Table
        public IDDicList<TWarData> Warfare { get; private set; } = new IDDicList<TWarData>();
        public MultiDic<TUnit, bool> Alliance { get; private set; } = new MultiDic<TUnit, bool>();
        public MultiDic<TUnit, bool> Marriage { get; private set; } = new MultiDic<TUnit, bool>();
        public MultiDic<TUnit, int> RelationShip { get; private set; } = new MultiDic<TUnit, int>();
        public MultiDic<TUnit, CD> Armistice { get; private set; } = new MultiDic<TUnit, CD>();
        public Dictionary<TUnit, HashList<TUnit>> Vassal { get; private set; } = new Dictionary<TUnit, HashList<TUnit>>();
        #endregion

        #region cache
        public Dictionary<TUnit, TUnit> CacheSuzerain { get; private set; } = new Dictionary<TUnit, TUnit>();
        public Dictionary<TUnit, HashList<TUnit>> CacheNationWarfare { get; private set; } = new Dictionary<TUnit, HashList<TUnit>>();
        public Dictionary<TUnit, HashList<TWarData>> CacheWarfare { get; private set; } = new Dictionary<TUnit, HashList<TWarData>>();
        public Dictionary<TUnit, HashList<TUnit>> CacheAlliance { get; private set; } = new Dictionary<TUnit, HashList<TUnit>>();
        public Dictionary<TUnit, HashList<TUnit>> CacheMarriage { get; private set; } = new Dictionary<TUnit, HashList<TUnit>>();
        public Dictionary<TUnit, HashList<TUnit>> CacheArmistice { get; private set; } = new Dictionary<TUnit, HashList<TUnit>>();
        #endregion

        #region life
        //停战期限
        protected virtual int ArmisticeCD => GameConfig.Ins.ArmisticeCD;
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            foreach (var item in GameConfig.Ins.RelationShip)
            {
                item.Value.Type = item.Key;
                if (item.Value.Max > MaxRelationShip)
                    MaxRelationShip = (int)item.Value.Max;
                if (item.Value.Min < MinRelationShip)
                    MinRelationShip = (int)item.Value.Min;
            }
        }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
            foreach (var item in Warfare)
                item.OnUpdateWarData();
        }
        #endregion

        #region Manual Update
        //更新战争
        protected void ManualUpdateWarfareData()
        {
            foreach (var item in Warfare)
            {
                item.OnUpdate();
                item.OnUpdateWarData();
            }
        }
        //更新停战协议
        protected void ManualUpdateArmistice()
        {
            needRemoveArmisticeKeys.Clear();
            foreach (var item in Armistice)
            {
                item.Value.Update();
                if (item.Value.IsOver())
                    needRemoveArmisticeKeys.Add(item.Key);
            }
            foreach (var item in needRemoveArmisticeKeys)
                Armistice.Remove(item);
        }
        #endregion

        #region ensure
        void EnsureDictionaryHashList(Dictionary<TUnit, HashList<TUnit>> data, TUnit key)
        {
            if (!data.ContainsKey(key))
                data.Add(key, new HashList<TUnit>());
        }
        void EnsureDictionaryHashList(Dictionary<TUnit, HashList<TWarData>> data, TUnit key)
        {
            if (!data.ContainsKey(key))
                data.Add(key, new HashList<TWarData>());
        }
        #endregion

        #region is have
        public bool IsHaveAlliance(BaseUnit unit) => GetAllianceCount(unit as TUnit) > 0;
        public bool IsHaveMarriage(BaseUnit unit) => GetMarriageCount(unit as TUnit) > 0;
        public bool IsHaveVassal(BaseUnit unit) => GetVassalCount(unit as TUnit) > 0;
        public bool IsHaveSuzerain(BaseUnit unit) => GetSuzerain(unit as TUnit) == null ? false : true;
        //是否有战争
        public bool IsHaveWarfare() => Warfare.Count > 0;
        #endregion

        #region is relation
        //是否为盟友或者联姻
        public bool IsFriend(TUnit first, TUnit second) => IsAlliance(first, second) || IsMarriage(first, second) || IsSubsidiary(first, second);
        public bool IsFriend(BaseUnit first, BaseUnit second) => IsFriend(first as TUnit, second as TUnit);
        //是否为联盟
        public bool IsAlliance(TUnit first, TUnit second) => Alliance.Get(first, second);
        //是否为联姻
        public bool IsMarriage(TUnit first, TUnit second) => Marriage.Get(first, second);
        //是否为附庸,first是否为second的附庸
        public bool IsVassal(TUnit first, TUnit second)
        {
            EnsureDictionaryHashList(Vassal, second);
            return Vassal[second].Contains(first);
        }
        public bool IsVassal(TUnit first) => CacheSuzerain.ContainsKey(first);
        //是否为宗主：first为second的宗主国
        public bool IsSuzerain(TUnit first, TUnit second)
        {
            EnsureDictionaryHashList(Vassal, first);
            return Vassal[first].Contains(second);
        }
        //是否为宗主国
        public bool IsSuzerain(TUnit first)
        {
            EnsureDictionaryHashList(Vassal, first);
            return Vassal[first].Count > 0;
        }
        //是否为臣属关系
        public bool IsSubsidiary(TUnit first, TUnit second) => IsVassal(first, second) || IsSuzerain(first, second);
        //是否处于停战状态
        public bool IsArmistice(TUnit first, TUnit second)
        {
            var ret = Armistice.Get(first, second);
            if (ret == null) return false;
            if (ret.IsOver()) return false;
            return true;
        }
        //检测是否处于战争状态
        public bool IsInWarfare(TUnit first, TUnit second)
        {
            if (first.BaseConfig.IsWild != second.BaseConfig.IsWild) return true;
            if (!CacheNationWarfare.ContainsKey(first)) return false;
            if (CacheNationWarfare[first].Contains(second)) return true;
            return false;
        }
        public bool IsInWarfare(BaseUnit first, BaseUnit second)
        {
            return IsInWarfare(first as TUnit, second as TUnit);
        }
        #endregion

        #region is in
        public bool IsInAlongside(TUnit first, TUnit second)
        {
            var datas = GetWarfareData(first, second);
            if (datas != null)
                return true;
            return false;
        }
        //是否处于侵略战争
        public bool IsInAggressive(TUnit first)
        {
            var datas = GetWarfareDatas(first);
            foreach (var item in datas)
            {
                if (item.IsInAttackers(first))
                    return true;
            }
            return false;
        }
        //是否处于防御战争
        public bool IsInDefensive(TUnit first)
        {
            var datas = GetWarfareDatas(first);
            foreach (var item in datas)
            {
                if (item.IsInDefensers(first))
                    return true;
            }
            return false;
        }
        public bool IsInWarfare(TUnit first) => GetInWarfare(first).Count > 0;
        public bool IsInWarfare(BaseUnit first) => GetInWarfare(first as TUnit).Count > 0;
        #endregion

        #region is
        //外交关系是否满员
        public bool IsRelationFull(TUnit first, float max) => GetFriends(first).Count >= max;
        //是否有权占领
        public bool IsCanOccupy(TUnit first, TWarData warData) => warData.IsCanOccupy(first);
        public bool IsGoodRelationShip(TUnit first, TUnit second)
        {
            return GetRelationShip(first, second) >= 0;
        }
        public bool IsBadRelationShip(TUnit first, TUnit second)
        {
            return GetRelationShip(first, second) < 0;
        }
        #endregion

        #region set
        //清空一个国家的所有的外交关系
        public virtual void ClearRelation(TUnit unit, bool quitAllWar = true, bool relationShip = true, bool armistice = true)
        {
            if (quitAllWar) QuitAllWar(unit);
            var tempAlliance = GetAlliance(unit);
            foreach (var item in tempAlliance.ToArray())
                SetAlliance(unit, item, false);

            if (armistice)
            {
                var tempArmistice = GetArmistice(unit);
                foreach (var item in tempArmistice.ToArray())
                    SetArmistice(unit, item, 0);
            }

            var tempMarraige = GetMarriage(unit);
            foreach (var item in tempMarraige.ToArray())
                SetMarriage(unit, item, false);

            if (relationShip)
            {
                Dictionary<TUnit, int> relationData = new Dictionary<TUnit, int>(GetRelationShip(unit));
                foreach (var item in relationData)
                    SetRelationShip(unit, item.Key, 0);
            }

            List<TUnit> vassalData = new List<TUnit>(GetVassal(unit));
            foreach (var item in vassalData)
                SetSubsidiary(unit, item, false);

            var suzerian = GetSuzerain(unit);
            if (suzerian != null) SetSubsidiary(suzerian, unit, false);

        }
        //退出所有战争,如果自己是盟主,则结束战争
        public void QuitAllWar(TUnit unit)
        {
            var tempWar = GetWarfareDatas(unit);
            tempWar.ForSafe((item) =>
            {
                if (item.IsChief(unit)) RemoveWar(item, false);
                else RemoveFromWar(item, unit, false);
            });
        }
        //清空2个国家之间所有的关系
        public virtual void EmptyAllRelation(TUnit first, TUnit target)
        {
            SetAlliance(first, target, false);
            SetArmistice(first, target, 0);
            SetMarriage(first, target, false);
            SetSubsidiary(first, target, false);
            SetSubsidiary(target, first, false);
        }
        public void SetAlliance(TUnit first, TUnit second, bool b)
        {
            if (first == null || second == null) return;
            EnsureDictionaryHashList(CacheAlliance, first);
            EnsureDictionaryHashList(CacheAlliance, second);

            if (b)
            {
                Alliance.Change(first, second, b);
                CacheAlliance[first].Add(second);
                CacheAlliance[second].Add(first);
            }
            else
            {
                Alliance.Remove(first, second);
                CacheAlliance[first].Remove(second);
                CacheAlliance[second].Remove(first);
            }
            if (BaseGlobal.IsUnReadData)
            {
                _TriggerChangeRelation(first, second);
                OnChangeAlliance(first, second, b);
            }
        }
        public void SetMarriage(TUnit first, TUnit second, bool b)
        {
            if (first == null || second == null) return;
            EnsureDictionaryHashList(CacheMarriage, first);
            EnsureDictionaryHashList(CacheMarriage, second);

            if (b)
            {
                Marriage.Change(first, second, b);
                CacheMarriage[first].Add(second);
                CacheMarriage[second].Add(first);
            }
            else
            {
                Marriage.Remove(first, second);
                CacheMarriage[first].Remove(second);
                CacheMarriage[second].Remove(first);
            }
            if (BaseGlobal.IsUnReadData)
            {
                _TriggerChangeRelation(first, second);
                OnChangeMarriage(first, second, b);
            }
        }
        //结束臣属关系,first:宗主国,second:附庸
        public void SetSubsidiary(TUnit first, TUnit second, bool b)
        {
            if (first == null || second == null) return;
            if (b) ClearRelation(second, true, false, false);
            EnsureDictionaryHashList(Vassal, first);
            EnsureDictionaryHashList(Vassal, second);

            if (b)
            {
                Vassal[first].Add(second);
                CacheSuzerain[second] = first;
            }
            else
            {
                Vassal[first].Remove(second);
                CacheSuzerain.Remove(second);
            }

            if (BaseGlobal.IsUnReadData)
            {
                _TriggerChangeRelation(first, second);
                OnChangeSubsidiary(first, second, b);
            }
        }
        public void SetArmistice(TUnit first, TUnit second, int count)
        {
            if (first == null || second == null) return;
            EnsureDictionaryHashList(CacheArmistice, first);
            EnsureDictionaryHashList(CacheArmistice, second);

            var preval = GetArmistice(first, second);
            if (preval != null)
            {
                if (preval.CurCount >= count)
                    return;
            }

            //添加停战缓存
            if (count > 0)
            {
                CD cd = new CD(count);
                Armistice.Change(first, second, cd);
                CacheArmistice[first].Add(second);
                CacheArmistice[second].Add(first);
            }
            else
            {
                Armistice.Remove(first, second);
                CacheArmistice[first].Remove(second);
                CacheArmistice[second].Remove(first);
            }
            if (BaseGlobal.IsUnReadData)
            {
                _TriggerChangeRelation(first, second);
                OnChangeArmistice(first, second, count);
            }
        }
        public void SetRelationShip(BaseUnit first, BaseUnit second, int val)
        {
            if (first == null || second == null) return;
            val = Mathf.Clamp(val, MinRelationShip, MaxRelationShip);
            RelationShip.Change(first as TUnit, second as TUnit, val);
            if (BaseGlobal.IsUnReadData)
            {
                OnChangeRelationShip(first as TUnit, second as TUnit, val);
            }
        }
        public void ChangeRelationShip(BaseUnit first, BaseUnit second, int val)
        {
            if (first == second) return;
            var curVal = RelationShip.Get(first as TUnit, second as TUnit);
            RelationShip.Change(first as TUnit, second as TUnit, Mathf.Clamp(curVal + val, MinRelationShip, MaxRelationShip));
            if (BaseGlobal.IsUnReadData)
            {
                OnChangeRelationShip(first as TUnit, second as TUnit, val);
            }
        }
        //宣战
        public TWarData DeclareWar(TUnit first, TUnit second)
        {
            if (first == second) return null;
            if (first == null || second == null) return null;
            if (IsInWarfare(first, second)) return null;
            if (IsVassal(first) && !IsVassal(first, second)) return null;
            if (IsVassal(second)) second = GetSuzerain(second);
            //二次判断
            if (IsInWarfare(first, second)) return null;
            TWarData warfareData = new TWarData();
            warfareData.ID = IDUtil.Gen();
            warfareData.TDID = first.BaseConfig.GetName() + " vs " + second.BaseConfig.GetName();
            warfareData.IsIndependent = IsSubsidiary(first, second);
            Warfare.Add(warfareData);
            //结束2国之间所有的外交关系
            EmptyAllRelation(first, second);
            AddToAttacker(warfareData, first);
            foreach (var item in GetVassal(first))
                AddToAttacker(warfareData, item);

            AddToDefenser(warfareData, second);
            foreach (var item in GetVassal(second))
                AddToDefenser(warfareData, item);

            if (BaseGlobal.IsUnReadData)
            {
                OnDecalarWar(warfareData, first, second);
                _TriggerChangeRelation(first, second);
            }
            return warfareData;
        }
        //宣布和平(单独议和/盟主议和)
        public TWarData DeclarePeace(TUnit first, TUnit second)
        {
            if (first == second) return null;
            if (first == null || second == null) return null;
            if (!IsInWarfare(first, second)) return null;
            TWarData warfareData = GetChiefWarfareData(first, second);
            //盟主议和
            if (warfareData != null)
            {
                RemoveWar(warfareData);
                Callback_OnDeclareChiefPeace?.Invoke(warfareData, first, second);
            }
            //单独议和
            else
            {
                warfareData = GetWarfareData(first, second);
                var converce = GetWarfareConverce(first, second);
                RemoveFromWar(warfareData, converce);
                Callback_OnDeclareSeparatePeace?.Invoke(warfareData, first, second);
            }
            OnDeclarePeace(warfareData, first, second);
            _TriggerChangeRelation(first, second);
            return warfareData;
        }
        //被召集进入战争
        public void ConvenceToWar(TWarData warData, TUnit mainNation, TUnit nation)
        {
            if (!warData.IsChief(mainNation))
            {
                CLog.Error("错误! " + mainNation + " 不是Chief");
                return;
            }
            if (warData.IsInAttackers(mainNation))
            {
                foreach (var item in warData.Defensers)
                {
                    EmptyAllRelation(item, nation);
                }
                AddToAttacker(warData, nation);
            }
            else if (warData.IsInDefensers(mainNation))
            {
                foreach (var item in warData.Attackers)
                {
                    EmptyAllRelation(item, nation);
                }
                AddToDefenser(warData, nation);
            }
        }
        private void AddToAttacker(TWarData warData, TUnit nation)
        {
            warData.Attackers.Add(nation);
            EnsureDictionaryHashList(CacheNationWarfare, nation);
            EnsureDictionaryHashList(CacheWarfare, nation);
            CacheWarfare[nation].Add(warData);
            foreach (var item in warData.Defensers)
            {
                EnsureDictionaryHashList(CacheNationWarfare, item);
                CacheNationWarfare[item].Add(nation);
                CacheNationWarfare[nation].Add(item);
            }

            if (BaseGlobal.IsUnReadData)
            {
                HashList<TUnit> temp = new HashList<TUnit>();
                temp.Add(nation);
                temp.AddRange(warData.Defensers);
                OnChangeRelation(temp);
                OnAddToAttacker(warData, nation);
            }
        }
        private void AddToDefenser(TWarData warData, TUnit nation)
        {
            warData.Defensers.Add(nation);
            EnsureDictionaryHashList(CacheNationWarfare, nation);
            EnsureDictionaryHashList(CacheWarfare, nation);
            CacheWarfare[nation].Add(warData);
            foreach (var item in warData.Attackers)
            {
                EnsureDictionaryHashList(CacheNationWarfare, item);
                CacheNationWarfare[item].Add(nation);
                CacheNationWarfare[nation].Add(item);
            }

            if (BaseGlobal.IsUnReadData)
            {
                HashList<TUnit> temp = new HashList<TUnit>();
                temp.Add(nation);
                temp.AddRange(warData.Attackers);
                OnChangeRelation(temp);
                OnAddToDefensers(warData, nation);
            }
        }
        void RemoveFromWar(TWarData warData, TUnit nation, bool isSetArmistice = true)
        {
            EnsureDictionaryHashList(CacheNationWarfare, nation);
            EnsureDictionaryHashList(CacheWarfare, nation);
            CacheWarfare[nation].Remove(warData);
            bool isInAttacker = warData.IsInAttackers(nation);
            if (isInAttacker)
            {
                foreach (var item in warData.Defensers)
                {
                    EnsureDictionaryHashList(CacheNationWarfare, item);
                    CacheNationWarfare[item].Remove(nation);
                    CacheNationWarfare[nation].Remove(item);
                    if (isSetArmistice)
                        SetArmistice(item, nation, ArmisticeCD);
                }
            }
            else
            {
                foreach (var item in warData.Attackers)
                {
                    EnsureDictionaryHashList(CacheNationWarfare, item);
                    CacheNationWarfare[item].Remove(nation);
                    CacheNationWarfare[nation].Remove(item);
                    if (isSetArmistice)
                        SetArmistice(item, nation, ArmisticeCD);
                }
            }
            warData.Remove(nation);

            if (BaseGlobal.IsUnReadData)
            {
                HashList<TUnit> temp = new HashList<TUnit>();
                temp.Add(nation);
                if (isInAttacker) temp.AddRange(warData.Defensers);
                else temp.AddRange(warData.Attackers);
                OnChangeRelation(temp);
                OnRemoveFromWar(warData, nation);
            }
        }
        protected void RemoveWar(TWarData warData, bool isSetArmistice = true)
        {
            foreach (var item in warData.Defensers)
            {
                EnsureDictionaryHashList(CacheWarfare, item);
                CacheWarfare[item].Remove(warData);
                foreach (var item2 in warData.Attackers)
                {
                    EnsureDictionaryHashList(CacheNationWarfare, item);
                    CacheNationWarfare[item].Remove(item2);
                }
            }

            foreach (var item in warData.Attackers)
            {
                EnsureDictionaryHashList(CacheWarfare, item);
                CacheWarfare[item].Remove(warData);
                foreach (var item2 in warData.Defensers)
                {
                    EnsureDictionaryHashList(CacheNationWarfare, item);
                    CacheNationWarfare[item].Remove(item2);
                }
            }

            if (isSetArmistice)
            {
                foreach (var attacker in warData.Attackers)
                {
                    foreach (var defender in warData.Defensers)
                    {
                        SetArmistice(attacker, defender, ArmisticeCD);
                    }
                }
            }

            Warfare.Remove(warData);

            if (BaseGlobal.IsUnReadData)
            {
                HashList<TUnit> temp = new HashList<TUnit>();
                temp.AddRange(warData.Defensers);
                temp.AddRange(warData.Attackers);
                OnChangeRelation(temp);
                foreach (var item in temp)
                    OnRemoveFromWar(warData, item);
            }
        }
        public void AddWarPoint(TUnit attacker, TUnit defender, float value)
        {
            var data = GetWarfareData(attacker, defender);
            if (data == null)
            {
                return;
            }
            data.AddWarPoint(attacker, value);
        }
        #endregion

        #region get relation ship
        //获得外交关系
        public int GetRelationShip(TUnit first, TUnit second) => RelationShip.Get(first, second);
        public int GetRelationShip(BaseUnit first, BaseUnit second) => GetRelationShip(first as TUnit, second as TUnit);
        public Dictionary<TUnit, int> GetRelationShip(TUnit first) => RelationShip[first];
        public RelationShip GetRelationShipType(int val)
        {
            var data = GameConfig.Ins.RelationShip;
            var Max = data.First().Value.Max;
            var Min = data.Last().Value.Min;
            foreach (var item in data)
            {
                if (item.Value.Min < Min) Min = item.Value.Min;
                if (item.Value.Max > Max) Max = item.Value.Max;
                if (item.Value.IsIn(val))
                    return item.Value;
            }
            if (val > Max) return data.First().Value;
            else if (val < Min) return data.Last().Value;
            return data.LastOrDefault().Value;
        }
        public RelationShipType GetRelationShipType(TUnit first, TUnit second)
        {
            return GetRelationShipType(GetRelationShip(first, second)).Type;
        }
        #endregion

        #region get count
        public int GetRelationCount(TUnit first)
        {
            EnsureDictionaryHashList(CacheAlliance, first);
            EnsureDictionaryHashList(CacheMarriage, first);
            EnsureDictionaryHashList(Vassal, first);
            int ret = 0;
            ret += CacheAlliance[first].Count;
            ret += CacheMarriage[first].Count;
            ret += Vassal[first].Count;
            if (IsHaveSuzerain(first))
                ret++;
            return ret;
        }
        public int GetMarriageCount(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            return Marriage[first].Keys.Count;
        }
        public int GetVassalCount(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            EnsureDictionaryHashList(Vassal, first);
            return Vassal[first].Count;
        }
        public int GetAllianceCount(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            return Alliance[first].Keys.Count;
        }
        public int GetWarfareCount() => Warfare.Count;
        #endregion

        #region get relation
        public List<TUnit> GetWarFriend(TUnit first)
        {
            List<TUnit> ret = new List<TUnit>();
            var tempAlliance = GetAlliance(first);
            var tempVassal = GetVassal(first);
            var tempSuzerain = GetSuzerain(first);
            if (tempAlliance != null) ret.AddRange(tempAlliance);
            if (tempVassal != null) ret.AddRange(tempVassal);
            if (tempSuzerain != null) ret.Add(tempSuzerain);
            return ret;
        }
        public List<TUnit> GetAlliance(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            EnsureDictionaryHashList(CacheAlliance, first);
            return CacheAlliance[first];
        }
        public List<TUnit> GetMarriage(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            EnsureDictionaryHashList(CacheMarriage, first);
            return CacheMarriage[first];
        }
        public List<TUnit> GetVassal(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            EnsureDictionaryHashList(Vassal, first);
            return Vassal[first];
        }
        public TUnit GetSuzerain(TUnit unit)
        {
            if (unit == null) throw new NullReferenceException();
            if (CacheSuzerain.ContainsKey(unit)) return CacheSuzerain[unit];
            return null;
        }
        public List<TUnit> GetArmistice(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            EnsureDictionaryHashList(CacheArmistice, first);
            return CacheArmistice[first];
        }
        //获得所有友好关系
        public HashList<TUnit> GetFriends(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            HashList<TUnit> ret = new HashList<TUnit>();
            ret.AddRange(Alliance[first].Keys);
            ret.AddRange(Marriage[first].Keys);
            if (Vassal.ContainsKey(first)) ret.AddRange(Vassal[first]);
            if (CacheSuzerain.ContainsKey(first)) ret.Add(CacheSuzerain[first]);
            return ret;
        }
        public List<TUnit> GetInWarfare(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            EnsureDictionaryHashList(CacheNationWarfare, first);
            return CacheNationWarfare[first];
        }
        #endregion

        #region get misc
        public CD GetArmistice(TUnit first, TUnit second) => Armistice.Get(first, second);
        #endregion

        #region get warfare
        public TWarData GetWarfareData(TUnit first, TUnit second)
        {
            TWarData ret = null;
            if (first == null || second == null) return ret;
            foreach (var item in Warfare)
            {
                if (item.IsIn(first) &&
                    item.IsIn(second))
                {
                    ret = item;
                }
            }
            return ret;
        }
        public TWarData GetWarfareData(long id) => Warfare.Get(id);
        public IWarfareData GetBaseWarfareData(long id)
        {
            return Warfare.Get(id);
        }
        //获得战争,要求first和second都是盟主
        public TWarData GetChiefWarfareData(TUnit first, TUnit second)
        {
            TWarData ret = null;
            foreach (var item in Warfare)
            {
                if (item.IsChief(first) &&
                    item.IsChief(second))
                {
                    ret = item;
                }
            }
            return ret;
        }
        //获得某个国家所处的战争
        public List<TWarData> GetWarfareDatas(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            EnsureDictionaryHashList(CacheWarfare, first);
            return CacheWarfare[first];
        }
        //获得某个国家所处的战争,first必须作为战争的盟主
        public List<TWarData> GetChiefWarfareDatas(TUnit first)
        {
            if (first == null) throw new NullReferenceException();
            List<TWarData> ret = new List<TWarData>();
            foreach (var item in GetWarfareDatas(first))
            {
                if (item.IsIn(first) && item.IsChief(first))
                {
                    ret.Add(item);
                }
            }
            return ret;
        }
        //获得first所处的所有战争,first必须是作为盟主,并且second不处于此战争可以被征召,并且不是独立战争
        public List<TWarData> GetConverceWarfareDatas(TUnit first, TUnit second)
        {
            if (first == null || second == null) throw new NullReferenceException();
            List<TWarData> ret = new List<TWarData>();
            foreach (var item in Warfare)
            {
                if (
                    item.IsIn(first) &&
                    item.IsChief(first) &&
                    !item.IsIn(second) &&
                    !item.IsIndependent
                    )
                {
                    ret.Add(item);
                }
            }
            return ret;
        }
        //处于同一个战争内
        public TWarData GetAlongsideWarfareData(TUnit first, TUnit second)
        {
            if (first == null || second == null) throw new NullReferenceException();
            TWarData ret = null;
            foreach (var item in Warfare)
            {
                if (item.IsInAttackers(first) &&
                    item.IsInAttackers(second))
                {
                    ret = item;
                }
                else if (
                    item.IsInDefensers(first) &&
                    item.IsInDefensers(second))
                {
                    ret = item;
                }
            }
            return ret;
        }
        #endregion

        #region get warfare unit
        //获得盟主
        public TUnit GetWarfareChief(TUnit first, TUnit second)
        {
            if (first == null || second == null) throw new NullReferenceException();
            BaseWarfareData<TUnit> data = GetWarfareData(first, second);
            if (data == null)
            {
                CLog.Error("first和second之间没有战争");
                return null;
            }
            if (data.IsChief(first)) return first;
            if (data.IsChief(second)) return second;
            return null;
        }
        //获得非盟主的一方
        public TUnit GetWarfareConverce(TUnit first, TUnit second)
        {
            if (first == null || second == null) throw new NullReferenceException();
            TWarData data = GetWarfareData(first, second);
            if (data == null)
            {
                CLog.Error("first和second之间没有战争");
                return null;
            }
            if (!data.IsChief(first)) return first;
            if (!data.IsChief(second)) return second;
            return null;
        }
        #endregion

        #region Relation Color
        public static readonly Color NormalRelationColor = Color.white;
        public static readonly Color SelfColor = Color.green;
        public static readonly Color FriendColor = Color.cyan;
        public static readonly Color WarfareColor = new Color(0.5f, 0.1f, 0.1f, 1.0f);
        public Color GetRelationColor(TUnit other)
        {
            if (other == null)
                return Color.white;
            var localPlayer = BaseGlobal.Player;
            if (localPlayer == null)
                return NormalRelationColor;
            if (other.BaseOwner == null)
                return NormalRelationColor;
            if (other.IsPlayer())
                return SelfColor;
            else if (IsFriend(other, localPlayer as TUnit))
                return FriendColor;
            else if (IsInWarfare(other, localPlayer as TUnit))
                return WarfareColor;
            else
                return NormalRelationColor;
        }
        #endregion

        #region db
        public void SaveWarDBData(List<TDBWarData> ret) 
        {
            ret.Clear();
            foreach (var item in Warfare)
            {
                TDBWarData data = new TDBWarData();
                data.ID = item.ID;
                data.TDID = item.TDID;
                data.WarDay = item.WarDay;
                data.AttackersWarPoint = item.AttackersWarPoint;
                data.DefensersWarPoint = item.DefensersWarPoint;
                item.Attackers.ForEach(x => data.Attackers.Add(x.BaseConfig.TDID));
                item.Defensers.ForEach(x => data.Defensers.Add(x.BaseConfig.TDID));
                item.AllowLand.ForEach(x => data.AllowLand.Add(x.BaseConfig.TDID));
                ret.Add(data);
            }
        }
        public void LoadWarDBData(List<TDBWarData> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                TWarData war = new TWarData();
                war.ID = item.ID;
                war.TDID = item.TDID;
                war.WarDay = item.WarDay;
                war.AttackersWarPoint = item.AttackersWarPoint;
                war.DefensersWarPoint = item.DefensersWarPoint;
                Warfare.Add(war);

                foreach (var temp in item.Defensers)
                    AddToDefenser(war, GetEntity<TUnit>(temp));
                foreach (var temp in item.Attackers)
                    AddToAttacker(war, GetEntity<TUnit>(temp));
                item.AllowLand.ForEach(x => war.AllowLand.Add(GetEntity<TUnit>(x)));
            }
        }
        public void SaveRelationShipDBData(MultiDic<string, int> ret)
        {
            ret.Clear();
            RelationShip.ForEach((x, y) => ret.Add(new MultiKey<string>(x.Item1.TDID, x.Item2.TDID), y));
        }
        public void LoadRelationShipDBData(MultiDic<string, int> data)
        {
            data.ForEach((x, y) => { SetRelationShip(GetEntity<TUnit>(x.Item1), GetEntity<TUnit>(x.Item2), y); });
        }
        public void SaveAllianceDBData(MultiDic<string, bool> ret)
        {
            ret.Clear();
            Alliance.ForEach((x, y) => ret.Add(new MultiKey<string>(x.Item1.TDID, x.Item2.TDID), y));
        }
        public void LoadAllianceDBData(MultiDic<string, bool> data)
        {
            data.ForEach((x, y) => { SetAlliance(GetEntity<TUnit>(x.Item1), GetEntity<TUnit>(x.Item2), y); });
        }
        public void SaveMarriageDBData(MultiDic<string, bool> ret)
        {
            ret.Clear();
            Marriage.ForEach((x, y) => ret.Add(new MultiKey<string>(x.Item1.TDID, x.Item2.TDID), y));
        }
        public void LoadMarriageDBData(MultiDic<string, bool> data)
        {
            data.ForEach((x, y) => { SetMarriage(GetEntity<TUnit>(x.Item1), GetEntity<TUnit>(x.Item2), y); });
        }
        public void SaveArmisticeDBData(MultiDic<string, int> ret)
        {
            ret.Clear();
            Armistice.ForEach((x, y) => ret.Add(new MultiKey<string>(x.Item1.TDID, x.Item2.TDID), (int)y.GetRemainder()));
        }
        public void LoadArmisticeDBData(MultiDic<string, int> data)
        {
            data.ForEach((x, y) => { SetArmistice(GetEntity<TUnit>(x.Item1), GetEntity<TUnit>(x.Item2), y); });
        }
        public void SaveVassalDBData(Dictionary<string, HashList<string>> ret)
        {
            ret.Clear();
            Vassal.ForEach((x, y) =>
            {
                ret[x.TDID] = new HashList<string>(y.Select(item => item.TDID).ToList());
            });
        }
        public void LoadVassalDBData(Dictionary<string, HashList<string>> data)
        {
            data.ForEach((x, y) =>
            {
                foreach (var item in y)
                    SetSubsidiary(GetEntity<TUnit>(x), GetEntity<TUnit>(item), true);
            });
        }
        #endregion

        #region Callback
        private void _TriggerChangeRelation(TUnit first, TUnit second)
        {
            HashList<TUnit> temp = new HashList<TUnit>();
            temp.Add(first);
            temp.Add(second);
            OnChangeRelation(temp);
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            //清空数据
            Warfare.Clear();
            Alliance.Clear();
            Marriage.Clear();
            Armistice.Clear();
            RelationShip.Clear();
            Vassal.Clear();
            //清空缓存
            CacheSuzerain.Clear();
            CacheNationWarfare.Clear();
            CacheWarfare.Clear();
            CacheAlliance.Clear();
            CacheMarriage.Clear();
            CacheArmistice.Clear();
        }
        protected virtual void OnChangeRelation(HashList<TUnit> units)
        {
            Callback_OnChangeRelationBaseUnits?.Invoke(units);
            Callback_OnChangeRelationUnits?.Invoke(units);
            Callback_OnChange?.Invoke();
            foreach (var starter in units)
            {
                foreach (var recive in units)
                {
                    if (starter == recive) continue;
                    starter.DipMgr.OnChangeRelation(recive);
                }
            }
        }
        protected virtual void OnChangeRelationShip(TUnit first, TUnit second, int relShip)
        {
            Callback_OnChangeRelationShip?.Invoke(first, second, relShip);
            first?.DipMgr.OnChangeRelationShip(second, relShip);
            second?.DipMgr.OnChangeRelationShip(first, relShip);
        }
        protected virtual void OnChangeAlliance(TUnit first, TUnit second, bool b)
        {
            Callback_OnChangeAlliance?.Invoke(first, second, b);
            first?.DipMgr.OnChangeAlliance(second, b);
            second?.DipMgr.OnChangeAlliance(first, b);
        }
        protected virtual void OnChangeMarriage(TUnit first, TUnit second, bool b)
        {
            Callback_OnChangeMarriage?.Invoke(first, second, b);
            first?.DipMgr.OnChangeMarriage(second, b);
            second?.DipMgr.OnChangeMarriage(first, b);
        }
        protected virtual void OnChangeSubsidiary(TUnit first, TUnit second, bool b)
        {
            Callback_OnChangeSubsidiary?.Invoke(first, second, b);
            first?.DipMgr.OnChangeSubsidiary(second, b);
            second?.DipMgr.OnChangeSubsidiary(first, b);
        }
        protected virtual void OnChangeArmistice(TUnit first, TUnit second, int count)
        {
            Callback_OnChangeArmistice?.Invoke(first, second, count);
            first?.DipMgr.OnChangeArmistice(second, count);
            second?.DipMgr.OnChangeArmistice(first, count);
        }
        protected virtual void OnAddToAttacker(TWarData warData, TUnit unit)
        {
            Callback_OnAddToAttacker?.Invoke(warData, unit);
            unit?.DipMgr.OnAddToAttacker(warData);
        }
        protected virtual void OnAddToDefensers(TWarData warData, TUnit unit)
        {
            Callback_OnAddToDefensers?.Invoke(warData, unit);
            unit?.DipMgr.OnAddToDefensers(warData);
        }
        protected virtual void OnRemoveFromWar(TWarData warData, TUnit unit)
        {
            Callback_OnRemoveWardata?.Invoke(warData, unit);
            unit?.DipMgr.OnRemoveFromWar(warData);
        }
        protected virtual void OnDecalarWar(TWarData warData, TUnit first, TUnit second)
        {
            Callback_OnDeclareWar?.Invoke(warData, first, second);
            first?.DipMgr.OnDecalarWar(warData, second);
            second?.DipMgr.OnBeDecalarWar(warData, first);
        }
        protected virtual void OnDeclarePeace(TWarData warData, TUnit first, TUnit second)
        {
            Callback_OnDeclarePeace?.Invoke(warData, first, second);
            first?.DipMgr.OnDeclarePeace(warData, second);
            second?.DipMgr.OnDeclarePeace(warData, first);
        }
        #endregion
    }


}