//------------------------------------------------------------------------------
// BasePersonMgr.cs
// Copyright 2019 2019/5/15 
// Created by CYM on 2019/5/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CYM.Person
{
    public class BasePersonMgr<TData> : BaseTDSpawnMgr<TData> 
        where TData : TDBasePersonData, new()
    {
        #region Rand
        //随机产生一个人物
        public virtual TData RandPerson(
            TDBaseCivilData civilData, 
            int age,
            Gender gender = Gender.Male, 
            string lastName = null, 
            Func<TData, TData> onProcessInfo = null)
        {
            TData person = new TData();
            if (onProcessInfo != null)
            {
                person = onProcessInfo.Invoke(person);
            }
            person.SetAge(age);
            person.DoRand(civilData, gender, lastName);
            SpawnAdd(person,new UnitSpawnParam { tdid = person.GetTDID() });
            return person;
        }
        //从表格读取一个人物
        public virtual TData GeneratePerson(
            TData config, 
            Func<TData, TData> onProcessInfo = null)
        {
            TData person = config.Copy<TData>();
            if (onProcessInfo != null)
            {
                person = onProcessInfo.Invoke(person);
            }
            person.DoGenerate();
            SpawnAdd(person, new UnitSpawnParam { tdid = person.GetTDID() });
            UnBornPerson.Remove(config.TDID);
            return person;
        }
        #endregion

        #region Manual Update
        public HashList<string> UnBornPerson { get; private set; } = new HashList<string>();
        List<TData> ClearData = new List<TData>();
        public virtual void ManualUpdatePerson()
        {
            foreach (var item in Data)
            {
                item.Growup();
                //加入死亡列表
                if (RandUtil.Rand(item.GetDeathProp()))
                {
                    item.IsLive = false;
                    ClearData.Add(item);
                }
                if (item.IsLive)
                {
                    item.ManualUpdate();
                }
            }
            ClearData.ForEach(x => x.DoDeath());
        }
        #endregion

        #region Copy data
        protected void CopyToDBData(TDBasePersonData config,DBBasePerson data)
        {
            data.Theory = config.Theory ;
            data.Civil = config.Civil;
            data.FirstName = config.FirstName;
            data.LastName = config.LastName;
            data.Gender = config.Gender ;
            data.Age = config.Age;
            data.AgeRange  = config.AgeRange;
            data.HeadIcon = config.HeadIcon;
            data.ChildHeadIcon = config.ChildHeadIcon;
            data.OldHeadIcon = config.OldHeadIcon;
        }
        protected void CopyToConfig(DBBasePerson data, TDBasePersonData config)
        {
            config.Theory = data.Theory;
            config.Civil = data.Civil;
            config.FirstName = data.FirstName;
            config.LastName = data.LastName;
            config.Gender = data.Gender;
            config.Age = data.Age;
            config.AgeRange = data.AgeRange;
            config.HeadIcon = data.HeadIcon;
            config.ChildHeadIcon = data.ChildHeadIcon;
            config.OldHeadIcon = data.OldHeadIcon;
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            Data.Clear();
            UnBornPerson.Clear();
        }
        #endregion

        #region DB
        public void LoadDBUnBornPerson(ref List<string> data)
        {
            foreach (var item in data)
                UnBornPerson.Add(item); 
        }
        public void SaveDBUnBornPerson(ref List<string> data)
        {
            foreach (var item in UnBornPerson)
                data.Add(item);
        }
        #endregion
    }
}