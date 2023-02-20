//------------------------------------------------------------------------------
// BaseDipMgr.cs
// Copyright 2023 2023/1/20 
// Created by CYM on 2023/1/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CYM.Diplomacy
{
    public interface IDipMgr
    {
        #region event
        event Callback<BaseUnit> Callback_OnChangeRelation;
        event Callback<BaseUnit, int> Callback_OnChangeRelationShip;
        event Callback<BaseUnit, bool> Callback_OnChangeAlliance;
        event Callback<BaseUnit, bool> Callback_OnChangeMarriage;
        event Callback<BaseUnit, bool> Callback_OnChangeSubsidiary;
        event Callback<BaseUnit, int> Callback_OnChangeArmistice;
        event Callback<object> Callback_OnAddToAttacker;
        event Callback<object> Callback_OnAddToDefensers;
        event Callback<object> Callback_OnRemoveFromWar;
        event Callback<object, BaseUnit> Callback_OnDecalarWar;
        event Callback<object, BaseUnit> Callback_OnBeDecalarWar;
        event Callback<TDBaseData, bool> Callback_OnBeAccept;
        event Callback<TDBaseData, bool> Callback_OnAccept;
        #endregion

        #region set
        void CalcNeighbor();
        #endregion

        #region Callback
        void OnChangeRelation(BaseUnit other);
        void OnChangeRelationShip(BaseUnit other, int relShip);
        void OnChangeAlliance(BaseUnit other, bool b);
        void OnChangeMarriage(BaseUnit other, bool b);
        void OnChangeSubsidiary(BaseUnit other, bool b);
        void OnChangeArmistice(BaseUnit other, int count);
        void OnAddToAttacker(object warData);
        void OnAddToDefensers(object warData);
        void OnRemoveFromWar(object warData);
        void OnDecalarWar(object warData, BaseUnit other);
        void OnBeDecalarWar(object warData, BaseUnit caster);
        void OnDeclarePeace(object warData, BaseUnit other);
        void OnBeAccept(TDBaseData data, bool b);
        void OnAccept(TDBaseData data, bool b);
        #endregion
    }
    public class BaseDipMgr<TUnit> : BaseUFlowMgr<TUnit>, IDipMgr
        where TUnit : BaseUnit
    {
        #region event
        public event Callback<BaseUnit> Callback_OnChangeRelation;
        public event Callback<BaseUnit, int> Callback_OnChangeRelationShip;
        public event Callback<BaseUnit, bool> Callback_OnChangeAlliance;
        public event Callback<BaseUnit, bool> Callback_OnChangeMarriage;
        public event Callback<BaseUnit, bool> Callback_OnChangeSubsidiary;
        public event Callback<BaseUnit, int> Callback_OnChangeArmistice;
        public event Callback<object> Callback_OnAddToAttacker;
        public event Callback<object> Callback_OnAddToDefensers;
        public event Callback<object> Callback_OnRemoveFromWar;
        public event Callback<object, BaseUnit> Callback_OnDecalarWar;
        public event Callback<object, BaseUnit> Callback_OnBeDecalarWar;
        public event Callback<TDBaseData, bool> Callback_OnBeAccept;
        public event Callback<TDBaseData, bool> Callback_OnAccept;
        #endregion

        #region prop
        List<TUnit> clearProvocative = new List<TUnit>();
        IRelationMgr RelationMgr => BaseGlobal.RelationMgr;
        public HashList<TUnit> Sents { get; private set; } = new HashList<TUnit>();
        public Dictionary<TUnit, CD> ChangeRelationShipCD { get; private set; } = new Dictionary<TUnit, CD>();
        public Dictionary<TUnit, CD> Provocative { get; private set; } = new Dictionary<TUnit, CD>();
        #endregion

        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            Sents.Clear();
            ChangeRelationShipCD.Clear();
            Provocative.Clear();
            SelfUnit.Callback_OnSetOwner += OnSetOwner;
        }
        public override void OnDisable()
        {
            SelfUnit.Callback_OnSetOwner -= OnSetOwner;
            base.OnDisable();
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            CalcNeighbor();
        }
        #endregion

        #region Manual Update
        //更新调整关系
        protected void ManualUpdateRelationShipCD()
        {
            foreach (var item in ChangeRelationShipCD)
                item.Value.Update();
        }
        //更新挑衅
        protected void ManualUpdateProvocativeCD()
        {
            clearProvocative.Clear();
            foreach (var item in Provocative)
            {
                item.Value.Update();
                if (item.Value.IsOver())
                    clearProvocative.Add(item.Key);
            }
            foreach (var item in clearProvocative)
                Provocative.Remove(item);
        }
        //更新发送缓存
        protected void ManualUpdateSents()
        {
            Sents.Clear();
        }
        #endregion

        #region set
        public virtual void Accept(TDBaseData data, bool b)
        {
            OnAccept(data, b);
            data.CastBaseUnit.DipMgr.OnBeAccept(data, b);
        }
        public void AddSent(TUnit other)
        {
            Sents.Add(other);
        }
        public void ResetChangeRelationCD(TUnit other, int cd)
        {
            if (!ChangeRelationShipCD.ContainsKey(other))
            {
                ChangeRelationShipCD.Add(other, new CD());
            }
            ChangeRelationShipCD[other].Reset(cd);
        }
        public void AddProvocative(TUnit other, int cd)
        {
            if (Provocative.ContainsKey(other))
            {
                Provocative[other].Reset(cd);
            }
            else
            {
                Provocative.Add(other, new CD(cd));
            }
        }
        public void RemoveProvocative(TUnit other)
        {
            Provocative.Remove(other);
        }
        public void ChangeNeighborRelationShip(int baseVal)
        {
            foreach (var item in Neighbor)
            {
                if (item == SelfUnit)
                    continue;
                int changeShip = baseVal;
                BaseGlobal.RelationMgr.ChangeRelationShip(SelfUnit, item, changeShip);
            }
        }
        #endregion

        #region get
        public int GetChangeRelationShipCD(TUnit other)
        {
            if (ChangeRelationShipCD.ContainsKey(other))
                return (int)ChangeRelationShipCD[other].CurCount;
            return 0;
        }
        public int GetRelationShip(TUnit target)
        {
            return RelationMgr.GetRelationShip(SelfUnit, target);
        }
        #endregion

        #region util
        protected virtual IDDicList<BaseUnit> CastleData => throw new NotImplementedException();
        public HashList<TUnit> Neighbor { get; protected set; } = new HashList<TUnit>();
        public HashList<TUnit> ExNeighbor { get; protected set; } = new HashList<TUnit>();
        public void CalcNeighbor()
        {
            Neighbor.Clear();
            ExNeighbor.Clear();
            foreach (var item in CastleData)
            {
                if (item.TerritoryMgr == null) continue;
                foreach (var castle in item.TerritoryMgr.Neighbours)
                {
                    if (castle.BaseOwner != SelfBaseUnit)
                    {
                        var temp = castle.BaseOwner as TUnit;
                        Neighbor.Add(temp);
                        ExNeighbor.Add(temp);
                        OnAddToNeighbor(temp);
                    }
                    //添加邻居的邻居
                    foreach (var exCastle in castle.TerritoryMgr.Neighbours)
                    {
                        if (exCastle.BaseOwner != SelfBaseUnit)
                        {
                            var temp = castle.BaseOwner as TUnit;
                            ExNeighbor.Add(temp);
                        }
                    }
                }
            }
        }
        #endregion

        #region is
        public bool IsInChangeRelationShipCD(TUnit other)
        {
            return GetChangeRelationShipCD(other) > 0;
        }
        public bool IsBeProvocative()
        {
            return Provocative.Count > 0;
        }
        public bool IsSent(TUnit other)
        {
            if (other == null) return false;
            if (SelfBaseUnit == other) return false;
            return Sents.Contains(other);
        }
        public bool IsInWar()
        {
            return RelationMgr.IsInWarfare(SelfBaseUnit);
        }
        public bool IsInProvocater(TUnit unit)
        {
            return Provocative.ContainsKey(unit);
        }
        public bool IsFriend(TUnit target)
        {
            return RelationMgr.IsFriend(SelfUnit, target);
        }
        public bool IsHaveWarFriend()
        {
            return IsHaveAlliance() || IsHaveVassal() || IsHaveSuzerain();
        }
        public bool IsHaveAlliance() => RelationMgr.IsHaveAlliance(SelfUnit);
        public bool IsHaveMarriage() => RelationMgr.IsHaveMarriage(SelfUnit);
        public bool IsHaveVassal() => RelationMgr.IsHaveVassal(SelfUnit);
        public bool IsHaveSuzerain() => RelationMgr.IsHaveSuzerain(SelfUnit);
        #endregion

        #region DB
        protected void LoadDBSent(ref List<string> db)
        {
            if (db == null) return;
            Sents.Clear();
            db.ForEach(x => Sents.Add(GetEntity<TUnit>(x)));
        }
        protected void LoadDBChangeRelationShipCD(ref Dictionary<long, CD> db)
        {
            if (db == null) return;
            db.ForEach((k, v) => ChangeRelationShipCD.Add(GetEntity<TUnit>(k), v));
        }
        protected void LoadDBProvocative(ref Dictionary<long, CD> db)
        {
            if (db == null) return;
            db.ForEach((k, v) => Provocative.Add(GetEntity<TUnit>(k), v));
        }
        protected void SaveDBSent(ref List<string> ret)
        {
            ret = new List<string>();
            var temp = ret;
            Sents.ForEach(x => temp.Add(x.TDID));
        }
        protected void SaveDBChangeRelationShipCD(ref Dictionary<long, CD> ret)
        {
            ret = new Dictionary<long, CD>();
            var temp = ret;
            ChangeRelationShipCD.ForEach((k, v) => temp.Add(k.ID, v));
        }
        protected void SaveDBProvocative(ref Dictionary<long, CD> ret)
        {
            ret = new Dictionary<long, CD>();
            var temp = ret;
            Provocative.ForEach((k, v) => temp.Add(k.ID, v));
        }
        #endregion

        #region Callback
        public virtual void OnChangeRelation(BaseUnit other)
        {
            Callback_OnChangeRelation?.Invoke(other);
        }
        public virtual void OnChangeRelationShip(BaseUnit other, int relShip)
        {
            Callback_OnChangeRelationShip?.Invoke(other, relShip);
        }
        public virtual void OnChangeAlliance(BaseUnit other, bool b)
        {
            Callback_OnChangeAlliance?.Invoke(other, b);
        }
        public virtual void OnChangeMarriage(BaseUnit other, bool b)
        {
            Callback_OnChangeMarriage?.Invoke(other, b);
        }
        public virtual void OnChangeSubsidiary(BaseUnit other, bool b)
        {
            Callback_OnChangeSubsidiary?.Invoke(other, b);
        }
        public virtual void OnChangeArmistice(BaseUnit other, int count)
        {
            Callback_OnChangeArmistice?.Invoke(other, count);
        }
        public virtual void OnAddToAttacker(object warData)
        {
            Callback_OnAddToAttacker?.Invoke(warData);
        }
        public virtual void OnAddToDefensers(object warData)
        {
            Callback_OnAddToDefensers?.Invoke(warData);
        }
        public virtual void OnRemoveFromWar(object warData)
        {
            Callback_OnRemoveFromWar?.Invoke(warData);
        }
        public virtual void OnDecalarWar(object warData, BaseUnit other)
        {
            Callback_OnDecalarWar?.Invoke(warData, other);
            RemoveProvocative(other as TUnit);
        }
        public virtual void OnBeDecalarWar(object warData, BaseUnit caster)
        {
            Callback_OnBeDecalarWar?.Invoke(warData, caster);
            RemoveProvocative(caster as TUnit);
        }
        public virtual void OnDeclarePeace(object warData, BaseUnit other)
        {
        }
        protected virtual void OnAddToNeighbor(TUnit unit)
        {

        }
        public virtual void OnBeAccept(TDBaseData data, bool b)
        {
            Callback_OnBeAccept?.Invoke(data, b);
        }
        public virtual void OnAccept(TDBaseData data, bool b)
        {
            Callback_OnAccept?.Invoke(data, b);
        }
        protected void OnSetOwner(BaseUnit arg1)
        {
            if (SelfUnit.TerritoryMgr == null)
                return;
            HashSet<BaseUnit> nations = new HashSet<BaseUnit>();
            nations.Add(SelfUnit.BaseOwner);
            foreach (var item in SelfUnit.TerritoryMgr.Neighbours)
            {
                if (!nations.Contains(item.BaseOwner))
                    nations.Add(item.BaseOwner);
            }
            foreach (var item in nations)
            {
                item.DipMgr.CalcNeighbor();
            }
        }
        #endregion
    }
}