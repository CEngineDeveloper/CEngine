using System;
using UnityEngine;
namespace CYM.Diplomacy
{
    [Serializable]
    public class BaseWarfareData<TUnit> : IBase, IWarfareData where TUnit : BaseUnit
    {
        #region prop
        public IDDicList<TUnit> Attackers { get; protected set; } = new IDDicList<TUnit>();
        public IDDicList<TUnit> Defensers { get; protected set; } = new IDDicList<TUnit>();
        //标记哪些国家可以占领土地
        public HashList<TUnit> AllowLand { get; protected set; } = new HashList<TUnit>();

        public long ID { get; set; }
        public string TDID { get; set; }
        public int WarDay { get; set; } = 0;
        public float AttackersWarPoint { get; set; } = 0;
        public float DefensersWarPoint { get; set; } = 0;
        //是否为独立战争
        public bool IsIndependent { get; set; } = false;
        #endregion

        #region life
        public BaseWarfareData() { }
        public virtual void OnUpdate()
        {
            WarDay++;
        }
        public virtual void OnUpdateWarData()
        {
            //e.x. 统计双方的战斗力
            //AttackerInfantry = 0;
            //AttackerCavalry = 0;
            //AttackerHeavyWeapon = 0;
            //DefenderInfantry = 0;
            //DefenderCavalry = 0;
            //DefenderHeavyWeapon = 0;
            //foreach (var item in Attackers)
            //{
            //    AttackerInfantry += item.TitleMgr.InfantryLife;
            //    AttackerCavalry += item.TitleMgr.CavalryLife;
            //    AttackerHeavyWeapon += item.TitleMgr.HeavyWeaponsLife;
            //    AttackerCastleCount += item.CastleMgr.Count;
            //}
            //foreach (var item in Defensers)
            //{
            //    DefenderInfantry += item.TitleMgr.InfantryLife;
            //    DefenderCavalry += item.TitleMgr.CavalryLife;
            //    DefenderHeavyWeapon += item.TitleMgr.HeavyWeaponsLife;
            //    DefenderCastleCount += item.CastleMgr.Count;
            //}
        }
        #endregion

        #region is
        public bool IsInAttackers(TUnit nation)
        {
            if (Attackers.Contains(nation))
                return true;
            return false;
        }
        public bool IsIn(TUnit nation)
        {
            return IsInAttackers(nation) || IsInDefensers(nation);
        }
        public bool IsInDefensers(TUnit nation)
        {
            if (Defensers.Contains(nation))
                return true;
            return false;
        }
        public bool IsChief(TUnit nation)
        {
            return GetAttackerChief() == nation || GetDefenserChief() == nation;
        }
        public bool IsCanOccupy(TUnit nation)
        {
            if (IsChief(nation))
                return true;
            return AllowLand.Contains(nation);
        }
        #endregion

        #region set
        public void AddWarPoint(TUnit nation, float step = 0.1f)
        {
            if (IsInAttackers(nation))
            {
                AttackersWarPoint += step;
            }
            else if (IsInDefensers(nation))
            {
                DefensersWarPoint += step;
            }
        }
        public void AddToConvence(BaseUnit chief, BaseUnit unit)
        {
            if (IsInAttackers(chief as TUnit))
                AddAttacker(unit as TUnit);
            if (IsInDefensers(chief as TUnit))
                AddDefensers(unit as TUnit);
        }
        public void AddAttacker(TUnit nation)
        {
            Attackers.Add(nation);
        }
        public void AddDefensers(TUnit nation)
        {
            Defensers.Add(nation);
        }
        public void Remove(TUnit nation)
        {
            if (IsChief(nation))
                return;
            Attackers.Remove(nation);
            Defensers.Remove(nation);
        }
        public void AllowOccupy(TUnit unit)
        {
            if (AllowLand.Contains(unit))
                return;
            AllowLand.Add(unit);
        }
        #endregion

        #region get
        /// <summary>
        /// 获得战争分数的平衡点,fisrt-second
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public float GetWarPointBlance(TUnit first, TUnit second)
        {
            return GetWarPoint(first) - GetWarPoint(second);
        }
        /// <summary>
        /// 获得战争进度位置,-100%~100%
        /// </summary>
        /// <returns></returns>
        public float GetWarPointPercent()
        {
            return Mathf.Clamp(AttackersWarPoint - DefensersWarPoint, -100, 100) / 100;
        }
        /// <summary>
        /// 根据参展方获得战争进度
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetWarPointPercent(TUnit unit)
        {
            if (IsInAttackers(unit))
                return GetWarPointPercent();
            else
                return -GetWarPointPercent();
        }
        public string GetWarPointPercentStr(TUnit unit)
        {
            return UIUtil.PerCI(GetWarPointPercent(unit));
        }
        public float GetWarPoint(TUnit nation)
        {
            if (IsInAttackers(nation))
                return AttackersWarPoint;
            if (IsInDefensers(nation))
                return DefensersWarPoint;
            return 0;
        }
        public string GetWarName()
        {
            return BaseLangMgr.Get("Text_战争名称", GetAttackerChief().BaseConfig.GetName(), GetDefenserChief().BaseConfig.GetName());
        }
        public TUnit GetSelfChief(TUnit nation)
        {
            if (IsInAttackers(nation))
                return GetAttackerChief();
            else
                return GetDefenserChief();
        }
        public TUnit GetOppositeChief(TUnit nation)
        {
            if (IsInAttackers(nation))
                return GetDefenserChief();
            else
                return GetAttackerChief();
        }
        public TUnit GetAttackerChief()
        {
            if (Attackers.Count > 0)
                return Attackers[0];
            return null;
        }
        public TUnit GetDefenserChief()
        {
            if (Defensers.Count > 0)
                return Defensers[0];
            return null;
        }
        public string GetAllAttackerNames()
        {
            string ret = "";
            foreach (var item in Attackers)
            {
                ret += item.BaseConfig.GetName() + ",";
            }
            return ret.TrimEnd(",");
        }
        public string GetAllDefenderNames()
        {
            string ret = "";
            foreach (var item in Defensers)
            {
                ret += item.BaseConfig.GetName() + ",";
            }
            return ret.TrimEnd(",");
        }
        #endregion
    }
}