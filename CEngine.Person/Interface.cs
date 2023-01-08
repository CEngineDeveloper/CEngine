//------------------------------------------------------------------------------
// Interface.cs
// Copyright 2022 2022/12/25 
// Created by CYM on 2022/12/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM.Person
{
    public interface ITDPersonData : ITDBaseData
    {
        #region Config
        string Theory { get; set; }
        string Civil { get; set; }
        //并非Key,而是翻译后的字符串
        string FirstName { get; set; }
        //并非Key,而是翻译后的字符串
        string LastName { get; set; }
        Gender Gender { get; set; }
        int Age { get; set; }
        AgeRange AgeRange { get; set; }
        Dictionary<PHIPart, string> HeadIcon { get; set; }
        Dictionary<PHIPart, string> ChildHeadIcon { get; set; }
        Dictionary<PHIPart, string> OldHeadIcon { get; set; }
        #endregion

        #region prop
        bool IsCelebrity { get; }
        TDBaseCivilData CivilData { get; }
        #endregion

        #region get
        Sprite GetPSprite(PHIPart part);
        AgeRange GetAgeRange();
        string GetAgeStr(bool haveAgeStr = true);
        Sprite GetStarIcon();
        #endregion
    }
    public interface IPersonTestMgr
    {
        void RandTestPerson();
        IList Data { get; }
    }
}