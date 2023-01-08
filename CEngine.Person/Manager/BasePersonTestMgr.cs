//------------------------------------------------------------------------------
// PersonTestMgr.cs
// Copyright 2019 2019/6/9 
// Created by CYM on 2019/6/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CYM.Person
{
    public class BasePersonTestMgr<TData> : BasePersonMgr<TData>,IPersonTestMgr
        where TData : TDBasePersonData, new()
    {
        #region prop
        IList IPersonTestMgr.Data => base.Data;
        #endregion

        #region life
        public override bool IsAddToGlobalSpawnerMgr => false;
        #endregion

        #region utile
        public void RandTestPerson()
        {
            Clear();
            for (int i = 0; i <= 100; ++i)
            {
                Rand(BaseGlobal.TDCivil.ListObjValues.Rand() as TDBaseCivilData);
            }
        }
        ITDPersonData Rand(TDBaseCivilData data)
        {
            var age = new List<AgeRange> { AgeRange.Adult, AgeRange.Middle, AgeRange.Old }.GetAge();
            return RandPerson(data, age, typeof(Gender).Rand<Gender>(), null);
        }
        #endregion
    }
}