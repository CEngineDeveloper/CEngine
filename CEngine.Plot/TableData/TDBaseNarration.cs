
using System;
using System.Collections.Generic;
using UnityEngine;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.Plot
{
    [Serializable]
    public class NarrationFragment : TDBaseData
    {
        /// <summary>
        /// 换页
        /// </summary>
        public bool IsNewPage { get; set; } = true;

        #region prop
        public int CurPage { get; set; } = 0;
        public int FragmentIndex { get; set; } = 0;
        public string NarrationId { get; set; }
        #endregion

        #region get
        public override string GetName()
        {
            return BaseLangMgr.Get(NarrationId);
        }
        public override string GetDesc(params object[] ps)
        {
            if (!Desc.IsInv())
                return BaseLangMgr.Get(Desc, ps);
            return BaseLangMgr.Get(NarrationId  + FragmentIndex, ps);
        }
        public override Sprite GetIcon()
        {
            if (!Icon.IsInv())
                return BaseGlobal.RsIcon.Get(Icon);
            var ret = BaseGlobal.RsIcon.Get(NarrationId + "_" + FragmentIndex, false);
            if(ret==null)
                ret = BaseGlobal.RsIcon.Get(NarrationId, false);
            return ret;
        }
        #endregion
    }
    [Serializable]
    public class TDBaseNarrationData : TDBaseData
    {
        #region prop
        public bool IsShowOnce { get; set; } = true;
        public string Music { get; set; } = SysConst.STR_Inv;
        public List<NarrationFragment> Fragments { get; set; } = new List<NarrationFragment>();
        #endregion

        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            int index = 0;
            foreach (var item in Fragments)
            {
                item.FragmentIndex = index;
                item.NarrationId = TDID;
                if (index == 0)
                    item.IsNewPage = true;
                if (item.IsNewPage && index > 0)
                {
                    item.CurPage++;
                }
                index++;
            }
        }
    }
}